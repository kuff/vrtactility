using Tactility.Calibration;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TactilityDeviceConfig))]
    public class TactilityDeviceConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _deviceName;
        private SerializedProperty _numPads;
        private SerializedProperty _minAmp;
        private SerializedProperty _maxAmp;
        private SerializedProperty _minWidth;
        private SerializedProperty _maxWidth;
        private SerializedProperty _baseFreq;
        private SerializedProperty _minFreq;
        private SerializedProperty _maxFreq;
        private SerializedProperty _useSpecialAnodes;
        private SerializedProperty _anodes;

        // Foldout states
        private static bool _showBasicInfo = true;
        private static bool _showMinMaxValues = true;
        private static bool _showSpecialAnodes = true;

        private void OnEnable()
        {
            _deviceName = serializedObject.FindProperty("deviceName");
            _numPads = serializedObject.FindProperty("numPads");
            _minAmp = serializedObject.FindProperty("minAmp");
            _maxAmp = serializedObject.FindProperty("maxAmp");
            _minWidth = serializedObject.FindProperty("minWidth");
            _maxWidth = serializedObject.FindProperty("maxWidth");
            _baseFreq = serializedObject.FindProperty("baseFreq");
            _minFreq = serializedObject.FindProperty("minFreq");
            _maxFreq = serializedObject.FindProperty("maxFreq");
            _useSpecialAnodes = serializedObject.FindProperty("useSpecialAnodes");
            _anodes = serializedObject.FindProperty("anodes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Basic Information
            _showBasicInfo = EditorGUILayout.BeginFoldoutHeaderGroup(_showBasicInfo, "Basic Information");
            if (_showBasicInfo)
            {
                EditorGUILayout.PropertyField(_deviceName);
                EditorGUILayout.PropertyField(_numPads);
                EditorGUILayout.PropertyField(_useSpecialAnodes);
                EditorGUILayout.PropertyField(_baseFreq);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // MinMax Values
            _showMinMaxValues = EditorGUILayout.BeginFoldoutHeaderGroup(_showMinMaxValues, "MinMax Values");
            if (_showMinMaxValues)
            {
                EditorGUILayout.PropertyField(_minAmp);
                EditorGUILayout.PropertyField(_maxAmp);
                EditorGUILayout.PropertyField(_minWidth);
                EditorGUILayout.PropertyField(_maxWidth);
                EditorGUILayout.PropertyField(_minFreq);
                EditorGUILayout.PropertyField(_maxFreq);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Special Anodes Configuration
            _showSpecialAnodes = EditorGUILayout.BeginFoldoutHeaderGroup(_showSpecialAnodes, "Anode Configuration (Zero-based Indexing)");
            if (_showSpecialAnodes)
            {
                EditorGUI.indentLevel++;
                if (_anodes.arraySize > 0)
                {
                    for (var i = 0; i < _anodes.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(_anodes.GetArrayElementAtIndex(i), new GUIContent($"Anode {i}"));

                        // Add button - adds an element directly below the current one
                        if (GUILayout.Button("Add", GUILayout.MaxWidth(50)))
                        {
                            _anodes.InsertArrayElementAtIndex(i + 1);
                            break; // Break to avoid modifying the collection while iterating
                        }

                        // Remove button - removes the current element
                        if (GUILayout.Button("Remove", GUILayout.MaxWidth(60)))
                        {
                            _anodes.DeleteArrayElementAtIndex(i);
                            if (i < _anodes.arraySize - 1) {
                                // When not the last element, Unity duplicates the last element to the removed position, so delete again
                                _anodes.DeleteArrayElementAtIndex(i);
                            }
                            break; // Break to avoid modifying the collection while iterating
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    // Show an "Add Anode" button when the array is empty
                    if (GUILayout.Button("Add Anode", GUILayout.MaxWidth(100)))
                    {
                        _anodes.arraySize++;
                    }
                }
                EditorGUI.indentLevel--;
            }

            // Input validation example for numPads
            _numPads.intValue = Mathf.Max(0, _numPads.intValue); // Ensure numPads cannot go below 0

            // Anode value validation
            for (var i = 0; i < _anodes.arraySize; i++)
            {
                var anode = _anodes.GetArrayElementAtIndex(i);
                anode.intValue = Mathf.Clamp(anode.intValue, 0, _numPads.intValue - 1); // Ensure anode values are within valid range
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
