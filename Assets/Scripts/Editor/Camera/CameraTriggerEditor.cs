using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraTrigger))]
public class CameraTriggerEditor : Editor
{ 
    private SerializedObject m_So;
    
    private SerializedProperty sp_Configuration;
    private SerializedProperty sp_PanCamera;
    private SerializedProperty sp_SwapCamera;
    private SerializedProperty sp_OnEnterCamera;
    private SerializedProperty sp_OnExitCamera;

    private SerializedObject m_ConfigurationSO;

    private void OnEnable()
    {
        m_So = new SerializedObject(target);

        sp_Configuration = m_So.FindProperty("m_Configuration");
        sp_SwapCamera = m_So.FindProperty("SwapCameras");
        sp_PanCamera = m_So.FindProperty("PanCamera");
        sp_OnEnterCamera = m_So.FindProperty("OnEnterCamera");
        sp_OnExitCamera  = m_So.FindProperty("OnExitCamera");

        m_ConfigurationSO = new SerializedObject(sp_Configuration.objectReferenceValue);
    }

    public override void OnInspectorGUI()
    {
        m_So.Update();

        EditorGUILayout.PropertyField(sp_SwapCamera, new GUIContent("Swap Cameras"));
        EditorGUILayout.PropertyField(sp_PanCamera, new GUIContent("Pan Camera"));
        
        if (sp_SwapCamera.boolValue)
        {
            sp_PanCamera.boolValue = false;
            
            EditorGUILayout.PropertyField(sp_OnEnterCamera, new GUIContent("On Enter Camera"));
            EditorGUILayout.PropertyField(sp_OnExitCamera, new GUIContent("On Exit Camera"));
        }
    
        if (sp_PanCamera.boolValue)
        {
            sp_SwapCamera.boolValue = false;
            
            EditorGUILayout.PropertyField(sp_Configuration, new GUIContent("Pan Camera Configuration"));
            
            if (sp_Configuration == null)
            {
                EditorGUILayout.HelpBox("No Configuration for the trigger is set on game object", MessageType.Error);
                return;
            }
            
            var configRef = sp_Configuration.objectReferenceValue;

            if (m_ConfigurationSO == null || m_ConfigurationSO.targetObject != configRef)
            {
                m_ConfigurationSO = new SerializedObject(sp_Configuration.objectReferenceValue);
            }
            
            m_ConfigurationSO.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_ConfigurationSO.FindProperty("PanDirection"));
                EditorGUILayout.PropertyField(m_ConfigurationSO.FindProperty("PanDistance"));
                EditorGUILayout.PropertyField(m_ConfigurationSO.FindProperty("PanSpeed"));
            }

            m_ConfigurationSO.ApplyModifiedProperties();
        }
    
        m_So.ApplyModifiedProperties();
    }
}
