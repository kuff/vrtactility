using System.Linq;
using Tactility.Calibration;

namespace Tactility.Modulation
{
    public class ConstantWidthModulator : AbstractModulator
    {
        public override ModulationData? GetModulationData()
        {
            return new ModulationData()
            {
                Type = ModulationType.Width,
                // Case CalibrationManager.BaseWidths to floats
                Values = CalibrationManager.BaseWidths.Select(x => (float)x).ToArray()
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