using UnityEngine;
using UnityEngine.UI;

namespace Tactility.Calibration.Interface
{
    [RequireComponent(typeof(PadCalibrationManager))]
    public class PadFigureManager : MonoBehaviour
    {
        [SerializeField]
        private Image[] padFields;
        
        private PadCalibrationManager _padCalibrationManager;
        private int _currentActiveField;
        
        private void Start()
        {
            _padCalibrationManager = GetComponent<PadCalibrationManager>();
        }

        private void Update()
        {
            var currentPadIndex = _padCalibrationManager.currentPadIndex;
            
            // If the index is larger than the number of pad fields, ignore
            if (currentPadIndex >= padFields.Length)
            {
                // Reset the active pad color
                padFields[_currentActiveField].color = Color.black;
                
                return;
            }
            
            // If the current pad index is not the same as the current active field
            if (currentPadIndex != _currentActiveField)
            {
                // Make the active Image black and the new one white
                padFields[_currentActiveField].color = Color.black;
                padFields[currentPadIndex].color = Color.green;
                
                _currentActiveField = currentPadIndex;
            }
            
            // If stimulation is on, make the active Image yellow, if it's off, make it white
            var isStimOn = _padCalibrationManager.isStimOn;
            padFields[_currentActiveField].color = isStimOn ? Color.yellow : Color.green;
        }
    }
}
