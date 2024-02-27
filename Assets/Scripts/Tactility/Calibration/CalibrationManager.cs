using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Tactility.Box;
using UnityEditor;
using UnityEngine;

namespace Tactility.Calibration
{
    public class CalibrationManager : MonoBehaviour
    {
        [Tooltip("The configuration settings for the connected tactility device. This includes device-specific " +
                 "parameters such as number of pads, use of implicit anodes, and other configurable properties." +
                 "Note: This selection may be overruled at runtime by the LoadDeviceConfigByName method.")]
        [SerializeField]
        private TactilityDeviceConfig deviceConfigInstance;        // Non-static, serialized field
        [Tooltip("The name of the calibration file to be loaded at startup. If left blank, no file will be loaded. " +
                 "This file should be located in the persistent data path and contain a list of amplitudes and " +
                 "widths for each pad, separated by commas. The first line should contain the device name and version.")]
        [SerializeField]
        private string calibrationFileName;
        
        private static TactilityDeviceConfig _deviceConfigStatic;  // Static field to hold the instance

        // Calibrated "just noticeable differences" (JNDs) for the device
        public static float[] BaseAmps;
        public static float[] BaseWidths;

        // Singleton instance
        public static CalibrationManager Instance { get; private set; }

        // Property to access the device configuration
        public static TactilityDeviceConfig DeviceConfig
        {
            get
            {
                if (_deviceConfigStatic == null) 
                    Debug.LogError("DeviceConfig has not been initialized. Ensure that CalibrationManager is active in the scene.");
                return _deviceConfigStatic;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // If another instance already exists, destroy this one
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Assign the serialized instance to the static field
            _deviceConfigStatic = deviceConfigInstance;

            DontDestroyOnLoad(gameObject);

            // Load the default configuration if none is assigned
            if (_deviceConfigStatic == null) InitializeDeviceConfig();
            
            // Load the calibration data if a file name is provided
            if (!string.IsNullOrEmpty(calibrationFileName)) LoadCalibrationDataFromFile(calibrationFileName);
        }

        public static void InitializeDeviceConfig(string deviceName = null)
        {
            _deviceConfigStatic = LoadDeviceConfigByName(deviceName) ?? 
                                  Resources.Load<TactilityDeviceConfig>("Tactility/GloveDeviceConfig");

            if (_deviceConfigStatic == null)
            {
                Debug.LogError("Default device configuration could not be loaded. Please check the path and file.");
            }
            else
            {
                Debug.LogWarning($"Device configuration for {deviceName ?? "default"} set.");
                BaseAmps = new float[_deviceConfigStatic.numPads];
                BaseWidths = new float[_deviceConfigStatic.numPads];
            }
        }

        public static TactilityDeviceConfig LoadDeviceConfigByName(string deviceName)
        {
            var allConfigs = GetAllDeviceConfigs();
            foreach (var config in allConfigs)
            {
                if (config.deviceName != deviceName) continue;
                Debug.Log($"Configuration for {deviceName} loaded successfully.");
                return config;
            }

            Debug.LogError($"Configuration for {deviceName} could not be found.");
            return null;
        }

        public static List<TactilityDeviceConfig> GetAllDeviceConfigs()
        {
            var configs = new List<TactilityDeviceConfig>();

#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t:{nameof(TactilityDeviceConfig)}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<TactilityDeviceConfig>(path);
                configs.Add(asset);
            }
#else
            // If running outside the editor, attempt to load all configs from a Resources folder
            configs.AddRange(Resources.LoadAll<TactilityDeviceConfig>("Tactility"));
#endif
            // Check for duplicate device names
            var deviceNames = new List<string>();
            foreach (var config in configs)
            {
                if (deviceNames.Contains(config.deviceName))
                {
                    Debug.LogError($"Duplicate device name found: {config.deviceName}. Config will be ignored.");
                    continue;
                }
                deviceNames.Add(config.deviceName);
            }
            
            // Sort the list by device name
            configs.Sort((a, b) => string.Compare(a.deviceName, b.deviceName, StringComparison.Ordinal));
            
            return configs;
        }

        public static string GetEncodedStringForSinglePad(int padIndex, float padAmp, float padWidth,
            AbstractBoxController boxController)
        {
            var ampsFull = new float[DeviceConfig.numPads];
            var widthsFull = new float[DeviceConfig.numPads];

            ampsFull[padIndex] = padAmp;
            widthsFull[padIndex] = padWidth;

            return boxController.GetEncodedString(ampsFull, widthsFull);
        }

        [CanBeNull]
        public static string SaveCalibrationDataToFile(string dataName = null)
        {
            var prefix = string.IsNullOrEmpty(dataName) ? "" : $"{dataName}_";
            var fileName = $"{prefix}Calibration_{_deviceConfigStatic.deviceName}_{DateTime.Now:yyyyMMdd}.txt";
            var filePath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"{_deviceConfigStatic.deviceName}, {Application.version}");
                    for (var i = 0; i < BaseAmps.Length; i++)
                        // Ensure floats are formatted with "." and not ","
                        writer.WriteLine($"{BaseAmps[i].ToString(System.Globalization.CultureInfo.InvariantCulture)},{BaseWidths[i].ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                }
                Debug.Log($"Calibration data saved to {filePath}");
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save calibration data: {e.Message}");
                return null;
            }
        }

        public static void LoadCalibrationDataFromFile(string calibrationFileName)
        {
            var filePath = Path.Combine(Application.persistentDataPath, calibrationFileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Calibration file not found: {filePath}");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                
                // Ensure that the loaded data is for the current device
                if (lines.Length < 1 || !lines[0].Contains(_deviceConfigStatic.deviceName))
                {
                    Debug.LogError($"Calibration file does not match the current device: {filePath}. The file wasn't loaded.");
                    return;
                }
                
                // Warn the user if the version of the calibration file is newer than the application
                if (lines[0].Contains(Application.version)) 
                    Debug.LogWarning($"The calibration file {filePath} was created with a newer version of the application.");
                
                // Extract device name and version from the first line
                var deviceConfigParts = lines[0].Split(',');
                if (deviceConfigParts.Length >= 1)
                {
                    var deviceName = deviceConfigParts[0].Trim();
                    _deviceConfigStatic = LoadDeviceConfigByName(deviceName);
                }

                BaseAmps = new float[lines.Length - 1];
                BaseWidths = new float[lines.Length - 1];

                for (var i = 1; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length < 2) continue;
                    
                    // Parse floats with "." and not ","
                    BaseAmps[i - 1] = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    BaseWidths[i - 1] = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                }
                // for (var i = 0; i < BaseAmps.Length; i++)
                //     Debug.Log($"Pad {i + 1}: Amp = {BaseAmps[i]}, Width = {BaseWidths[i]}");
                Debug.Log($"Calibration data loaded from {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load calibration data: {e.Message}");
            }
        }
    }
}