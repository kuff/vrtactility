// Copyright (C) 2024 Peter Leth

#region
using System.Collections;
using Tactility.Calibration;
using static Tactility.Calibration.CalibrationManager;
#endregion

namespace Tactility.Modulation
{
    public class ConstantAmpModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;
        
        protected override IEnumerator Start()
        {
            yield return base.Start();
            _dataProvider = GetComponent<ITactilityDataProvider>();
        }
        
        public override ModulationData? GetModulationData()
        {
            // Generate array of DeviceConfig.maxAmp values with length DeviceConfig.numPads
            // var amps = new float[DeviceConfig.numPads];
            // for (var i = 0; i < DeviceConfig.numPads; i++)
            // {
            //     amps[i] = DeviceConfig.maxAmp;
            // }

            if (!_dataProvider.IsActive())
            {
                return null;
            }

            return new ModulationData
            {
                Type = ModulationType.Amplitude,
                Values = BaseAmps
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should always be true, as the base amps should always be compatible with the device
            // Otherwise, this is a problem in the device config.
            return true;
        }
    }
}
