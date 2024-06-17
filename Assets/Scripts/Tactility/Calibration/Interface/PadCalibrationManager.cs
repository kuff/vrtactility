// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections.Generic;
using System.Globalization;
using Tactility.Box;
using UnityEngine;
using UnityEngine.UI;
using static Tactility.Calibration.CalibrationManager;
using static Tactility.Calibration.Interface.ValueChangeManager;
#endregion

namespace Tactility.Calibration.Interface
{
    // public struct CalibrationValues
    // {
    //     public readonly float Amplitude;
    //     public readonly float Width;
    //
    //     public CalibrationValues(float amplitude, float width)
    //     {
    //         Amplitude = amplitude;
    //         Width = width;
    //     }
    // }

    public class PadCalibrationManager : MonoBehaviour
    {
        [HideInInspector]
        public int currentPadIndex;
        [HideInInspector]
        public bool isStimOn;
        [HideInInspector]
        public string fileName;
        
        [SerializeField]
        private InputField amplitudeField;
        [SerializeField]
        private InputField widthField;
        [SerializeField]
        private Toggle stimulateToggle;
        [SerializeField]
        private Text activePadText;
        [SerializeField]
        private InputField participantField;
        // [SerializeField]
        // private Button nextButton;
        // [SerializeField]
        // private Button prevButton;

        private AbstractBoxController _boxController;
        // private List<CalibrationValues> _calibrationValues;
        // private bool _canSaveData;

        // Elements from 0-5 -> index (I1 of connecting board)
        // Elements from 6-12 -> thumb (I2 of connecting board)
        private readonly int[] _remapStrip = {31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18};    
        private int _currentPad;

        private void OnEnable()
        {
            FindObjectOfType<InterfaceManager>()!.OnSceneChanged += UpdateInputFieldsOnSceneChanged;
            FindObjectOfType<InterfaceManager>()!.OnSceneChanged += ((_, _) =>
            {
                stimulateToggle.isOn = false;
                UpdateStimulation();
            });
        }
        
        private void OnDisable()
        {
            try
            {
                FindObjectOfType<InterfaceManager>()!.OnSceneChanged -= UpdateInputFieldsOnSceneChanged;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // Do nothing...
            }
        }

        private void Start()
        {
            _boxController = FindObjectOfType<AbstractBoxController>();
            // _calibrationValues = new List<CalibrationValues>();
            currentPadIndex = 0;
            isStimOn = false;

            // Initialize values array with number of pads
            // for (var i = 0; i < DeviceConfig.numPads; i++)
            // {
            //     _calibrationValues.Add(new CalibrationValues(0.5f, 100));
            // }

            // Initialize the Input Fields to their default values
            //UpdateInputFields();
            _currentPad = _remapStrip[currentPadIndex]-1;
            UpdateCurrentPadString();
            SetCalibrationFileName();
        }

        /*private void OnDestroy()
        {
            // Save the calibration values to the CalibrationManager when the object (scene) is destroyed
            BaseAmps = new float[DeviceConfig.numPads];
            BaseWidths = new int[DeviceConfig.numPads];
            for (var i = 0; i < DeviceConfig.numPads; i++)
            {
                BaseAmps[i] = _calibrationValues[i].Amplitude;
                BaseWidths[i] = (int)_calibrationValues[i].Width;
            }
            
            // var calibrationFilePath = SaveCalibrationDataToFile(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            // LoadCalibrationDataFromFile(calibrationFilePath);
            SaveCalibrationDataToFile(fileName);
        }*/
        
        private void UpdateInputFieldsOnSceneChanged(GameObject oldScene, GameObject newScene)
        {
            if (newScene.name == "Calibration")
            {
                UpdateInputFields();
            }
        }

        public void SetCalibrationFileName()
        {
            var newFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            
            // if participantField.text is not empty, prepend it to the filename
            var participantString = participantField?.text;
            if (!string.IsNullOrEmpty(participantString))
            {
                newFileName = participantString + "_" + newFileName;
            }
            
            fileName = newFileName;
        }

        public void SaveCalibrationDataToFileVoid()
        {
            SetCalibrationFileName();
            SaveCalibrationDataToFile(fileName);
        }

