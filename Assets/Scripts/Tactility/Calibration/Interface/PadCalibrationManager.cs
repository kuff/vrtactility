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
    public struct CalibrationValues
    {
        public readonly float Amplitude;
        public readonly float Width;

        public CalibrationValues(float amplitude, float width)
        {
            Amplitude = amplitude;
            Width = width;
        }
    }

    public class PadCalibrationManager : MonoBehaviour
    {
        [SerializeField]
        private InputField amplitudeField;
        [SerializeField]
        private InputField widthField;
        [SerializeField]
        private Toggle stimulateToggle;
        [SerializeField]
        private Text activePadText;
        // [SerializeField]
        // private Button nextButton;
        // [SerializeField]
        // private Button prevButton;

        private AbstractBoxController _boxController;
        private List<CalibrationValues> _calibrationValues;
        private bool _canSaveData;
        private int _currentPadIndex;
        private bool _isStimOn;

        private void Start()
        {
            _boxController = FindObjectOfType<AbstractBoxController>();
            _calibrationValues = new List<CalibrationValues>();
            _currentPadIndex = 0;
            _isStimOn = false;

            // Initialize values array with number of pads
            for (var i = 0; i < DeviceConfig.numPads; i++)
            {
                _calibrationValues.Add(new CalibrationValues(DeviceConfig.minAmp, DeviceConfig.minWidth));
            }

            // Initialize the Input Fields to their default values
            //UpdateInputFields();

            UpdateCurrentPadString();
        }

        private void OnDestroy()
        {
            // Save the calibration values to the CalibrationManager when the object (scene) is destroyed
            BaseAmps = new float[DeviceConfig.numPads];
            BaseWidths = new int[DeviceConfig.numPads];
            for (var i = 0; i < DeviceConfig.numPads; i++)
            {
                BaseAmps[i] = _calibrationValues[i].Amplitude;
                BaseWidths[i] = (int)_calibrationValues[i].Width;
            }

            // Save the calibration data to a file with a timestamp name
            if (_canSaveData)
            {
                SaveCalibrationDataToFile(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            }
        }

        public void NextPad()
        {
            SaveCalibrationValues();
            if (_currentPadIndex < DeviceConfig.numPads - 1)
            {
                _currentPadIndex++;

                // Only save the calibration data to disk if we've cycled through all the pads
                if (_currentPadIndex == DeviceConfig.numPads - 1)
                {
                    _canSaveData = true;
                }
            }
            UpdateCurrentPadString();
            UpdateInputFields();
        }

        public void PreviousPad()
        {
            SaveCalibrationValues();
            if (_currentPadIndex > 0)
            {
                _currentPadIndex--;
            }
            UpdateCurrentPadString();
            UpdateInputFields();
        }

        private void SaveCalibrationValues()
        {
            // Save the text from the text field, but if it's empty, save the placeholder text instead
            _calibrationValues[_currentPadIndex] = new CalibrationValues(string.IsNullOrEmpty(amplitudeField.text) ? TextToFloat(amplitudeField.placeholder.GetComponent<Text>().text) : TextToFloat(amplitudeField.text), string.IsNullOrEmpty(widthField.text) ? TextToFloat(widthField.placeholder.GetComponent<Text>().text) : TextToFloat(widthField.text));
        }

        private void UpdateInputFields()
        {
            // Debug.Log($"Updating for index {_currentPadIndex}");
            // Update the text of the input fields
            amplitudeField.text = FloatToText(_calibrationValues[_currentPadIndex].Amplitude);
            widthField.text = FloatToText(_calibrationValues[_currentPadIndex].Width);
        }

        private void UpdateCurrentPadString()
        {
            // If the current pad is an anode, update the text to reflect that
            if (DeviceConfig.IsAnode(_currentPadIndex))
            {
                activePadText.text = "Pad is an anode";
                activePadText.color = Color.red;
                return;
            }

            // Update the text otherwise
            activePadText.text = "Calibrating Pad: " + (_currentPadIndex + 1);
        }

        public void UpdateStimulation()
        {
            var prevStimOn = _isStimOn;

            _boxController.ResetAllPads();
            if (DeviceConfig.IsAnode(_currentPadIndex) || !stimulateToggle.isOn)
            {
                _isStimOn = false;

                // Turn the activePadText red if it's an anode, otherwise white
                activePadText.color = DeviceConfig.IsAnode(_currentPadIndex) ? Color.red : Color.white;
            }
            else if (stimulateToggle.isOn)
            {
                _isStimOn = true;

                // Get stim string for single pad using Text Field values
                var stimString = GetEncodedStringForSinglePad(_currentPadIndex, TextToFloat(amplitudeField.text), int.Parse(widthField.text, CultureInfo.InvariantCulture), _boxController);
                _boxController.Send(stimString);

                // Turn the activePadText yellow
                activePadText.color = Color.yellow;
            }

            // If stim changed, enable/disable it
            if (prevStimOn != _isStimOn)
            {
                if (_isStimOn)
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
