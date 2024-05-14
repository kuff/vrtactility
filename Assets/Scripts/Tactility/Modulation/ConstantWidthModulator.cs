// Copyright (C) 2024 Peter Leth

#region
using System.Linq;
using Tactility.Calibration;
using static Tactility.Calibration.CalibrationManager;
#endregion

namespace Tactility.Modulation
{
    public class ConstantWidthModulator : AbstractModulator
    {
        public override ModulationData? GetModulationData()
        {
            // Generate array of DeviceConfig.maxWidth values with length DeviceConfig.numPads
            // var widths = new float[DeviceConfig.numPads];
            // for (var i = 0; i < DeviceConfig.numPads; i++)
            // {
            //     widths[i] = DeviceConfig.maxWidth;
            // }

            return new ModulationData
            {
                Type = ModulationType.Width,
                // Case CalibrationManager.BaseWidths to floats
                Values = BaseWidths.Select(x => (float)x).ToArray()
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should always be true, as the base widths should always be compatible with the device
            // Otherwise, this is a problem elsewhere.
            return true;
        }
    }
}
