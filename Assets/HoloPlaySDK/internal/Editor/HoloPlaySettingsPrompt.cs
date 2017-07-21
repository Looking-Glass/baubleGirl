using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class HoloPlaySettingsPrompt : EditorWindow
{
    // public static HoloPlaySettingsPrompt Instance { get; private set; }
    // public static bool IsOpen { get { return Instance != null; } }

    class setting
    {
        public string label;
        public bool on;
        public setting(string label, bool on)
        {
            this.label = label;
            this.on = on;
        }
    }
    List<setting> settings;
    static string editorPrefName = "HoloPlay Proj Settings";
    BuildTarget[] buildPlatforms = new []
    {
        BuildTarget.StandaloneOSXIntel,
        BuildTarget.StandaloneOSXIntel64,
        BuildTarget.StandaloneOSXUniversal
    };
    GraphicsDeviceType[] graphicsAPIs = new []
    {
        GraphicsDeviceType.OpenGLCore,
        GraphicsDeviceType.Metal
    };

    static HoloPlaySettingsPrompt()
    {
        if (!EditorPrefs.GetBool(editorPrefName, false))
        {
            Init();
        }
    }

    [MenuItem("HoloPlay/Optimize Project Settings")]
    static void Init()
    {
        HoloPlaySettingsPrompt window = EditorWindow.GetWindow<HoloPlaySettingsPrompt>();
        window.Show();
    }

    void OnEnable()
    {
        titleContent = new GUIContent("HoloPlay Settings");

        settings = new List<setting>
        {
            new setting("Shadows: Hard Only", true),
            new setting("Shadow Projection: Close Fit", true),
            new setting("Shadow Distance: 1000", true),
            new setting("Shadow Cascades: 0", true),
            new setting("vSync: off", true),
            new setting("macOS Graphics API: OpenGLCore", true)
        };

        Vector2 size = new Vector2(360, 230);
        maxSize = size;
        minSize = size;
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "It is recommended you change the following project settings " +
            "to ensure the best performace for your HoloPlay application",
            MessageType.Warning
        );
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Select which options to change:", EditorStyles.miniLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        foreach(var s in settings)
        {
            EditorGUILayout.BeginHorizontal();
            s.on = EditorGUILayout.ToggleLeft(s.label, s.on);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        // GUILayout.FlexibleSpace();
        GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.green : Color.Lerp(Color.green, Color.white, 0.5f);
        if (GUILayout.Button("Apply Changes"))
        {
            var qs = QualitySettings.names;
            int currentQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < qs.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);

                if (settings[0].on)
                    QualitySettings.shadows = ShadowQuality.HardOnly;

                if (settings[1].on)
                    QualitySettings.shadowProjection = ShadowProjection.CloseFit;

                if (settings[2].on)
                    QualitySettings.shadowDistance = 1000;

                if (settings[3].on)
                    QualitySettings.shadowCascades = 0;

                if (settings[4].on)
                    QualitySettings.vSyncCount = 0;
            }

            //graphics api settings
            if (settings[5].on)
            {
                foreach(var b in buildPlatforms)
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(b, false);
                    PlayerSettings.SetGraphicsAPIs(b, graphicsAPIs);
                    if (EditorUserBuildSettings.activeBuildTarget == b)
                    {
                        Debug.LogWarning("For graphics API switch to take effect, a project re-open is required");
                    }
                }
            }
            QualitySettings.SetQualityLevel(currentQuality, true);
            EditorPrefs.SetBool(editorPrefName, true);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }
}