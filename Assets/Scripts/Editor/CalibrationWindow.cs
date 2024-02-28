using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tactility.Box;
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
        private int _currentWidth;

        private string _customFileName = "Test";
        private string _comPort = "COM3";
        private int _baudRate = 115200;
        private EmulatorSettings _settings;
        
        private float _lastUpdateTimestamp;
        private float _prevAmp;
        private float _prevWidth;
        
        private SerialController _serialController;
        private GammaBoxController _gammaBoxController;

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
            
            var gammaBoxControllerObject = new GameObject("TEMP_GammaBoxController");
            gammaBoxControllerObject.SetActive(false);
            _serialController = gammaBoxControllerObject.AddComponent<SerialController>();
            _serialController.baudRate = _settings.baudRate;
            _serialController.reconnectionDelay = _settings.reconnectionDelay;
            _serialController.maxUnreadMessages = _settings.maxUnreadMessages;
            _serialController.messageListener = gammaBoxControllerObject;
            _serialController.enabled = false;
            gammaBoxControllerObject.SetActive(true);
            _gammaBoxController = gammaBoxControllerObject.AddComponent<GammaBoxController>();
        }

        private void OnGUI()
        {
            // NOTE: Currently disabled from here
            // _calibrationState = 3;
            // GUILayout.Label("Not yet implemented", EditorStyles.boldLabel);
            // if (GUILayout.Button("Close Window")) 
            //     Close();
            
            // Switch between calibration states
            switch (_calibrationState)
            {
                case 0:
                    DrawConfiguration();
                    break;
                case 1:
                    DrawCalibration();
                    break;
                case 2:
                    DrawSaveData();
                    break;
            }
        }

        private void DrawSaveData()
        {
            var filePath = CalibrationManager.SaveCalibrationDataToFile(_customFileName);
            EditorGUIUtility.systemCopyBuffer = filePath;  // Copy the file path to the clipboard
            EditorUtility.DisplayDialog("Calibration Saved", $"Calibration saved to {filePath}. Path copied to clipboard.", "OK");
            _calibrationState = 0;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void DrawCalibration()
        {
            GUILayout.Label("Calibration", EditorStyles.boldLabel);
            GUILayout.Label($"Pad {_currentPad + 1}");
            _currentAmp = EditorGUILayout.Slider("Amplitude", _currentAmp, 0.5f, CalibrationManager.DeviceConfig.maxAmp);
            _currentWidth = EditorGUILayout.IntSlider("Width", _currentWidth, 100, (int)CalibrationManager.DeviceConfig.maxWidth);
            
            if (_prevAmp != _currentAmp || _prevWidth != _currentWidth)
            {
                _prevAmp = _currentAmp;
                _prevWidth = _currentWidth;
                HandleRealTimeUpdate();
            }

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
            // var commandString = CalibrationManager.GetEncodedStringForSinglePad(padIndex, amp, width, _gammaBoxController);
            // _gammaBoxController.Send(commandString);
        }

        private void MoveToNextPad()
        {
            // Save current settings and move to next pad or finish calibration
            CalibrationManager.BaseAmps[_currentPad] = _currentAmp;
            CalibrationManager.BaseWidths[_currentPad] = _currentWidth;
            // _currentAmp = 4.0f;
            // _currentWidth = 400.0f;

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
            _comPort = EditorGUILayout.TextField(new GUIContent("COM Port", "Enter the COM port the emulator will use"), _comPort);
            _baudRate = EditorGUILayout.IntField(new GUIContent("Baud Rate", "Set the baud rate for the serial connection"), _baudRate);
            _customFileName = EditorGUILayout.TextField(new GUIContent("File Name", "A custom file name to help identify the calibration data"), _customFileName);
            if (!GUILayout.Button("Start Calibration")) 
                return;
            
            // Load the selected device config
            var selectedConfigName = _deviceConfigNames[_selectedConfigIndex];
            CalibrationManager.InitializeDeviceConfig(selectedConfigName);
            _currentPad = 0;
            
            // Initialize the GammaBoxController with the selected COM port, connect, and enable stimulation
            // _serialController.portName = _comPort;
            // _serialController.enabled = true;
            //_gammaBoxController.gameObject.SetActive(true);
            // _gammaBoxController.Connect(_comPort);
            // _gammaBoxController.EnableStimulation();
            
            _calibrationState = 1;
        }
        
        private void OnDestroy()
        {
            // Check if the GameObject exists and destroy it
            if (_gammaBoxController != null)
            {
                DestroyImmediate(_gammaBoxController.gameObject);
            }
        }
    }
}
