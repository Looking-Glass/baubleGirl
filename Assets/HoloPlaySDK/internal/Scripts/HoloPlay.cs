//Copyright 2017 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace HoloPlaySDK
{
    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    public class HoloPlay : MonoBehaviour
    {
        /// <summary>
        /// Release version of the SDK. 
        /// Stored as a float to allow comparisons (i.e. is this version > 0.1f ?)
        /// </summary>
        public static readonly float version = 0.32f;

        /// <summary>
        /// The HoloPlay holds a HoloPlayConfig from which it reads 
        /// all the values required for lenticular-izing the resulting image.
        /// </summary>
        private static HoloPlayConfig config;
        public static HoloPlayConfig Config
        {
            get
            {
                if (config == null)
                {
                    InitializeConfig();
                }
                return config;
            }
            set { config = value; }
        }
        private readonly static string configFileName = "holoPlayConfig.json";
        private readonly static string configCompanyName = "Looking Glass Factory";
        private readonly static string configDirName = "holoPlaySDK_calibration";
        public static string configDrivePath = "";

        /// <summary>
        /// The size of the HoloPlay Capture. 
        /// Use this, rather than the transform's scale, to resize the HoloPlay Capture.
        /// </summary>
        [Range(0.001f, 200)]
        public float size = 10;
        /// <summary>
        /// The field of view of the HoloPlay.
        /// Only available in Perspective mode.
        /// </summary>
        [Range(1, 75)]
        public float fov = 10;
        /// <summary>
        /// The near clipping plane.
        /// Larger value = more distance *in front* of the focal plane is rendered.
        /// Use caution when using a high number for this,
        /// as objects too far in front or behind the focal plane will appear blurry and double-image.
        /// </summary>
        [Range(0f, 2f)]
        public float nearClip = 1f;
        /// <summary>
        /// The far clipping plane.
        /// Larger value = more distance *behind* the focal plane is rendered.
        /// Use caution when using a high number for this,
        /// as objects too far in front or behind the focal plane will appear blurry and double-image.
        /// </summary>
        [Range(0.01f, 3f)]
        public float farClip = 1.5f;
        /// <summary>
        /// Render in editor.
        /// When true, the lenticularization will happen even in edit-mode.
        /// For use in conjunction with the HoloPlayGameWindowMover.
        /// </summary>
        [SerializeField]
        public bool renderInEditor = true;

        /// <summary>
        /// Gizmo Color.
        /// For use in editor only, sets the color of the gizmo in scene view.
        /// </summary>
        public static float gizmoColor = 0.35f;
        public static float gizmoVisibility = 0.2f;

        //public fields that the HoloPlay must reference, 
        private RenderTexture rtMain;
        private RenderTexture rtFinal;
        private Material matFinal;
        private Camera cam;
        /// <summary>
        /// The Camera doing the rendering of the views.
        /// This camera moves around the focal pane, taking x number of renders 
        /// (where x is the number of views)
        /// </summary>
        /// <returns></returns>
        public Camera Cam
        {
            get { return cam; }
            private set { cam = value; }
        }
        private Camera camFinal;
        private int rtFinalSize = 2048;
        private int tilesX = 4;
        private int tilesY = 8;

        /// <summary>
        /// On View Render callback.
        /// This event fires once every time a view is rendered, just before the render happens.
        /// It passes an int which is the 0th indexed view being rendered (so from 0 to numViews-1).
        /// It fires one last time after the last render, passing the int numViews.
        /// </summary>
        public static Action<int> onViewRender;

        //the test textures
        [SerializeField]
        private Texture2D colorTestTex;
        [SerializeField]
        private Texture2D numTestTex;

        //to preface warnings and logs
        public static readonly string warningText = "[HoloPlay] ";

        //todo: perhaps remove
        public static Action onLoadConfig;

        /// <summary>
        /// is the HoloPlay done with its OnEnable actions?
        /// </summary>
        public bool isReady = false;

        private static HoloPlay main;
        /// <summary>
        /// Static ref to the currently active HoloPlay.
        /// There may only be one active at a time, so this will always return the active one.
        /// </summary>
        public static HoloPlay Main
        {
            get
            {
                if (main != null) return main;
                main = FindObjectOfType<HoloPlay>();
                return main;
            }
            private set { main = value; }
        }

        void OnEnable()
        {
            //setup the static ref
            Main = this;

            //setup the main camera: this is the child that pivots around the focal pane
            Transform holoPlayCamChild = transform.Find("HoloPlay Camera");
            if (holoPlayCamChild == null)
            {
                holoPlayCamChild = new GameObject("HoloPlay Camera", typeof (Camera)).transform;
                holoPlayCamChild.parent = transform;
                var lc = holoPlayCamChild.GetComponent<Camera>();
                lc.clearFlags = CameraClearFlags.Color;
                lc.backgroundColor = Color.black;
            }
            Cam = holoPlayCamChild.GetComponent<Camera>();
            Cam.transform.hideFlags = HideFlags.NotEditable;

            //setup final camera. this is the camera which does the final render
            camFinal = GetComponent<Camera>();
            if (camFinal == null) gameObject.AddComponent<Camera>();
            camFinal.useOcclusionCulling = false;
            // camFinal.allowHDR = false;
            // camFinal.allowMSAA = false;
            camFinal.cullingMask = 0;
            camFinal.clearFlags = CameraClearFlags.Nothing;
            camFinal.backgroundColor = Color.black;
            camFinal.orthographic = true;
            camFinal.orthographicSize = size * 0.5f;
            camFinal.nearClipPlane = 0;
            camFinal.farClipPlane = 0.001f;
            camFinal.hideFlags = HideFlags.HideInInspector;

            //setup material to post-process the final cam
            matFinal = new Material(Shader.Find("Hidden/HoloPlay/HoloPlay Final"));
            matFinal.mainTexture = rtFinal;

            //setup textures used in the tests
            if (colorTestTex == null)
                colorTestTex = (Texture2D) Resources.Load("HoloPlay_colorTestTex__");
            if (numTestTex == null)
                numTestTex = (Texture2D) Resources.Load("HoloPlay_numTestTex__");

            //enforce the one-active-at-a-time rule
            var otherHoloPlay = FindObjectsOfType<HoloPlay>();
            if (otherHoloPlay.Length > 0)
            {
                bool displayMsg = false;
                for (int i = 0; i < otherHoloPlay.Length; i++)
                {
                    if (otherHoloPlay[i].gameObject != gameObject)
                    {
                        otherHoloPlay[i].gameObject.SetActive(false);
                        displayMsg = true;
                    }
                }
                if (displayMsg)
                    Debug.LogWarning(warningText +
                        "Can only have one active HoloPlay at a time! disabling all others.");
            }

            //if the config isn't loaded yet, load it
            if (Config == null)
                InitializeConfig();
            else
                ForceCalibrationRefresh();

            isReady = true;
        }

        void OnDisable()
        {
            //destroy the rendertextues and materials we created
            if (!Application.isPlaying)
            {
                rtMain.Release();
                DestroyImmediate(rtMain);
            }
            rtFinal.Release();
            DestroyImmediate(rtFinal);
            DestroyImmediate(matFinal);
            isReady = false;
        }

        //the meat of the rendering is in here
        void LateUpdate()
        {
            float adjustedSize = GetAdjustedSize();
            //restore cameras to original state
            Cam.enabled = false;
            Cam.aspect = Config.screenW / Config.screenH;
            Cam.transform.position = transform.position + transform.forward * -adjustedSize;
            Cam.transform.localRotation = Quaternion.identity;
            Cam.nearClipPlane = Mathf.Max(adjustedSize - nearClip * size, 0.1f * size);
            Cam.farClipPlane = adjustedSize + farClip * size;
            Cam.fieldOfView = fov;
            Cam.orthographicSize = size * 0.5f;

            camFinal.enabled = true;
            camFinal.orthographicSize = size * 0.5f;

            //set the render texture based on if there's a test or not 
            switch ((int) Config.test)
            {
                case 0:
                    matFinal.mainTexture = rtFinal;
                    break;
                case 1:
                    matFinal.mainTexture = colorTestTex;
                    break;
                case 2:
                    matFinal.mainTexture = numTestTex;
                    break;
            }

            //****************************
            //if not rendering views
            //****************************
            if (!renderInEditor && !Application.isPlaying)
            {
                Cam.targetTexture = null;
                camFinal.enabled = false;
                Cam.enabled = true;
                HandleOffset(0, Config.verticalAngle);
                return;
            }

            //****************************
            //rendering views
            //****************************

            //handle loop
            for (int i = 0; i < Config.numViews; i++)
            {
                //reset render texture
                Cam.targetTexture = rtMain;

                //offset or rotation
                HandleOffset(GetAngleAtView(i), Config.verticalAngle);

                //broadcast the onViewRender action
                if (onViewRender != null && Application.isPlaying)
                    onViewRender(i);

                //actually render~!
                Cam.Render();

                //copy to fullsize rt
                int ri = (tilesX * tilesY) - i - 1;
                int x = (i % tilesX) * rtMain.width;
                int y = (ri / tilesX) * rtMain.height;
                Rect rtRect = new Rect(x, y, rtMain.width, rtMain.height);
                if (rtMain.IsCreated() && rtFinal.IsCreated())
                {
                    Graphics.SetRenderTarget(rtFinal);
                    GL.PushMatrix();
                    GL.LoadPixelMatrix(0, rtFinal.width, rtFinal.height, 0);
                    Graphics.DrawTexture(rtRect, rtMain);
                    GL.PopMatrix();
                }
            }

            //sending variables to the shader

            //pitch
            float screenInches = (float) Screen.width / Config.DPI;
            float newPitch = Config.pitch * screenInches;
            //account for tilt in measuring pitch horizontally
            newPitch *= Mathf.Cos(Mathf.Atan(1f / Config.slope));
            matFinal.SetFloat("pitch", newPitch);

            //tilt
            float newTilt = Config.screenH / (Config.screenW * Config.slope);
            matFinal.SetFloat("tilt", newTilt);

            //center
            matFinal.SetFloat("center", Config.center);

            //numViews
            //during color test, force views down to 2
            float newNumViews = (int) Config.test == 1 ? 4 : (int) Config.numViews;
            matFinal.SetFloat("numViews", newNumViews);

            //tiles
            matFinal.SetFloat("tilesX", tilesX);
            matFinal.SetFloat("tilesY", tilesY);

            //flip x
            matFinal.SetFloat("flipX", Config.flipImage);

            //broadcast the onViewRender action one last time, 
            //incase there is a need to change things before the final render
            if (onViewRender != null && Application.isPlaying)
                onViewRender((int) Config.numViews);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (renderInEditor || Application.isPlaying)
            {
                Graphics.Blit(matFinal.mainTexture, dest, matFinal);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }

        void ForceCalibrationRefresh()
        {
            tilesX = 4;
            tilesY = 8;
            rtFinalSize = 2048;
            if ((int) Config.numViews > 32)
            {
                tilesX = 8;
                tilesY = 16;
                rtFinalSize = 4096;
            }

            //setup render texture
            rtMain = new RenderTexture(rtFinalSize / tilesX, rtFinalSize / tilesY, 24)
            {
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false,
                useMipMap = false
            };
            rtMain.Create();
            rtFinal = new RenderTexture(rtFinalSize, rtFinalSize, 0)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false,
                useMipMap = false
            };
            rtFinal.Create();

            //if the config is already set, return out
            if (Screen.width == Config.screenW && Screen.height == Config.screenH)
                return;

            if (Application.isEditor)
            {
                // Debug.LogWarning(holoPlayWarning + "Set game window to 2048x1536 for proper aspect in editor");
                //todo: make the game window mover set the the resolution properly so we don't get this error
            }
            else
            {
                Screen.SetResolution((int) Config.screenW, (int) Config.screenH, true);
            }

            #if UNITY_EDITOR
            if (UnityEditor.PlayerSettings.defaultScreenWidth != (int) Config.screenW ||
                UnityEditor.PlayerSettings.defaultScreenHeight != (int) Config.screenH)
            {
                UnityEditor.PlayerSettings.defaultScreenWidth = (int) Config.screenW;
                UnityEditor.PlayerSettings.defaultScreenHeight = (int) Config.screenH;
            }
            #endif
        }

        float GetAngleAtView(float viewNum)
        {
            return -Config.viewCone * 0.5f + (float) viewNum / (Config.numViews - 1) * Config.viewCone;
        }

        void HandleOffset(float horizontalOffset, float verticalOffset)
        {
            float adjustedSize = GetAdjustedSize();

            //start from scratch
            Cam.ResetProjectionMatrix();
            Cam.transform.localRotation = Quaternion.identity;

            //orthographic or regular perspective
            if (Cam.orthographic)
            {
                Cam.transform.position = transform.position + transform.forward * -adjustedSize;
                Cam.transform.RotateAround(transform.position, transform.up, -horizontalOffset);
                return;
            }

            //perspective correction
            //imagine triangle from pivot center, to camera, to camera's ideal new position. 
            //offAngle is angle at the pivot center. solve for offsetX
            //tan(offAngle) = offX / camDist
            //offX = camDist * tan(offAngle)
            float offsetX = adjustedSize * Mathf.Tan(horizontalOffset * Mathf.Deg2Rad);
            float offsetY = adjustedSize * Mathf.Tan(verticalOffset * Mathf.Deg2Rad);
            Vector3 camPos = transform.position;
            camPos += transform.right * offsetX;
            camPos += transform.up * offsetY;
            camPos += transform.forward * -adjustedSize;
            Cam.transform.position = camPos;

            //create var for new cam projection matrix
            Matrix4x4 matrix = Cam.projectionMatrix;

            //to measure pane width, get the pivot corners and measure the length
            Vector3[] pivotCorners = GetFrustumCorners(Cam, adjustedSize);
            for (int i = 0; i < pivotCorners.Length; i++)
            {
                pivotCorners[i] = transform.InverseTransformDirection(pivotCorners[i]);
            }
            float paneWidth = pivotCorners[3].x - pivotCorners[0].x;
            float paneHeight = pivotCorners[1].y - pivotCorners[0].y;

            //offset in unity space is in terms of 1/2 pane width, 
            //i.e. offset of 1 moves the camera 1/2 pane width to the right, so account for that
            matrix[0, 2] = -2 * Mathf.Sign(transform.lossyScale.x) * offsetX / paneWidth;
            matrix[1, 2] = -2 * Mathf.Sign(transform.lossyScale.y) * offsetY / paneHeight;

            //tada
            Cam.projectionMatrix = matrix;
        }

        public static void SaveConfigToFile()
        {
            string drivePath = GetConfigDriveFullFilePath();
            string persistentPath = GetConfigPersistentPath();

            //never save test mode as being on.
            float testMode = Config.test;
            Config.test.Value = 0;
            string json = JsonUtility.ToJson(Config, true);
            Config.test.Value = testMode;

            if (drivePath != "")
                File.WriteAllText(drivePath, json);
            File.WriteAllText(persistentPath, json);
            Debug.Log(warningText + "Config saved!");

            #if UNITY_EDITOR
            if (UnityEditor.PlayerSettings.defaultScreenWidth != (int) Config.screenW ||
                UnityEditor.PlayerSettings.defaultScreenHeight != (int) Config.screenH)
            {
                UnityEditor.PlayerSettings.defaultScreenWidth = (int) Config.screenW;
                UnityEditor.PlayerSettings.defaultScreenHeight = (int) Config.screenH;
            }
            #endif
        }

        void OnDrawGizmos()
        {
            Color c = Color.white;
            float h = gizmoColor;
            float hInterval = 0.13f / (int) Config.numViews;
            for (int i = 0; i < (int) Config.numViews; i++)
            {
                HandleOffset(GetAngleAtView(i), Config.verticalAngle);

                c = Color.HSVToRGB((h += hInterval) % 1f, 1, 1);
                c.a = gizmoVisibility;
                Gizmos.color = c;
                DrawFrustum(Cam);
            }

            //bump up the a for the final lines
            c.a = gizmoVisibility * 3;

            //focal pane
            HandleOffset(0, Cam.orthographic ? 0f : Config.verticalAngle);
            var focalPaneCorners = GetFrustumCorners(Cam, GetAdjustedSize());

            //focal partial frame
            Gizmos.color *= new Color(1, 1, 1, 3);
            Gizmos.DrawLine(focalPaneCorners[0], focalPaneCorners[1]);
            Gizmos.DrawLine(focalPaneCorners[1], Vector3.Lerp(focalPaneCorners[1], focalPaneCorners[2], 0.1f));
            Gizmos.DrawLine(focalPaneCorners[2], Vector3.Lerp(focalPaneCorners[2], focalPaneCorners[1], 0.1f));
            Gizmos.DrawLine(focalPaneCorners[2], focalPaneCorners[3]);
            Gizmos.DrawLine(focalPaneCorners[3], Vector3.Lerp(focalPaneCorners[3], focalPaneCorners[0], 0.1f));
            Gizmos.DrawLine(focalPaneCorners[0], Vector3.Lerp(focalPaneCorners[0], focalPaneCorners[3], 0.1f));

            //resets the camera to where it should be after the gizmo drawing
            HandleOffset(0, Config.verticalAngle);
        }

        ///draws a camera frustum. less broken than the built-in unity version
        void DrawFrustum(Camera cam)
        {
            //get corners
            List<Vector3> frustumCorners = new List<Vector3>();
            frustumCorners.AddRange(GetFrustumCorners(cam, cam.nearClipPlane));
            frustumCorners.AddRange(GetFrustumCorners(cam, cam.farClipPlane));

            //draw near box
            Gizmos.DrawLine(frustumCorners[0], frustumCorners[1]);
            Gizmos.DrawLine(frustumCorners[1], frustumCorners[2]);
            Gizmos.DrawLine(frustumCorners[2], frustumCorners[3]);
            Gizmos.DrawLine(frustumCorners[3], frustumCorners[0]);

            //draw far box
            Gizmos.DrawLine(frustumCorners[0 + 4], frustumCorners[1 + 4]);
            Gizmos.DrawLine(frustumCorners[1 + 4], frustumCorners[2 + 4]);
            Gizmos.DrawLine(frustumCorners[2 + 4], frustumCorners[3 + 4]);
            Gizmos.DrawLine(frustumCorners[3 + 4], frustumCorners[0 + 4]);

            //connect them
            Gizmos.DrawLine(frustumCorners[0], frustumCorners[0 + 4]);
            Gizmos.DrawLine(frustumCorners[1], frustumCorners[1 + 4]);
            Gizmos.DrawLine(frustumCorners[2], frustumCorners[2 + 4]);
            Gizmos.DrawLine(frustumCorners[3], frustumCorners[3 + 4]);
        }

        ///returns the corners of the camera frustum as vector3s at dist
        public Vector3[] GetFrustumCorners(Camera cam, float dist)
        {
            //make sure the dist is actually within the camera's clipping area
            dist = Mathf.Clamp(dist, cam.nearClipPlane, cam.farClipPlane);

            //get corners
            Vector3[] frustumCorners = new []
            {
                cam.ViewportToWorldPoint(new Vector3(0, 0, dist)),
                cam.ViewportToWorldPoint(new Vector3(0, 1, dist)),
                cam.ViewportToWorldPoint(new Vector3(1, 1, dist)),
                cam.ViewportToWorldPoint(new Vector3(1, 0, dist))
            };

            return frustumCorners;
        }

        ///returns the cam size after adjustment for FOV. this is essentially the real cam distance from center.
        public float GetAdjustedSize()
        {
            return size * 0.5f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        }

        public static void InitializeConfig()
        {
            //check if there's a config in the data path
            string pdataConfigPath = GetConfigPersistentPath();
            bool pdataConfigExists = File.Exists(pdataConfigPath);

            //check if there's a config on the thumb drive
            string driveConfigPath = SearchDrivesForConfigPath();
            if (driveConfigPath != "" && File.Exists(driveConfigPath))
            {
                string driveConfig = File.ReadAllText(driveConfigPath);
                //if the data config exists, compare the two
                if (pdataConfigExists)
                {
                    string pdataConfig = File.ReadAllText(pdataConfigPath);

                    //if the files are the same, exit
                    if (driveConfig == pdataConfig)
                    {
                        LoadConfig(JsonUtility.FromJson<HoloPlayConfig>(driveConfig));
                        return;
                    }
                }

                //if the data config doesn't exist/isn't the same
                //write it with the values on the thumb drive
                File.WriteAllText(pdataConfigPath, driveConfig);
                LoadConfig(JsonUtility.FromJson<HoloPlayConfig>(driveConfig));
                return;
            }
            //if there's no config on the thumb drive
            //use the permanent config if it exists
            if (pdataConfigExists)
            {
                string pdataConfig = File.ReadAllText(pdataConfigPath);
                LoadConfig(JsonUtility.FromJson<HoloPlayConfig>(pdataConfig));
                return;
            }

            //if nothing at all found, make a new one
            LoadConfig(new HoloPlayConfig());
            return;
        }

        static void LoadConfig(HoloPlayConfig configIn)
        {
            Config = configIn;
            //make sure test value is always 0 unless specified by calibrator
            Config.test.Value = 0;
            if (onLoadConfig != null) onLoadConfig();
            HoloPlay.Main.ForceCalibrationRefresh();
        }

        /// <summary>
        /// This will return the path to the persistent calibration
        /// </summary>
        /// <returns></returns>
        public static string GetConfigPersistentPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, configCompanyName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, configDirName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, configFileName);
            return path;
        }

        /// <summary>
        /// Returns the file path for the config at ConfigDrivePath.
        /// If the config directory doesn't exist, one is created!
        /// </summary>
        /// <returns></returns>
        public static string GetConfigDriveFullFilePath()
        {
            if (configDrivePath == "")
                return "";
            string path = Path.Combine(configDrivePath, configDirName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, configFileName);
            return path;
        }

        /// <summary>
        /// This will return the path of the calibration file if it finds it on any of the drives.
        /// If not, it will return the string ""
        /// </summary>
        /// <returns></returns>
        public static string SearchDrivesForConfigPath()
        {
            foreach(var drive in GetAllDrives())
            {
                string path = Path.Combine(drive, configDirName);
                path = Path.Combine(path, configFileName);
                if (File.Exists(path)) return path;
                SetConfigDrivePath(drive);
            }
            return "";
        }

        /// <summary>
        /// Used to set the config drive path 
        /// </summary>
        public static void SetConfigDrivePath(string drivePath)
        {
            configDrivePath = drivePath;
        }

        public static string[] GetAllDrives()
        {
            List<string> drives = new List<string>();

            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            drives.AddRange(Directory.GetLogicalDrives());
            #else
            drives.AddRange(Directory.GetDirectories("/Volumes/"));
            #endif

            for (int i = 0; i < drives.Count;)
            {
                var d = drives[i];
                string testPath = Path.Combine(d, "testForHoloPlaySDK.txt");
                try
                {
                    File.WriteAllText(testPath, "test");
                }
                catch
                {
                    //if the write didn't succeed, remove this entry and continue
                    drives.RemoveAt(i);
                    continue;
                }
                //if it did succeed in writing, delete the test file and increment
                File.Delete(testPath);
                i++;
            }

            return drives.ToArray();
        }
    }
}