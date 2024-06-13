// Copyright (C) 2024 Peter Leth

#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tactility.Calibration;
using UnityEngine;
using static Tactility.Calibration.CalibrationManager;
#endregion

namespace Tactility.Modulation
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwiseFreqModulator : AbstractModulator
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
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwiseFreqModulator.");
            enabled = false;
        }

        public override ModulationData? GetModulationData()
        {
            if (!_dataProvider.IsActive())
            {
                return null;
            }

            ref var modulationData = ref _dataProvider.GetTactilityData();

            var remap_strip = new[] { 31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18 };
            var activePads = new List<int> { 4, 10, 1, 7 }; //we need only two pads for finger active! 

            // Update stimuli for each touching finger bone of interest
            var valueBatch = new float[5];
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

            var pressureValue = valueBatch.Max() switch
            {
                > 0.85f => 1.0f,
                > 0.6f => 0.7f,
                > 0.45f => 0.4f,
                > 0.3f => 0.26f,
                > 0.15f => 0.15f,
                > 0f => 0.8f,
                _ => 0f
            };

            var freqValue = DeviceConfig.baseFreq * pressureValue;
            // Debug.Log($"Pressure value: {freqValue}");

            return new ModulationData
            {
                Type = ModulationType.Frequency,
                Values = new[] { freqValue }
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should in theory work with every device...
            return true;
        }
    }
}
