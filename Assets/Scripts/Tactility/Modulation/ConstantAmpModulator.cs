using Tactility.Calibration;

namespace Tactility.Modulation
{
    public class ConstantAmpModulator : AbstractModulator
    {
        public override ModulationData? GetModulationData()
        {
            return new ModulationData()
            {
                Type = ModulationType.Amplitude,
                Values = CalibrationManager.BaseAmps
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should always be true, as the base amps are always compatible with the device
            // Otherwise, this is a problem elsewhere.
            return true;
        }
    }
}