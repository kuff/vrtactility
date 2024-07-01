using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Tactility.Calibration.CalibrationManager;

namespace Tactility.Calibration.Interface
{
    public class ModalitySelectionManager : MonoBehaviour
    {
        public bool isModalityActivated;
        public bool doOscillation;

        [SerializeField]
        [Tooltip("List of UI Buttons to manage.")]
        private List<Button> uiButtons;

        [SerializeField]
        [Tooltip("Index of the default selected button.")]
        private int defaultSelectedIndex = 0;

        [SerializeField]
        private Toggle stimulationToggle;
        
        [SerializeField]
        private Toggle oscillationToggle;

        [SerializeField]
        [Tooltip("List of force level UI Buttons to manage.")]
        private List<Button> forceLevelButtons;

        [SerializeField]
        [Tooltip("Index of the default selected force level button.")]
        private int defaultForceLevelIndex = 0;

        [SerializeField]
        [Tooltip("Slider to tune the current amplitude of Thumb pads.")]
        private Slider sliderThumb;

        [SerializeField]
        [Tooltip("Slider to tune the current amplitude of Index pads.")]
        private Slider sliderIndex;

        [SerializeField]
        [Tooltip("Text input with the thumb amplitude multiplier.")]
        public InputField ampMultThumb;

        [SerializeField]
        [Tooltip("Text input with the index amplitude multiplier.")]
        public InputField ampMultIndex;

        private int _currentSelectedIndex;
        private int _currentForceLevelIndex;

        private InterfaceManager _interfaceManager;
        private PadFigureManager _padFigureManager;
        
        private static float[] _calibratedAmps;
        private static int[] _calibratedWidths;

        private int[] thumbPads = new int[] { 11, 12, 13, 10, 9, 8};
        private int[] indexPads = new int[] { 31, 32, 29, 16, 15, 14};

    // Define a delegate for the modality changed event
    public delegate void ModalityChangedEventHandler(string modality, bool isActive, bool isOscillating, string forceLevel);

        // Define the event based on the delegate
        public event ModalityChangedEventHandler OnModalityChanged;

        private void Start()
        {
            _padFigureManager = GetComponent<PadFigureManager>();

            sliderThumb.onValueChanged.AddListener(OnSliderThumbValueChangeCheck);
            ampMultThumb.onValueChanged.AddListener(OnInputFieldThumbValueChangeCheck);

            sliderIndex.onValueChanged.AddListener(OnSliderIndexValueChangeCheck);
            ampMultIndex.onValueChanged.AddListener(OnInputFieldIndexValueChangeCheck);

            _interfaceManager = FindObjectOfType<InterfaceManager>();
            _interfaceManager.OnSceneChanged += (_, _) =>
            {
                // Reset the selected buttons when the scene changes
                SelectButton(defaultSelectedIndex);
                SelectForceLevelButton(defaultForceLevelIndex);
                
                // Reset toggles
                if (doOscillation)
                {
                    oscillationToggle.isOn = false;
                }
                if (isModalityActivated)
                {
                    stimulationToggle.isOn = false;
                }
            };
            
            // if (uiButtons.Count == 0)
            // {
            //     Debug.LogWarning("No UI buttons specified.");
            //     return;
            // }
            //
            // if (forceLevelButtons.Count == 0)
            // {
            //     Debug.LogWarning("No force level buttons specified.");
            //     return;
            // }

            // Clamp the default selected index within the valid range
            defaultSelectedIndex = Mathf.Clamp(defaultSelectedIndex, 0, uiButtons.Count - 1);
            defaultForceLevelIndex = Mathf.Clamp(defaultForceLevelIndex, 0, forceLevelButtons.Count - 1);

            // Initialize the selected buttons
            SelectButton(defaultSelectedIndex);
            SelectForceLevelButton(defaultForceLevelIndex);

            //if (_calibratedAmps == null && _calibratedWidths == null)
            //{
            //    UpdateCalibrationValues();
            //}
            //CalibrationDataLoaded += _ => UpdateCalibrationValues();
        }

