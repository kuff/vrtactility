// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
#endregion

namespace Tactility.Calibration.Interface
{
    public class DeviceSelectionManager : DropdownManager<TactilityDeviceConfig>
    {
        protected override List<TactilityDeviceConfig> GetAllItems()
        {
            return CalibrationManager.GetAllDeviceConfigs();
        }

        protected override void SetSelectedItem(TactilityDeviceConfig config)
        {
            CalibrationManager.SetDeviceConfig(config.deviceName);
        }

        protected override string GetItemName(TactilityDeviceConfig config)
        {
            return config.deviceName;
        }
    }
}
