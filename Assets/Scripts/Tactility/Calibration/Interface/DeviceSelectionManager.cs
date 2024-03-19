// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Tactility.Calibration.CalibrationManager;
#endregion

namespace Tactility.Calibration.Interface
{
    public class DeviceSelectionManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The dropdown menu for selecting the device configuration.")]
        private Dropdown configDropdown;

        [SerializeField]
        [CanBeNull]
        [Tooltip("The default device configuration to use if no other is selected. It is not required that this field be specified.")]
        private TactilityDeviceConfig defaultConfig;

        [HideInInspector]
        public TactilityDeviceConfig selectedConfig;

        // Populate the dropdown with the available device configuration names
        private void Start()
        {
            configDropdown.ClearOptions();
            var configs = GetAllDeviceConfigs();

            // Add device names to the dropdown
            if (defaultConfig != null)
            {
                // If a default config is set, add it first
                configDropdown.options.Add(new Dropdown.OptionData(defaultConfig.deviceName));
            }
            // Add any device config which hasn't yet been added
            foreach (var config in configs.Where(config => config != defaultConfig))
            {
                configDropdown.options.Add(new Dropdown.OptionData(config.deviceName));
            }

            UpdateSelectedConfig();
        }

        // Set the selectedConfig to the TactilityDeviceConfig instance corresponding to the selected name
        public void UpdateSelectedConfig()
        {
            var configName = configDropdown.options[configDropdown.value].text;
            SetDeviceConfig(configName);
        }
    }
}
