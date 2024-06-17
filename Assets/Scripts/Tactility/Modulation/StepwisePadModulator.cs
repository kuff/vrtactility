// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections;
using System.Collections.Generic;
using Tactility.Calibration;
using UnityEngine;
#endregion

namespace Tactility.Modulation
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwisePadModulator : AbstractModulator
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
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwisePadModulator.");
            enabled = false;
        }

        public override ModulationData? GetModulationData()
        {
            if (!_dataProvider.IsActive())
            {
                return null;
            }

            ref var modulationData = ref _dataProvider.GetTactilityData();

            var remapStrip = new[] {31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18};

            // Define spatial levels (values require remapping)
            var level0 = new List<int>();
            var level1 = new List<int> { 4, 10 };
            var level2 = new List<int> { 4, 10, 1, 7 };
            var level3 = new List<int> { 4, 10, 1, 7, 3, 9 };
            var level4 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11 };
            var level5 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11, 0, 6 };
            var level6 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11, 0, 6, 2, 8 };

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

                // Define value buckets (0.15, 0.3, 0.45, 0.6, 0.85, 1.0) and project pressureValue to last bucket it is greater than
                pressureValues[i] = pressureValue switch
                {
                    > 0.85f => 1.0f,
                    > 0.6f => 0.85f,
                    > 0.45f => 0.6f,
                    > 0.3f => 0.45f,
                    > 0.15f => 0.3f,
                    _ => 0.15f
                };
            }

            // Grab largest pressure value and use it to determine the spatial level
            var maxPressure = Mathf.Max(pressureValues);
            var spatialLevel = maxPressure switch
            {
                > 0.85f => level6,
                > 0.6f => level5,
                > 0.45f => level4,
                > 0.3f => level3,
                > 0.15f => level2,
                0.0f => level0,
                _ => level1
            };
            // Debug.Log(spatialLevel.Count);

            // Define a list where each element is 1f if the index is in the spatial level, 0f otherwise
            // NOTE: This requires remapping of the spatial level indices
            var spatialValues = new float[32];
            for (var i = 0; i < 32; i++)
            {
                spatialValues[remapStrip[i] - 1] = spatialLevel.Contains(i + 1)
                    ? 1f
                    : 0f;
            }

            // Return ModulationData object
            return new ModulationData
            {
                Type = ModulationType.Pad,
                Values = spatialValues
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This modulator only supports the "glove" device at the moment
            return deviceConfig.deviceName == "strip";
        }
    }
}
