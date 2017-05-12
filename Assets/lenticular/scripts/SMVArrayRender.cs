using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RtResolution
{
    _512x256_32maxViews,
    _512x512_16maxViews,
    _1024x512_32maxViews,
    _1024x1024_16maxViews
}

[Serializable]
public class SMVConfig
{
    [Serializable]
    public struct configValue
    {
        public readonly bool isInt;
        public float value;
        public readonly float defaultValue;
        public readonly float min;
        public readonly float max;
        public readonly string tooltip;
        public configValue(bool isInt, float value, float defaultValue, float min, float max, string tooltip = "")
        {
            this.isInt = isInt;
            this.value = value;
            this.defaultValue = defaultValue;
            this.min = min;
            this.max = max;
            this.tooltip = tooltip;
        }
    }

    //todo: finish the rest of the elements of the config

    public configValue numViews = new configValue(true, 12, 12, 1, 16);
    public configValue viewCone = new configValue(false, 90, 90, -90, 90);
    public configValue tilt = new configValue(false, 0, 0, -45, 45);
    public configValue pitch = new configValue(false, 10, 10, 1f, 100);
    public configValue offsetX = new configValue(true, 0, 0, -300, 300);
    public configValue offsetY = new configValue(true, 0, 0, -100, 100);
    public configValue pitchOffsetX = new configValue(false, 0, 0, -1, 1);
    public configValue headerSize = new configValue(false, 1 / 6f, 1 / 6f, 0, 1);
    public configValue blending = new configValue(false, 0, 0, 0, 1);
    public configValue uselessP = new configValue(false, 0, 0, 0, 100);
    //some dpi ref:
    //my asus: 166.054
    //ipad mini 4: 326
    //alvin screen: 265
    public configValue DPI = new configValue(false, 326, 326, 1, 500);
    public bool flipX = false;
    public bool colorTest = false;
    public bool numTest = false;
    public RtResolution rtResolution = RtResolution._512x256_32maxViews;
    public int tilesX;
    public int tilesY;

    public void SetValue(string valueName, float value)
    {
        var fi = GetType().GetField(valueName);
        configValue newConfigValue = (configValue)fi.GetValue(this);
        newConfigValue.value = value;
        if (newConfigValue.isInt)
        {
            newConfigValue.value = Mathf.Round(newConfigValue.value);
        }
        fi.SetValue(this, newConfigValue);
    }

    public float GetValue(string valueName)
    {
        var fi = GetType().GetField(valueName);
        var configVal = (configValue)fi.GetValue(this);
        return configVal.value;
    }

    public configValue GetFullConfigValue(string valueName)
    {
        var fi = GetType().GetField(valueName);
        var configVal = (configValue)fi.GetValue(this);
        return configVal;
    }

    public void SetupMaxViews()
    {
        int currView = (int)numViews.value;
        switch (rtResolution)
        {
            case RtResolution._512x256_32maxViews:
            case RtResolution._1024x512_32maxViews:
                currView = Mathf.Clamp(currView, 1, 32);
                numViews = new configValue(true, currView, 12, 1, 32);
                break;
            case RtResolution._512x512_16maxViews:
            case RtResolution._1024x1024_16maxViews:
                currView = Mathf.Clamp(currView, 1, 16);
                numViews = new configValue(true, currView, 12, 1, 16);
                break;
        }
    }
}

[ExecuteInEditMode]
public class SMVArrayRender : MonoBehaviour
{
    [Header("- Config -")]
    //trying something very new here.
    public SMVConfig config;
    [Range(0.001f, 200)]
    public float smvCamRadius = 10;
    [Range(0.001f, 90)]
    public float smvCamFov = 10;
    [Range(0, 1)]
    public float smvCamNearClip = 0.5f;
    [Range(1, 100)]
    public float smvCamFarClip = 2;
    [Tooltip("Click this to save the config file values from the editor. This will overwrite the ones set in playmode.")]
    public bool saveConfig;
    [Tooltip("Click this to load the config file values from the editor. This is useful if you want to apply the changes made in play mode to the editor.")]
    public bool loadConfig;

