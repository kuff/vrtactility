using Internal.Ball;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GrabAndMoveScenario))]
    public class GrabAndMoveScenarioVisualizer : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var scenario = (GrabAndMoveScenario)target;

            // Draw a line at target height
            Handles.color = Color.green;
            var startPosition = new Vector3(-5, scenario.targetHeight, 0);
            var endPosition = new Vector3(5, scenario.targetHeight, 0);
            Handles.DrawLine(startPosition, endPosition);

            // Create a slider to adjust the target height
            scenario.targetHeight = Handles.Slider(new Vector3(0, scenario.targetHeight, 0), Vector3.up).y;

            // Optionally display progress
            Handles.Label(new Vector3(0, scenario.targetHeight + 0.5f, 0), $"Target Height: {scenario.targetHeight}");

            // Save the changes made to the targetHeight
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}