        private static void UpdateCalibrationValues()
        {
            // Save current calibration values for if amplitude is changed with the slider (save as value not as reference)
            _calibratedAmps = new float[BaseAmps.Length];
            _calibratedWidths = new int[BaseWidths.Length];
            BaseAmps.CopyTo(_calibratedAmps, 0);
            BaseWidths.CopyTo(_calibratedWidths, 0);
        }

        public void SelectButton(int index)
        {
            if (index < 0 || index >= uiButtons.Count)
            {
                Debug.LogWarning("Button index out of range.");
                return;
            }

            // Deselect the current button
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < uiButtons.Count)
            {
                uiButtons[_currentSelectedIndex].GetComponent<Image>().color = Color.white;
            }

            // Select the new button
            _currentSelectedIndex = index;
            UpdateComponentColor(uiButtons[_currentSelectedIndex], isModalityActivated);

            // Raise the event
            RaiseModalityChangedEvent();
        }

        public void SelectForceLevelButton(int index)
        {
            if (index < 0 || index >= forceLevelButtons.Count)
            {
                Debug.LogWarning("Force level button index out of range.");
                return;
            }

            // Deselect the current force level button
            if (_currentForceLevelIndex >= 0 && _currentForceLevelIndex < forceLevelButtons.Count)
            {
                forceLevelButtons[_currentForceLevelIndex].GetComponent<Image>().color = Color.white;
            }

            // Select the new force level button
            _currentForceLevelIndex = index;
            UpdateComponentColor(forceLevelButtons[_currentForceLevelIndex], isModalityActivated);

            // Raise the event
            RaiseModalityChangedEvent();
        }

        private static void UpdateComponentColor(Component button, bool isActive)
        {
            button.GetComponent<Image>().color = isActive ? Color.yellow : Color.green;
        }

        public void CycleNextButton()
        {
            if (doOscillation)
            {
                // If oscillation is enabled, only cycle through modality buttons
                var nextModalityIndex = (_currentSelectedIndex + 1) % uiButtons.Count;
                SelectButton(nextModalityIndex);
            }
            else
            {
                // Otherwise, cycle through force level buttons first, then modality buttons
                var nextForceLevelIndex = (_currentForceLevelIndex + 1) % forceLevelButtons.Count;
                if (nextForceLevelIndex == 0)
                {
                    var nextModalityIndex = (_currentSelectedIndex + 1) % uiButtons.Count;
                    SelectButton(nextModalityIndex);
                }
                SelectForceLevelButton(nextForceLevelIndex);
            }
        }

        public void CyclePreviousButton()
        {
            if (doOscillation)
            {
                // If oscillation is enabled, only cycle through modality buttons
                var prevModalityIndex = (_currentSelectedIndex - 1 + uiButtons.Count) % uiButtons.Count;
                SelectButton(prevModalityIndex);
            }
            else
            {
                // Otherwise, cycle through force level buttons first, then modality buttons
                var prevForceLevelIndex = (_currentForceLevelIndex - 1 + forceLevelButtons.Count) % forceLevelButtons.Count;
                if (prevForceLevelIndex == forceLevelButtons.Count - 1)
                {
                    var prevModalityIndex = (_currentSelectedIndex - 1 + uiButtons.Count) % uiButtons.Count;
                    SelectButton(prevModalityIndex);
                }
                SelectForceLevelButton(prevForceLevelIndex);
            }
        }

        public void ToggleSelectedButtonActivation()
        {
            isModalityActivated = !isModalityActivated;

            // Change the color of the selected modality button based on the activation state
            UpdateComponentColor(uiButtons[_currentSelectedIndex], isModalityActivated);

            if (!doOscillation)
            {
                // Change the color of the selected force level button based on the activation state
                UpdateComponentColor(forceLevelButtons[_currentForceLevelIndex], isModalityActivated);
            }

            // Raise the event
            RaiseModalityChangedEvent();
        }

