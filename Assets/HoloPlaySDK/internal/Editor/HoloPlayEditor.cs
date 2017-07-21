using System.Collections;
using System.Collections.Generic;

using HoloPlaySDK;

using UnityEditor;

using UnityEngine;

namespace HoloPlaySDK_UI
{
    [CustomEditor(typeof (HoloPlay))]
    public class HoloPlayEditor : Editor
    {
        SerializedProperty size;
        SerializedProperty fov;
        SerializedProperty nearClip;
        SerializedProperty farClip;
        SerializedProperty renderInEditor;

        void OnEnable()
        {
            size = serializedObject.FindProperty("size");
            fov = serializedObject.FindProperty("fov");
            nearClip = serializedObject.FindProperty("nearClip");
            farClip = serializedObject.FindProperty("farClip");
            renderInEditor = serializedObject.FindProperty("renderInEditor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Color gizc = Color.HSVToRGB(HoloPlay.gizmoColor, 1, EditorGUIUtility.isProSkin ? 1 : 0.5f);
            HoloPlay holoPlay = ((HoloPlay) target);

            EditorGUILayout.Space();

            GUI.color = Color.HSVToRGB(HoloPlay.gizmoColor, 0.3f, 1);
            EditorGUILayout.LabelField("HoloPlay", EditorStyles.centeredGreyMiniLabel);
            GUI.color = Color.white;

            GUI.color = gizc;
            EditorGUILayout.LabelField("- Camera -", EditorStyles.whiteMiniLabel);
            GUI.color = Color.white;

            EditorGUILayout.PropertyField(size);
            EditorGUILayout.PropertyField(nearClip);
            EditorGUILayout.PropertyField(farClip);

            //fix for if the user is inspecting the prefab
            if (holoPlay.Cam == null)
                return;

            EditorGUILayout.BeginHorizontal();
            bool ortho = holoPlay.Cam.orthographic;
            EditorGUILayout.LabelField(new GUIContent("Projection"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                    ortho ? "orthographic" : "perspective",
                    EditorStyles.miniButton,
                    GUILayout.MaxWidth(EditorGUIUtility.fieldWidth * 2)))
            {
                ortho = !ortho;
                holoPlay.Cam.orthographic = ortho;
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = !ortho;
            EditorGUILayout.PropertyField(fov);
            GUI.enabled = true;

            EditorGUILayout.Space();

            GUI.color = gizc;
            EditorGUILayout.LabelField("- Preview -", EditorStyles.whiteMiniLabel);
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Render in editor", "Renders with the lenticular postprocess shader while in edit mode"));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(renderInEditor.boolValue ? "on" : "off", EditorStyles.miniButton, GUILayout.MaxWidth(EditorGUIUtility.fieldWidth)))
            {
                renderInEditor.boolValue = !renderInEditor.boolValue;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/HoloPlay/HoloPlay Capture", false, 10)]
        public static void CreateHoloPlay()
        {
            var go = new GameObject("HoloPlay Capture", typeof (HoloPlay));
            go.AddComponent<AudioListener>();
            var realsenseChild = new GameObject("Realsense Calibration");
            realsenseChild.transform.SetParent(go.transform, false);
            realsenseChild.AddComponent<depthPlugin>();
            realsenseChild.AddComponent<RealsenseCalibrator>();
        }
    }
}