        public void NextPad()
        {
            SaveCalibrationValues();
            if (currentPadIndex < DeviceConfig.numPads - 1)
            {
                currentPadIndex++;

                // Only save the calibration data to disk if we've cycled through all the pads
                // if (currentPadIndex+1 == DeviceConfig.numPads)
                // {
                //     _canSaveData = true;
                // }
            }
            _currentPad = _remapStrip[currentPadIndex]-1;
            UpdateCurrentPadString();
            UpdateInputFields();
            UpdateStimulation();
        }

        public void PreviousPad()
        {
            SaveCalibrationValues();
            if (currentPadIndex > 0)
            {
                currentPadIndex--;
            }
            _currentPad = _remapStrip[currentPadIndex]-1;
            UpdateCurrentPadString();
            UpdateInputFields();
            UpdateStimulation();
        }

        private void SaveCalibrationValues()
        {
            // Save the text from the text field, but if it's empty, save the placeholder text instead
            // _calibrationValues[_currentPad] = new CalibrationValues(string.IsNullOrEmpty(amplitudeField.text) ? TextToFloat(amplitudeField.placeholder.GetComponent<Text>().text) : TextToFloat(amplitudeField.text), string.IsNullOrEmpty(widthField.text) ? TextToFloat(widthField.placeholder.GetComponent<Text>().text) : TextToFloat(widthField.text));
            // Use CalibrationManager.baseAmps and baseWidths instead of _calibrationValues
            BaseAmps[_currentPad] = string.IsNullOrEmpty(amplitudeField.text) ? TextToFloat(amplitudeField.placeholder.GetComponent<Text>().text) : TextToFloat(amplitudeField.text);
            BaseWidths[_currentPad] = string.IsNullOrEmpty(widthField.text) ? int.Parse(widthField.placeholder.GetComponent<Text>().text, CultureInfo.InvariantCulture) : int.Parse(widthField.text, CultureInfo.InvariantCulture);
        }

        public void UpdateInputFields()
        {
            // Debug.Log($"Updating for index {_currentPadIndex}");
            // Update the text of the input fields
            // amplitudeField.text = FloatToText(_calibrationValues[_currentPad].Amplitude);
            // widthField.text = FloatToText(_calibrationValues[_currentPad].Width);
            // Use CalibrationManager.baseAmps and baseWidths instead of _calibrationValues
            amplitudeField.text = FloatToText(BaseAmps[_currentPad]);
            widthField.text = BaseWidths[_currentPad].ToString();
        }

        private void UpdateCurrentPadString()
        {
            // If the current pad is an anode, update the text to reflect that
            //if (DeviceConfig.IsAnode(_currentPad))
            //{
            //    activePadText.text = "Pad is an anode";
            //    activePadText.color = Color.red;
            //    return;
            //}

            // Update the text otherwise
            activePadText.text = "Calibrating Pad: " + (currentPadIndex + 1);
        }

        public void UpdateStimulation()
        {
            var prevStimOn = isStimOn;

            //_boxController.ResetAllPads();
            if (DeviceConfig.IsAnode(_currentPad) || !stimulateToggle.isOn)
            {
                isStimOn = false;

                // Turn the activePadText red if it's an anode, otherwise white
                activePadText.color = DeviceConfig.IsAnode(_currentPad) ? Color.red : Color.white;
            }
            else if (stimulateToggle.isOn)
            {
                isStimOn = true;

                // Get stim string for single pad using Text Field values
                var stimString = GetEncodedStringForSinglePad(_currentPad, TextToFloat(amplitudeField.text), int.Parse(widthField.text, CultureInfo.InvariantCulture), _boxController);
                _boxController.Send(stimString);

                // Turn the activePadText yellow
                activePadText.color = Color.yellow;
            }

            // If stim changed, enable/disable it
            if (prevStimOn != isStimOn)
            {
                if (isStimOn)
                {
                    _boxController.EnableStimulation();
                }
                else
                {
                    _boxController.DisableStimulation();
                }
            }
        }

        private void SetStimulation(bool value)
        {
            stimulateToggle.isOn = value;
            UpdateStimulation();
        }
    }
}
