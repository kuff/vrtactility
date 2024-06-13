// Copyright (C) 2024 Peter Leth

#region
using System.Collections;
using System.Linq;
using Tactility.Calibration;
using UnityEngine;
using static Tactility.Calibration.CalibrationManager;
#endregion

namespace Tactility.Modulation
{
    public class ConstantWidthModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;

        protected override IEnumerator Start()
        {
            yield return base.Start();
            _dataProvider = GetComponent<ITactilityDataProvider>();
        }

        public override ModulationData? GetModulationData()
        {
            // Generate array of DeviceConfig.maxWidth values with length DeviceConfig.numPads
            // var widths = new float[DeviceConfig.numPads];
            // for (var i = 0; i < DeviceConfig.numPads; i++)
            // {
            //     widths[i] = DeviceConfig.maxWidth;
            // }
            Debug.Log(_dataProvider.IsActive());
            return new ModulationData
            {
                Type = ModulationType.Width,
                // Case CalibrationManager.BaseWidths to floats
                Values = _dataProvider.IsActive() ? BaseWidths.Select(x => (float)x).ToArray() : null,
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
