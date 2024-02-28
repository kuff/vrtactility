using System.Collections;
using System.Collections.Generic;
using Tactility.Calibration;
using UnityEngine;

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
            if (_dataProvider != null) yield break;
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwisePadModulator.");
            enabled = false;
        }
        
        public override ModulationData GetModulationData()
        {
            ref var modulationData = ref _dataProvider.GetTactilityData();
            var remap = new[] { 30, 27, 29, 28, 25, 31, 32, 26, 17, 18, 20, 1, 2, 22, 19, 3, 23, 21, 24, 4, 5, 8, 9, 6, 7, 10, 13, 14, 11, 12, 15, 16 };
            
            // Define spatial levels (values require remapping)
            var level1 = new List<int> { 1, 9, 22, 27, 32 };
            var level2 = new List<int> { 1, 2, 9, 10, 22, 23, 27, 28, 32 };
            var level3 = new List<int> { 1, 2, 3, 5, 9, 10, 11, 13, 22, 23, 24, 27, 28, 29, 32 };
            var level4 = new List<int> { 1, 2, 3, 5, 7, 9, 10, 11, 13, 15, 22, 23, 24, 26, 27, 28, 29, 31, 32 };
            
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

            var pressureValues = new float[32];
            for (var i = 0; i < 32; i++)
            {
                // Use remap value to determine which finger pressure value we use
                var pressureValue = i switch
                {
                    < 8 => valueBatch[0],
                    < 21 => valueBatch[1],
                    < 26 => valueBatch[2],
                    < 31 => valueBatch[3],
                    _ => valueBatch[4] // == 31
                };

                // Define value buckets (0.25, 0.5, 0.75, 1.0) and project pressureValue to last bucket it is greater than
                pressureValues[i] = pressureValue switch
                {
                    > 0.75f => 1.0f,
                    > 0.5f => 0.75f,
                    > 0.25f => 0.5f,
                    _ => 0.25f
                };
            }
            
            // Grab largest pressure value and use it to determine the spatial level
            var maxPressure = Mathf.Max(pressureValues);
            var spatialLevel = maxPressure switch
            {
                > 0.75f => level4,
                > 0.5f => level3,
                > 0.25f => level2,
                _ => level1
            };
            // Debug.Log(spatialLevel.Count);
            
            // Define a list where each element is 1f if the index is in the spatial level, 0f otherwise
            // NOTE: This requires remapping of the spatial level indices
            var spatialValues = new float[32];
            for (var i = 0; i < 32; i++) 
                spatialValues[remap[i] - 1] = spatialLevel.Contains(i + 1) ? 1f : 0f;
            
            // Return ModulationData object
            return new ModulationData()
            {
                Type = ModulationType.Pad,
                Values = spatialValues
            };
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This modulator only supports the "glove" device at the moment
            return deviceConfig.deviceName == "glove";
        }
    }
}
