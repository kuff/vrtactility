using Tactility.Calibration;
using static Tactility.Calibration.CalibrationManager;

namespace Tactility.Modulation
{
    public class ConstantAmpModulator : AbstractModulator
    {
        public override ModulationData? GetModulationData()
        {
            return new ModulationData()
            {
                Type = ModulationType.Amplitude,
                Values = BaseAmps
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should always be true, as the base amps should always be compatible with the device
            // Otherwise, this is a problem elsewhere.
            return true;
        }
    }
}