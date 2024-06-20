// Copyright (C) 2024 Peter Leth

#region
using System;
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

        private AbstractBoxController _boxController;
        private PadFigureManager _padFigureManager;

        private readonly int[] _remapStrip = {31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18};    
        private int _currentPad;

        public static event Action CalibrationValuesChanged;

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
            _padFigureManager = GetComponent<PadFigureManager>();
            currentPadIndex = 0;
            isStimOn = false;

            _padFigureManager.SetPadFigureCalibration();
            _currentPad = _remapStrip[currentPadIndex]-1;
            UpdateCurrentPadString();
            SetCalibrationFileName();
            
            // var (padAmp, padWidth) = GetFieldValuesForPad();
            // BaseAmps[_currentPad] = padAmp;
            // BaseWidths[_currentPad] = padWidth;
        }
        
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
            }
            _currentPad = _remapStrip[currentPadIndex]-1;
            _padFigureManager.SetPadFigureCalibration();
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
            _padFigureManager.SetPadFigureCalibration();
            UpdateCurrentPadString();
            UpdateInputFields();
            UpdateStimulation();
        }

        private void SaveCalibrationValues()
        {
            var (padAmp, padWidth) = GetFieldValuesForPad();
            BaseAmps[_currentPad] = padAmp;
            BaseWidths[_currentPad] = padWidth;
        }

        private void UpdateInputFields()
        {
            amplitudeField.text = FloatToText(BaseAmps[_currentPad]);
            widthField.text = BaseWidths[_currentPad].ToString();
        }

        private void UpdateCurrentPadString()
        {
            activePadText.text = "Calibrating Pad: " + (currentPadIndex + 1);
        }

        public void UpdateStimulation()
        {
            var prevStimOn = isStimOn;

            var (padAmp, padWidth) = GetFieldValuesForPad();
            if (!DeviceConfig.IsAnode(_currentPad))
            {
                var calibratedAmp = BaseAmps[_currentPad];
                var calibratedWidth = BaseWidths[_currentPad];
                if (padAmp != calibratedAmp || padWidth != calibratedWidth)
                {
                    CalibrationValuesChanged?.Invoke();
                }
            }

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
                var stimString = GetEncodedStringForSinglePad(_currentPad, padAmp, padWidth, _boxController);
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
        
        private (float, int) GetFieldValuesForPad()
        {
            var amp = string.IsNullOrEmpty(amplitudeField.text) ? TextToFloat(amplitudeField.placeholder.GetComponent<Text>().text) : TextToFloat(amplitudeField.text);
            var width = string.IsNullOrEmpty(widthField.text) ? int.Parse(widthField.placeholder.GetComponent<Text>().text, CultureInfo.InvariantCulture) : int.Parse(widthField.text, CultureInfo.InvariantCulture);
            return (amp, width);
        }
    }
}