    [Header("- Editor Only -")]
    [Range(0, 1)]
    public float gizmoColor;
    [Range(0, 1)]
    public float gizmoVisibility;
    public bool previewActiveView;
    [Range(0, 32)]
    public int activeView;

    [Header("- Rt -")]
    public RenderTexture rt2048;
    public RenderTexture rt4096;
    Transform camPivot;
    Camera cam;
    SMVFinalRender smvFinal;

    [Header("- Debug -")]
    public Texture2D colorTest;
    public Texture2D numTest;
    [Range(-2, 2)]
    public float blah;

    // Use this for initialization
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        camPivot = cam.transform.parent;
        smvFinal = GetComponentInChildren<SMVFinalRender>();
        FindObjectOfType<SMVHeader>().LoadConfig();
        SetupResolution();
        smvFinal.camMat.SetTexture("_ColorTest", colorTest);
        smvFinal.camMat.SetTexture("_NumTest", numTest);
    }

    void OnValidate()
    {
        //as the config values are changed in the inspector, keep them clamped.
        //this might be irrelevant if i completely remove the ability to change these values from inspector
        foreach (var fieldInfo in config.GetType().GetFields())
        {
            if (fieldInfo.FieldType != typeof(SMVConfig.configValue))
            {
                continue;
            }
            var configField = (SMVConfig.configValue)fieldInfo.GetValue(config);
            configField.value = Mathf.Clamp(configField.value, configField.min, configField.max);
            if (configField.isInt)
            {
                configField.value = Mathf.Round(configField.value);
            }
            fieldInfo.SetValue(config, configField);
        }
        if (loadConfig)
        {
            loadConfig = false;
            FindObjectOfType<SMVHeader>().LoadConfig();
            print("Config loaded!");
        }
        if (saveConfig)
        {
            saveConfig = false;
            FindObjectOfType<SMVHeader>().SaveConfig();
            print("Config saved!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.localPosition = Vector3.back * Mathf.Abs(smvCamRadius);

        cam.nearClipPlane = smvCamRadius * smvCamNearClip;
        cam.farClipPlane = smvCamRadius * smvCamFarClip;
        cam.fieldOfView = smvCamFov;

        //editor only. choose which view to preview
        activeView = Mathf.Clamp(activeView, 0, (int)config.numViews.value - 1);
        float activeAngle = 0f;

        smvFinal.enabled = false;
        if (config.rtResolution == RtResolution._512x256_32maxViews ||
            config.rtResolution == RtResolution._512x512_16maxViews)
        {
            cam.targetTexture = rt2048;
        }
        else
        {
            cam.targetTexture = rt4096;
        }
        float aspect = Screen.width / (Screen.height * (1 - config.headerSize.value));
        for (int i = 0; i < config.numViews.value; i++)
        {
            //change angle
            if (config.numViews.value > 1)
            {
                float angle = -config.viewCone.value * 0.5f + (float)i / (config.numViews.value - 1) * config.viewCone.value;
                camPivot.localEulerAngles = Vector3.up * angle;
                if (i == activeView) activeAngle = angle;
            }

            //setup and shoot for the tile
            float x = i % config.tilesX;
            float y = i / config.tilesX;
            cam.rect = new Rect(x / config.tilesX, y / config.tilesY, 1f / config.tilesX, 1f / config.tilesY);
            cam.aspect = aspect;
            cam.Render();
        }
        smvFinal.enabled = enabled;
        cam.targetTexture = null;
        cam.rect = new Rect(0, 0, 1, 1 - config.headerSize.value);
        cam.aspect = aspect;
        camPivot.localEulerAngles = previewActiveView ? Vector3.up * activeAngle : Vector3.zero;

        //set all shader values before the final render
        float numViews = config.colorTest ? 2 : (int)config.numViews.value;
        smvFinal.camMat.SetFloat("numViews", numViews);
        float newTilt = Mathf.Tan(config.tilt.value * Mathf.Deg2Rad) * (1 / cam.aspect);
        smvFinal.camMat.SetFloat("tilt", newTilt);
        //pitch = 1 should mean 1 lpi
        float newPitch = config.pitch.value * Screen.width / config.DPI.value;
        newPitch *= Mathf.Cos(config.tilt.value * Mathf.Deg2Rad);
        smvFinal.camMat.SetFloat("pitch", newPitch);
        smvFinal.camMat.SetInt("flipX", config.flipX ? 1 : 0);
        smvFinal.camMat.SetInt("offsetX", (int)config.offsetX.value);
        smvFinal.camMat.SetInt("offsetY", (int)config.offsetY.value);
        smvFinal.camMat.SetFloat("pitchOffsetX", config.pitchOffsetX.value);
        smvFinal.camMat.SetFloat("blending", config.blending.value);
        smvFinal.camMat.SetFloat("uselessP", config.uselessP.value / 100f);
        smvFinal.camMat.SetInt("colorTest", config.colorTest ? 1 : 0);
        smvFinal.camMat.SetInt("numTest", config.numTest ? 1 : 0);
        smvFinal.camMat.SetFloat("blah", blah);
    }

    public void SetupResolution()
    {
        switch (config.rtResolution)
        {
            case RtResolution._512x256_32maxViews:
                smvFinal.rt = rt2048;
                config.tilesX = 4;
                config.tilesY = 8;
                break;
            case RtResolution._1024x512_32maxViews:
                smvFinal.rt = rt4096;
                config.tilesX = 4;
                config.tilesY = 8;
                break;
            case RtResolution._512x512_16maxViews:
                smvFinal.rt = rt2048;
                config.tilesX = 4;
                config.tilesY = 4;
                break;
            case RtResolution._1024x1024_16maxViews:
                smvFinal.rt = rt4096;
                config.tilesX = 4;
                config.tilesY = 4;
                break;
        }
        smvFinal.camMat.SetFloat("tilesX", config.tilesX);
        smvFinal.camMat.SetFloat("tilesY", config.tilesY);
        config.SetupMaxViews();
    }

    void OnDrawGizmos()
    {
        if (cam == null)
            cam = GetComponentInChildren<Camera>();
        if (camPivot == null)
            camPivot = cam.transform.parent;

        float h = gizmoColor;
        float hInterval = 0.1f / config.numViews.value;
        Quaternion tempRot = camPivot.rotation;
        for (int i = 0; i < config.numViews.value; i++)
        {
            if (config.numViews.value > 1)
            {
                float angle = -config.viewCone.value * 0.5f + (float)i / (config.numViews.value - 1) * config.viewCone.value;
                camPivot.localEulerAngles = Vector3.up * angle;
            }
            Color c = Color.HSVToRGB((h += hInterval) % 1f, 1, 1);
            Gizmos.color = new Color(c.r, c.g, c.b, gizmoVisibility);
            if (!cam.orthographic)
            {
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
                Gizmos.matrix = temp;
            }
            else
            {
                List<Vector3> camCorners = new List<Vector3>();
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        camCorners.Add(cam.ScreenToWorldPoint(new Vector3(x * cam.pixelWidth, y * cam.pixelHeight, cam.nearClipPlane)));
                        camCorners.Add(cam.ScreenToWorldPoint(new Vector3(x * cam.pixelWidth, y * cam.pixelHeight, cam.farClipPlane)));
                    }
                }
                //near square, clockwise from bottom left
                Gizmos.DrawLine(camCorners[0], camCorners[4]);
                Gizmos.DrawLine(camCorners[4], camCorners[6]);
                Gizmos.DrawLine(camCorners[6], camCorners[2]);
                Gizmos.DrawLine(camCorners[2], camCorners[0]);

                //far square, clockwise from bottom left
                Gizmos.DrawLine(camCorners[0 + 1], camCorners[4 + 1]);
                Gizmos.DrawLine(camCorners[4 + 1], camCorners[6 + 1]);
                Gizmos.DrawLine(camCorners[6 + 1], camCorners[2 + 1]);
                Gizmos.DrawLine(camCorners[2 + 1], camCorners[0 + 1]);

                //connecting lines
                Gizmos.DrawLine(camCorners[0], camCorners[1]);
                Gizmos.DrawLine(camCorners[2], camCorners[3]);
                Gizmos.DrawLine(camCorners[4], camCorners[5]);
                Gizmos.DrawLine(camCorners[6], camCorners[7]);
            }
        }
        camPivot.rotation = tempRot;
    }
}
