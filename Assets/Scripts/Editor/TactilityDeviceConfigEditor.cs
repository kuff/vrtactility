// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using Tactility.Calibration;
using UnityEditor;
using UnityEngine;
#endregion

namespace Editor
{
    [CustomEditor(typeof(TactilityDeviceConfig))]
    public class TactilityDeviceConfigEditor : UnityEditor.Editor
    {

        // Foldout states
        private static bool _showBasicInfo = true;
        private static bool _showMinMaxValues = true;
        private static bool _showSpecialAnodes = true;
        private static bool _showMapping = true;
        private SerializedProperty _anodes;
        private SerializedProperty _baseFreq;
        private SerializedProperty _deviceName;
        private SerializedProperty _mapping;

        private bool _mappingEnabled;
        private SerializedProperty _maxAmp;
        private SerializedProperty _maxFreq;
        private SerializedProperty _maxWidth;
        private SerializedProperty _minAmp;
        private SerializedProperty _minFreq;
        private SerializedProperty _minWidth;
        private SerializedProperty _numPads;
        private SerializedProperty _useSpecialAnodes;

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
            _mapping = serializedObject.FindProperty("mapping");

            _mappingEnabled = serializedObject.FindProperty("mapping").arraySize > 0;
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
                            if (i < _anodes.arraySize - 1)
                            {
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

            EditorGUILayout.EndFoldoutHeaderGroup();

            // Mapping Configuration
            _showMapping = EditorGUILayout.BeginFoldoutHeaderGroup(_showMapping, "Mapping Configuration");
            if (_showMapping)
            {
                EditorGUI.indentLevel++;

                // Toggle to enable/disable mapping
                EditorGUI.BeginChangeCheck();
                _mappingEnabled = EditorGUILayout.Toggle("Enable Mapping", _mappingEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_mappingEnabled)
                    {
                        // Enable mapping: populate the array based on numPads
                        var numPads = _numPads.intValue;
                        _mapping.arraySize = numPads;
                        for (var i = 0; i < numPads; i++)
                        {
                            _mapping.GetArrayElementAtIndex(i).intValue = i + 1; // Default mapping
                        }
                    }
                    else
                    {
                        // Disable mapping: clear the array
                        _mapping.arraySize = 0;
                    }
                }

                if (_mappingEnabled && _mapping.arraySize > 0)
                {
                    var referencedIndices = new HashSet<int>();
                    for (var i = 0; i < _mapping.arraySize; i++)
                    {
                        var mapElement = _mapping.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(mapElement, new GUIContent($"Pad {i + 1} Mapping"));
                        referencedIndices.Add(mapElement.intValue);

                        // Validate the mapping index
                        if (mapElement.intValue <= 0 || mapElement.intValue > _numPads.intValue)
                        {
                            EditorGUILayout.HelpBox($"Mapping index {mapElement.intValue} for pad {i} is out of valid range [1, {_numPads.intValue}].", MessageType.Error);
                        }
                    }

                    // Check for unreferenced indices
                    for (var i = 1; i <= _numPads.intValue; i++)
                    {
                        if (!referencedIndices.Contains(i))
                        {
                            EditorGUILayout.HelpBox($"Pad index {i} is not referenced in the mapping. Ensure all pad indices are included.", MessageType.Warning);
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
