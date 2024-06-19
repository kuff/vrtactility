using JetBrains.Annotations;
using System.Linq;
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
        [CanBeNull]
        private GameObject _outgoingModality;

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

        private void Update()
        {
            // We do this to give the system one frame to activate the new modality demo object before deactivating the old one to avoid inconsistencies with stim on/off
            if (_outgoingModality != null)
            {
                _outgoingModality.SetActive(false);
                _outgoingModality = null;
            }
        }

        private void HandleModalityChanged(string modality, bool isActive, bool isOscillating, string forceLevel)
        {
            // Debug.Log($"Modality changed to {modality}, Active: {isActive}, Oscillating: {isOscillating}, Force Level: {forceLevel}");
            
            // Go no further if the modality is not active
            if (!isActive)
            {
                return;
            }
            
            // Save a reference to the currently active modality object
            var oldModality = modalityDemoObjects.FirstOrDefault(obj => obj.activeSelf);
            
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
            
            var newModality = modalityDemoObjects[indexToActivate];
            if (oldModality != newModality)
            {
                _outgoingModality = oldModality;
                newModality.SetActive(true);
            }

            if (!isOscillating)
            {
                // Pass the force level to the Debug Data Provider
                var debugSetTactilityDataProvider = FindObjectOfType<DebugSetTactilityDataProvider>();
                debugSetTactilityDataProvider.SetForceLevel(int.Parse(forceLevel));
            }
        }
    }
}
