using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using HoloPlaySDK;

using UnityEditor;

using UnityEngine;

namespace HoloPlaySDK_UI
{
    public class HoloPlayConfigWindow : EditorWindow
    {
        FieldInfo[] configFields;
        HoloPlayConfig.ConfigValue[] configValues;
        float[] increments;
        Vector2 scrollPos = Vector2.zero;

        public static HoloPlayConfigWindow Instance { get; private set; }
        public static bool IsOpen { get { return Instance != null; } }

        [MenuItem("HoloPlay/Config %&w")]
        static void Init()
        {
            if (HoloPlayConfigWindow.Instance != null)
            {
                HoloPlayConfigWindow.Instance.Close();
            }
            else
            {
                if (FindObjectOfType<HoloPlay>() == null)
                {
                    Debug.LogWarning("[HoloPlay] Cannot edit the config without a HoloPlay in the scene!");
                    return;
                }
                HoloPlayConfigWindow window = EditorWindow.GetWindow<HoloPlayConfigWindow>();
                window.Show();
            }
        }

        void ResetReferences()
        {
            configFields = typeof (HoloPlayConfig).GetFields();
            List<FieldInfo> configFieldsList = new List<FieldInfo>();
            for (int i = 0; i < configFields.Length; i++)
            {
                if (configFields[i].FieldType == typeof (HoloPlayConfig.ConfigValue))
                {
                    configFieldsList.Add(configFields[i]);
                }
            }
            configFields = configFieldsList.ToArray();
            configValues = new HoloPlayConfig.ConfigValue[configFields.Length];
            for (int i = 0; i < configFields.Length; i++)
            {
                configValues[i] = (HoloPlayConfig.ConfigValue) configFields[i].GetValue(HoloPlay.Config);
            }
            increments = new float[configFields.Length];
            for (int i = 0; i < increments.Length; i++)
            {
                increments[i] = 0.01f;
            }
        }

        void OnEnable()
        {
            Instance = this;
            minSize = new Vector2(340, 540);
            titleContent = new GUIContent("HoloPlay Config");
            ResetReferences();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            Color gizc = Color.HSVToRGB(HoloPlay.gizmoColor, 1, EditorGUIUtility.isProSkin ? 1 : 0.5f);
            GUI.color = gizc;
            EditorGUILayout.LabelField("- Config -", EditorStyles.whiteMiniLabel);
            GUI.color = Color.white;

            GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.red : Color.Lerp(Color.red, Color.white, 0.5f);
            if (GUILayout.Button("save *", EditorStyles.miniButton, GUILayout.MaxWidth(100)))
            {
                HoloPlay.SaveConfigToFile();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox("save after making changes! otherwise the config edits you make here will " +
                "be erased on play/stop", MessageType.Warning);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < configValues.Length; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(configValues[i].name);
                //only add the increment if it's not an int
                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    configValues[i].Value += configValues[i].isInt ? 1 : increments[i];
                }
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    configValues[i].Value -= configValues[i].isInt ? 1 : increments[i];
                }
                if (!configValues[i].isInt)
                    increments[i] = EditorGUILayout.FloatField(increments[i], GUILayout.Width(EditorGUIUtility.fieldWidth));

                //continue with the default button
                if (GUILayout.Button("Default", EditorStyles.miniButton, GUILayout.Width(EditorGUIUtility.fieldWidth)))
                {
                    GUI.FocusControl(null);
                    configValues[i].Value = configValues[i].defaultValue;
                }
                EditorGUILayout.EndHorizontal();

                //slider
                EditorGUILayout.BeginHorizontal();
                float sliderVal = EditorGUILayout.Slider(
                    configValues[i].isInt ? (int) configValues[i].Value : configValues[i].Value,
                    configValues[i].isInt ? (int) configValues[i].min : configValues[i].min,
                    configValues[i].isInt ? (int) configValues[i].max : configValues[i].max
                );
                configValues[i].Value = sliderVal;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            if (HoloPlay.Main != null)
                EditorUtility.SetDirty(HoloPlay.Main);
        }
    }
}