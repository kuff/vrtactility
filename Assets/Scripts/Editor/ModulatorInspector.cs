using Tactility.Modulation;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(AbstractModulator), true)]
    public class ModulatorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector options
            base.OnInspectorGUI();
        
            // Cast the target of this inspector to AbstractModulator
            var modulator = (AbstractModulator)target;

            // Check if the ITactilityDataProvider component exists on the same GameObject
            if (modulator.GetComponent<ITactilityDataProvider>() == null)
            {
                // Display a warning if ITactilityDataProvider is missing
                EditorGUILayout.HelpBox("Warning: No ITactilityDataProvider component found on this GameObject. " +
                                        "It is often needed for AbstractModulator derivatives.", MessageType.Warning);
            }
        }
    }
}