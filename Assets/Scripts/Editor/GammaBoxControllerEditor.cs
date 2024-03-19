// Copyright (C) 2024 Peter Leth

#region
using Tactility.Box;
using UnityEditor;
using UnityEngine;
#endregion

namespace Editor
{
    [CustomEditor(typeof(GammaBoxController))]
    public class GammaBoxControllerEditor : UnityEditor.Editor
    {
        private bool _showReadOnlyValues = true; // You can set the default state of the foldout here

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Cast the target to your class to access its fields
            var messenger = (GammaBoxController)target;

            // Section headline and foldout
            _showReadOnlyValues = EditorGUILayout.Foldout(_showReadOnlyValues, "Runtime Read-Only Values");
            if (!_showReadOnlyValues)
            {
                return;
            }

            // Set GUI enabled to false to make the fields read-only
            GUI.enabled = false;

            // Display the fields inside a box for better visual separation
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Real-Time Stim Box Data", EditorStyles.boldLabel); // Section headline
            EditorGUILayout.TextField("Port", messenger.Port ?? "N/A");
            EditorGUILayout.TextField("Battery", messenger.Battery ?? "N/A");
            EditorGUILayout.TextField("Voltage", messenger.Voltage ?? "N/A");
            EditorGUILayout.TextField("Current", messenger.Current ?? "N/A");
            EditorGUILayout.TextField("Temperature", messenger.Temperature ?? "N/A");
            EditorGUILayout.Toggle("Is Connected", messenger.IsConnected);
            EditorGUILayout.EndVertical();

            // Re-enable the GUI in case you add editable fields below
            GUI.enabled = true;
        }
    }
}
