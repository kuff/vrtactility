using Tactility.Modulation;
using UnityEngine;
namespace Tactility.Calibration.Interface
{
    public class ModalityDemoManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("List of modality demo GameObjects. Each GameObject represents a different modality demo. The GameObjects are activated/deactivated based on the selected modality. They are assumed to be in the order \"Frequency\", \"Spatial\", \"Mixed\", and \"None\", following by the same four oscillating variant.")]
        private GameObject[] modalityDemoObjects;
        
        private ModalitySelectionManager _modalitySelectionManager;

        private void OnDisable()
        {
            if (_modalitySelectionManager != null)
            {
                _modalitySelectionManager.OnModalityChanged -= HandleModalityChanged;
            }
        }

        private void Start()
        {
            _modalitySelectionManager = FindObjectOfType<ModalitySelectionManager>();
            _modalitySelectionManager.OnModalityChanged += HandleModalityChanged;
        }

        private void HandleModalityChanged(string modality, bool isActive, bool isOscillating, string forceLevel)
        {
            // Debug.Log($"Modality changed to {modality}, Active: {isActive}, Oscillating: {isOscillating}, Force Level: {forceLevel}");
            
            // Deactivate all modality demo objects
            foreach (var modalityDemoObject in modalityDemoObjects)
            {
                modalityDemoObject.SetActive(false);
            }
            
            // Go no further if the modality is not active
            if (!isActive)
            {
                return;
            }
            
            // Activate the modality demo object corresponding to the selected modality
            var indexToActivate = modality switch
            {
                "Frequency" => 0,
                "Spatial" => 1,
                "Mixed" => 2,
                "Constant" => 3,
                _ => 0
            };

            // If oscillation is enabled, add 4 to the index to activate the corresponding oscillation demo
            if (isOscillating)
            {
                indexToActivate += 4;
            }
            
            // Enable the selected modality demo object
            modalityDemoObjects[indexToActivate].SetActive(true);

            if (!isOscillating)
            {
                // Pass the force level to the Debug Data Provider
                var debugSetTactilityDataProvider = FindObjectOfType<DebugSetTactilityDataProvider>();
                debugSetTactilityDataProvider.SetForceLevel(int.Parse(forceLevel));
            }
        }
    }
}
