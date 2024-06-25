using UnityEngine;
using static Tactility.Calibration.CalibrationManager;
using static Tactility.Calibration.Interface.PadCalibrationManager;

namespace Tactility.Calibration.Interface
{
    // NOTE: This script is not used in the current version of the application
    public class SaveButtonColorChangeScript : MonoBehaviour
    {
        private static bool _isEnabled;
        
        private void OnEnable()
        {
            CalibrationValuesChanged += () => UpdateButtonColor(true);
            CalibrationDataSaved += _ => UpdateButtonColor(false);
        }

        private void Start()
        {
            var interfaceManager = FindObjectOfType<InterfaceManager>();
            interfaceManager.OnSceneChanged += (_, _) => UpdateButtonColor(_isEnabled);
        }

        private static void UpdateButtonColor(bool state)
        {
            _isEnabled = state;
            
            // Find all objects with the tag SaveButton and enable/disable interactivity and change color (yellow = true, white = false) based on the value of state
            var saveButtons = GameObject.FindGameObjectsWithTag("SaveButton");
            foreach (var saveButton in saveButtons)
            {
                saveButton.GetComponent<UnityEngine.UI.Button>().interactable = state;
                saveButton.GetComponent<UnityEngine.UI.Image>().color = state ? Color.yellow : Color.white;
            }
        }
    }
}
