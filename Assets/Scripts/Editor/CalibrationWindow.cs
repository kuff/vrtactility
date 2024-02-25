using System.Linq;
using Tactility.Calibration;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CalibrationWindow : EditorWindow
    {
        private string[] _deviceConfigNames;
        private TactilityDeviceConfig[] _deviceConfigs;
        private int _selectedConfigIndex;
        private int _calibrationState;
        private int _currentPad;
        private float _currentAmp;
        private float _currentWidth;

        private string _customFileName = "Test";
        private string _comPort;
        private int _baudRate;
        private EmulatorSettings _settings;
        
        private float _lastUpdateTimestamp;

        [MenuItem("Tactility/Onboard Calibrator")]
        public static void ShowWindow()
        {
            GetWindow<CalibrationWindow>("Onboard Calibrator");
        }

        private void OnEnable()
        {
            // Retrieve all device configurations and populate names for the dropdown
            _deviceConfigs = CalibrationManager.GetAllDeviceConfigs().ToArray();
            _deviceConfigNames = _deviceConfigs.Select(config => config.deviceName).ToArray();
            _currentPad = 0;
            _settings = EmulatorSettings.Instance;
        }

        private void OnGUI()
        {
            // Switch between calibration states
            switch (_calibrationState)
            {
                case 0:
                    DrawConfiguration();
                    break;
                case 1:
                    DrawCalibration();
                    HandleRealTimeUpdate();
                    break;
                case 2:
                    DrawSaveData();
                    break;
            }
        }

        private void DrawSaveData()
        {
            var filePath = CalibrationManager.SaveToFile(_customFileName);
            EditorGUIUtility.systemCopyBuffer = filePath;  // Copy the file path to the clipboard
            EditorUtility.DisplayDialog("Calibration Saved", $"Calibration saved to {filePath}. Path copied to clipboard.", "OK");
            _calibrationState = 0;
        }

        private void DrawCalibration()
        {
            GUILayout.Label("Calibration", EditorStyles.boldLabel);
            GUILayout.Label($"Pad {_currentPad + 1}");
            _currentAmp = EditorGUILayout.Slider("Amplitude", _currentAmp, 2.0f, CalibrationManager.DeviceConfig.maxAmp);
            _currentWidth = EditorGUILayout.Slider("Width", _currentWidth, 500f, CalibrationManager.DeviceConfig.maxWidth);

            if (GUILayout.Button("Next")) 
                MoveToNextPad();
        }

        private void HandleRealTimeUpdate()
        {
            var currentTime = (float)EditorApplication.timeSinceStartup;
            if (!(currentTime - _lastUpdateTimestamp >= 0.1f)) 
                return;  // Update every 0.1 seconds
            
            _lastUpdateTimestamp = currentTime;
            // Perform the update
            UpdateStimulator(_currentPad, _currentAmp, _currentWidth);
        }
        
        private void UpdateStimulator(int padIndex, float amp, float width)
        {
            // Assuming you have a method to send the update to the device
            // Example: yourDeviceController.UpdateStimulation(padIndex, amp, width);
            // This is a placeholder. Implement according to your actual device control logic.
        }

        private void MoveToNextPad()
        {
            // Save current settings and move to next pad or finish calibration
            CalibrationManager.BaseAmps[_currentPad] = _currentAmp;
            CalibrationManager.BaseWidths[_currentPad] = _currentWidth;

            _currentPad++;
            if (_currentPad >= CalibrationManager.DeviceConfig.numPads) 
                _calibrationState = 2;  // Move to save state
            // Reset the timestamp to ensure immediate update for the new pad
            _lastUpdateTimestamp = (float)EditorApplication.timeSinceStartup;
        }

        private void DrawConfiguration()
        {
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            _selectedConfigIndex = EditorGUILayout.Popup("Device Configuration", _selectedConfigIndex, _deviceConfigNames);
            _comPort = EditorGUILayout.TextField(new GUIContent("COM Port", "Enter the COM port the emulator will use"), "COM3");
            _baudRate = EditorGUILayout.IntField(new GUIContent("Baud Rate", "Set the baud rate for the serial connection"), _settings.baudRate);
            _customFileName = EditorGUILayout.TextField(new GUIContent("File Name", "A custom file name to help identify the calibration data"), _customFileName);
            if (!GUILayout.Button("Start Calibration")) 
                return;
            
            // Load the selected device config
            var selectedConfigName = _deviceConfigNames[_selectedConfigIndex];
            CalibrationManager.InitializeDeviceConfig(selectedConfigName);
            _currentPad = 0;
            
            // TODO: Initialize the GammaBoxController with the selected COM port and baud rate
            
            _calibrationState = 1;
        }
    }
}