        public void ToggleDoOscillation()
        {
            doOscillation = !doOscillation;

            // If oscillation is now enabled and the selected modality isn't activated, activate it
            if (doOscillation)
            {
                // Enable the stimulation toggle and disable its interactability
                var wasAlreadyStimulating = stimulationToggle.isOn;
                stimulationToggle.isOn = true;
                stimulationToggle.interactable = false;

                if (wasAlreadyStimulating)
                {
                    RaiseModalityChangedEvent();
                }

                // Disable the force level buttons
                foreach (var button in forceLevelButtons)
                {
                    button.interactable = false;
                    button.GetComponent<Image>().color = Color.white; // Reset color to default
                }
            }
            else
            {
                // Disable the stimulation toggle and enable its interactability
                stimulationToggle.isOn = false;
                stimulationToggle.interactable = true;

                // Enable the force level buttons
                foreach (var button in forceLevelButtons)
                {
                    button.interactable = true;
                }

                // Restore color for the currently selected force level button
                UpdateComponentColor(forceLevelButtons[_currentForceLevelIndex], isModalityActivated);
            }

            // NOTE: stimulationToggle.isOn will invoke ToggleSelectedButtonActivation and handle button color change and event raising
        }

        private string GetSelectedModality()
        {
            // Return the name of the selected modality using the button's text
            return uiButtons[_currentSelectedIndex].GetComponentInChildren<Text>().text;
        }

        private string GetSelectedForceLevel()
        {
            // Return the name of the selected force level using the button's text
            return forceLevelButtons[_currentForceLevelIndex].GetComponentInChildren<Text>().text;
        }

        private void OnSliderThumbValueChangeCheck(float value)
        {
            if (_calibratedAmps == null && _calibratedWidths == null)
            {
                UpdateCalibrationValues();
            }
            CalibrationDataLoaded += _ => UpdateCalibrationValues();

            var roundedValue = Mathf.Round(value * 10f) / 10f;
            sliderThumb.value = roundedValue;
            ampMultThumb.text = ValueChangeManager.FloatToText(roundedValue);
        }

        private void OnInputFieldThumbValueChangeCheck(string value)
        {
            var roundedValue = ValueChangeManager.TextToFloat(value);
            sliderThumb.value = roundedValue;
            
            // Update the BaseAmps values with the value of _calibratedAmps * roundedValue
            // NOTE: We only need to do this here, since the slider value change will trigger the event
            //for (var i = 0; i < BaseAmps.Length; i++)
            //{
            //    BaseAmps[i] = _calibratedAmps[i] * roundedValue;
            //}

            for (var i = 0; i < thumbPads.Length; i++)
            {
                BaseAmps[thumbPads[i] - 1] = _calibratedAmps[thumbPads[i] - 1] * roundedValue;
            }
        }

        private void OnSliderIndexValueChangeCheck(float value)
        {
            if (_calibratedAmps == null && _calibratedWidths == null)
            {
                UpdateCalibrationValues();
            }
            CalibrationDataLoaded += _ => UpdateCalibrationValues();

            var roundedValue = Mathf.Round(value * 10f) / 10f;
            sliderIndex.value = roundedValue;
            ampMultIndex.text = ValueChangeManager.FloatToText(roundedValue);
        }

        private void OnInputFieldIndexValueChangeCheck(string value)
        {
            var roundedValue = ValueChangeManager.TextToFloat(value);
            sliderIndex.value = roundedValue;

            // Update the BaseAmps values with the value of _calibratedAmps * roundedValue
            // NOTE: We only need to do this here, since the slider value change will trigger the event
            //for (var i = 0; i < BaseAmps.Length; i++)
            //{
            //    BaseAmps[i] = _calibratedAmps[i] * roundedValue;
            //    // Debug.Log($"New BaseAmps[{i}] = {BaseAmps[i]}");
            //}

            for (var i = 0; i < indexPads.Length; i++)
            {
                BaseAmps[indexPads[i] - 1] = _calibratedAmps[indexPads[i] - 1] * roundedValue;
            }
        }

        private void RaiseModalityChangedEvent()
        {
            // Get the selected modality and force level
            var modality = GetSelectedModality();
            var forceLevel = GetSelectedForceLevel();

            // Update pad figure
            var force = int.Parse(forceLevel);
            _padFigureManager.SetPadFigureDemo(force, modality);

            // Raise the event if there are any subscribers
            OnModalityChanged?.Invoke(modality, isModalityActivated, doOscillation, forceLevel);
        }
    }
}
