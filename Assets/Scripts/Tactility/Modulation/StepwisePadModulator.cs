// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections;
using System.Collections.Generic;
using Tactility.Calibration;
using Tactility.Task;
using UnityEngine;
#endregion

namespace Tactility.Modulation
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwisePadModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;
        private GrabAndMoveScenario _grabScenario;

        protected override IEnumerator Start()
        {
            yield return base.Start();

            _dataProvider = GetComponent<ITactilityDataProvider>();
            _grabScenario = FindObjectOfType<GrabAndMoveScenario>();

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
            var level0 = new List<int> { 28 };  //one random pad for which i'm sure amp is not calibrated (0.5)
            var level1 = new List<int> { 4, 10 };
            var level2 = new List<int> { 4, 10, 1, 7 };
            var level3 = new List<int> { 4, 10, 1, 7, 3, 9 };
            var level4 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11 };
            var level5 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11, 0, 6 };
            var level6 = new List<int> { 4, 10, 1, 7, 3, 9, 5, 11, 0, 6, 2, 8 };

            var spatialLevel = new List<int>();

            if (_grabScenario == null)
            {
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
                        > 0.75f => 1f,
                        > 0.62f => 0.75f,
                        > 0.49f => 0.62f,
                        > 0.36f => 0.49f,
                        > 0.23f => 0.36f,
                        > 0.1f => 0.23f,
                        _ => 0.0f
                    };
                }

                // Grab largest pressure value and use it to determine the spatial level
                var maxPressure = Mathf.Max(pressureValues);
                spatialLevel = maxPressure switch
                {
                    > 0.75f => level6,
                    > 0.62f => level5,
                    > 0.49f => level4,
                    > 0.36f => level3,
                    > 0.23f => level2,
                    > 0.1f => level1,
                    _ => level0
                };
            }

            else
            {
                var grabScenario = _grabScenario.currentForceLevel;
                spatialLevel = grabScenario switch
                    {
                        6 => level6,
                        5 => level5,
                        4 => level4,
                        3 => level3,
                        2 => level2,
                        1 => level1,
                        _ => level0
                    };
            }

            
            // Define a list where each element is 1f if the index is in the spatial level, 0f otherwise
            // NOTE: This requires remapping of the spatial level indices
            var spatialValues = new float[32];
            for (var i = 0; i < 32; i++)
            {
                spatialValues[remapStrip[i] - 1] = spatialLevel.Contains(i)
                    ? 1f
                    : 0f;
                // Debug.Log($"{i}: {spatialValues[remapStrip[i] - 1]}");
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
