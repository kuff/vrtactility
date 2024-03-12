using System.Collections;
using System.Linq;
using Tactility.Calibration;
using UnityEngine;

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
            if (_dataProvider != null) yield break;
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwiseFreqModulator.");
            enabled = false;
        }
        
        public override ModulationData? GetModulationData()
        {
            if (!_dataProvider.IsActive()) return null;
            
            ref var modulationData = ref _dataProvider.GetTactilityData();
            
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
                    case OVRSkeleton.BoneId.Hand_Middle3:
                        valueBatch[2] = pressure;
                        break;
                    case OVRSkeleton.BoneId.Hand_Ring3:
                        valueBatch[3] = pressure;
                        break;
                    case OVRSkeleton.BoneId.Hand_Pinky3:
                        valueBatch[4] = pressure;
                        break;
                }
            }

            var pressureValue = valueBatch.Max() switch
            {
                > 0.75f => 1.0f,
                > 0.5f => 0.75f,
                > 0.25f => 0.5f,
                _ => 0.25f
            };
            
            var config = CalibrationManager.DeviceConfig;
            var freqValue = config.baseFreq * pressureValue;
            // Debug.Log($"Pressure value: {freqValue}");
            
            return new ModulationData()
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