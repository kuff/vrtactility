using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Tactility.Calibration.CalibrationManager;

namespace Tactility.Calibration.Interface
{
    public class DeviceConfigSelection : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The dropdown menu for selecting the device configuration.")]
        private Dropdown configDropdown;
        [HideInInspector]
        public TactilityDeviceConfig selectedConfig;
        
        // Populate the dropdown with the available device configuration names
        private void Start()
        {
            configDropdown.ClearOptions();
            var configs = GetAllDeviceConfigs();
            
            // Add device names to the dropdown
            configDropdown.AddOptions(configs.Select(config => config.deviceName).ToList());
        }
        
        // Set the selectedConfig to the TactilityDeviceConfig instance corresponding to the selected name
        public void SetSelectedConfig()
        {
            selectedConfig = GetAllDeviceConfigs().First(config => config.deviceName == configDropdown.options[configDropdown.value].text);
        }
    }
}
