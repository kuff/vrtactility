// Copyright (C) 2024 Peter Leth

#region
using Tactility.Task;
using UnityEditor;
using UnityEngine;
#endregion

namespace Editor
{
    [CustomEditor(typeof(GrabAndMoveScenario))]
    public class GrabAndMoveScenarioVisualizer : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            /*var scenario = (GrabAndMoveScenario)target;

            // Draw a line at target height
            Handles.color = Color.green;
            var startPosition = new Vector3(-5, scenario.targetPosition.y, 0);
            var endPosition = new Vector3(5, scenario.targetPosition.y, 0);
            Handles.DrawLine(startPosition, endPosition);

            // Create a slider to adjust the target height
            scenario.targetPosition = Handles.Slider(new Vector3(0f, scenario.targetPosition.y, 0f), Vector3.up);

            // Optionally display progress
            Handles.Label(new Vector3(0, scenario.targetPosition.y + 0.5f, 0), $"Target Height: {scenario.targetPosition}");

            // Save the changes made to the targetPosition
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }*/
        }
    }
}
