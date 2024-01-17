using Internal;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(StimBoxMessenger))]
    public class StimBoxMessengerEditor : UnityEditor.Editor
    {
        private bool _showReadOnlyValues = true; // You can set the default state of the foldout here

        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Cast the target to your class to access its fields
            var messenger = (StimBoxMessenger)target;

            // Section headline and foldout
            _showReadOnlyValues = EditorGUILayout.Foldout(_showReadOnlyValues, "Runtime Read-Only Values");
            if (_showReadOnlyValues)
            {
                // Set GUI enabled to false to make the fields read-only
                GUI.enabled = false;

                // Display the fields inside a box for better visual separation
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Connection Info", EditorStyles.boldLabel); // Section headline
                EditorGUILayout.TextField("Port", messenger.Port ?? "N/A");
                EditorGUILayout.TextField("Battery", messenger.Battery ?? "N/A");
                EditorGUILayout.Toggle("Is Connected", messenger.IsConnected);
                EditorGUILayout.EndVertical();

                // Re-enable the GUI in case you add editable fields below
                GUI.enabled = true;
            }
            
            // if (Application.isPlaying) // Only do this while the game is running
            // {
            //     EditorUtility.SetDirty(target); // Mark the object as dirty to ensure the inspector updates
            //     Repaint(); // Request immediate repaint
            // }

            // Add any additional custom UI elements here
        }
    }
}