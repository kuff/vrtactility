using UnityEngine;
using UnityEngine.UI;

namespace Tactility.Calibration.Interface
{
    [RequireComponent(typeof(PadCalibrationManager))]
    [DefaultExecutionOrder(-10)] //to make sure it runs before PadCalibrationManager Script, otherwise it throws an error
    public class PadFigureManager : MonoBehaviour
    {
        [SerializeField]
        private Image[] padFields;
        
        private PadCalibrationManager _padCalibrationManager;
        private ModalitySelectionManager _modulationSelectionManager;
        private int _currentActiveField;
        
        private void Start()
        {
            _padCalibrationManager = GetComponent<PadCalibrationManager>();
            _modulationSelectionManager = GetComponent<ModalitySelectionManager>();
        }

        public void SetPadFigureCalibration()
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

        public void SetPadFigureDemo(int forceLevel, string modality)
        {
            // Set all padFields to black
            for (int i = 12; i < padFields.Length; i++)
            {
                padFields[i].color = Color.black;
            }

            var isStimOn_demo = _modulationSelectionManager.isModalityActivated;
            Debug.Log("isStimOn: " + isStimOn_demo);

            if (modality == "Spatial" || modality == "Mixed")
            {
                int[][] greenIndices = new int[][]
                {
                    new int[] {1, 7}, // forceLevel 0
                    new int[] {1, 7, 4, 10}, // forceLevel 1
                    new int[] {1, 7, 4, 10, 3, 9}, // forceLevel 2
                    new int[] {1, 7, 4, 10, 3, 9, 5, 11}, // forceLevel 3
                    new int[] {1, 7, 4, 10, 3, 9, 5, 11, 0, 6}, // forceLevel 4
                    new int[] {1, 7, 4, 10, 3, 9, 5, 11, 0, 6, 2, 8}  // forceLevel 5
                };

                // Set the specific padFields to green based on the forceLevel
                if (forceLevel >= 0 && forceLevel <= greenIndices.Length)
                {
                    foreach (int index in greenIndices[forceLevel - 1])
                    {
                        padFields[index + 12].color = isStimOn_demo ? Color.yellow : Color.green; ;
                    }
                }
            }
            else if (modality == "Frequency")
            {
                int[] greenIndices = new int[] { 1, 7, 4, 10 };

                // Set the specific padFields to green based on the forceLevel
                if (forceLevel >= 0)
                {
                    foreach (int index in greenIndices)
                    {
                        padFields[index + 12].color = isStimOn_demo ? Color.yellow : Color.green; ;
                    }
                }
            }
        }
        //private void Update()
        //{
        //    var currentPadIndex = _padCalibrationManager.currentPadIndex;

        //    // If the index is larger than the number of pad fields, ignore
        //    if (currentPadIndex >= padFields.Length)
        //    {
        //        // Reset the active pad color
        //        padFields[_currentActiveField].color = Color.black;

        //        return;
        //    }

        //    // If the current pad index is not the same as the current active field
        //    if (currentPadIndex != _currentActiveField)
        //    {
        //        // Make the active Image black and the new one white
        //        padFields[_currentActiveField].color = Color.black;
        //        padFields[currentPadIndex].color = Color.green;

        //        _currentActiveField = currentPadIndex;
        //    }

        //    // If stimulation is on, make the active Image yellow, if it's off, make it white
        //    var isStimOn = _padCalibrationManager.isStimOn;
        //    padFields[_currentActiveField].color = isStimOn ? Color.yellow : Color.green;
        //}
    }
}
