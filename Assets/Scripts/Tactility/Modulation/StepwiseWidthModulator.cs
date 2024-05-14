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
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwiseWidthModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;

        protected override IEnumerator Start()
        {
            yield return base.Start();

            _dataProvider = GetComponent<ITactilityDataProvider>();

            // If no ITactilityDataProvider is found, disable the modulator
            if (_dataProvider != null)
            {
                yield break;
            }
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwiseWidthModulator.");
            enabled = false;
        }

        public override ModulationData? GetModulationData()
        {
            // TODO: This one is not really used. Turn it into a varied one across all pads maybe?

            if (!_dataProvider.IsActive())
            {
                return null;
            }

            ref var modulationData = ref _dataProvider.GetTactilityData();
            var remap_strip = new[] { 31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18 };

            // Update stimuli for each touching finger bone of interest
            var valueBatch = new float[2];
            for (var i = 0; i < modulationData.BoneIds.Count; i++)
            {
                var pressure = modulationData.Values[i];
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (modulationData.BoneIds[i])
                {
                    case OVRSkeleton.BoneId.Hand_Thumb3:
                        valueBatch[0] = pressure;
                        break;
                    case OVRSkeleton.BoneId.Hand_Index3:
                        valueBatch[1] = pressure;
                        break;
                }
            }

            var pressureValues = new float[32];
            for (var i = 0; i < 32; i++)
            {
                // Use remap value to determine which finger pressure value we use
                var pressureValue = i switch
                {
                    < 6 => valueBatch[0],
                    < 12 => valueBatch[1],
                    _ => 0
                };

                // Define value buckets (0.25, 0.5, 0.75, 1.0) and project pressureValue to last bucket it is greater than
                pressureValue = pressureValue switch
                {
                    > 0.85f => 1.0f,
                    > 0.6f => 0.85f,
                    > 0.45f => 0.6f,
                    > 0.3f => 0.45f,
                    > 0.15f => 0.3f,
                    _ => 0.15f
                };
                var widthValue = BaseWidths[i] + 200f * pressureValue;

                // Set widthValue to 0 if all pressure values are 0
                if (valueBatch.All(p => p == 0f))
                {
                    widthValue = 0f;
                }

                // Remap widthValue using the remap array and store it in the pressureValues array
                pressureValues[remap_strip[i] - 1] = widthValue;
            }

            return new ModulationData
            {
                Type = ModulationType.Width,
                Values = pressureValues
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This modulator only supports the "glove" device at the moment
            return deviceConfig.deviceName == "glove";
        }
    }
}
