// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections;
using Tactility.Calibration;
using UnityEngine;
using static Tactility.Calibration.CalibrationManager;
#endregion

// ReSharper disable Unity.NoNullPropagation

namespace Tactility.Modulation
{
    public enum ModulationType
    {
        Pad,
        Amplitude,
        Width,
        Frequency
    }

    public struct ModulationData
    {
        public ModulationType Type { get; set; }
        public float[] Values { get; set; }
        public float[] MappedValues
        {
            get
            {
                // Remap values using CalibrationManager.DeviceConfig.mapping
                var remappedValues = new float[Values.Length];
                for (var i = 0; i < Values.Length; i++)
                {
                    remappedValues[i] = GetMappedValue(i);
                }
                return remappedValues;
            }
        }
        public float GetMappedValue(int index)
        {
            // Handle the case where DeviceConfig.mapping is null or empty
            return DeviceConfig.mapping == null || DeviceConfig.mapping.Length == 0
                ? Values[index]
                : Values[DeviceConfig.mapping[index] - 1];
        }
    }

    public abstract class AbstractModulator : MonoBehaviour
    {
        private TactilityManager _tactilityManager;

        protected virtual IEnumerator Start()
        {
            _tactilityManager = FindObjectOfType<TactilityManager>();

            // Wait for 100 milliseconds for the dependencies of TactilityManager to populate
            yield return new WaitForSeconds(0.1f);
            _tactilityManager.Subscribe(this);
        }

        protected virtual void OnEnable()
        {
            try
            {
                _tactilityManager?.Subscribe(this);
            }
            catch (ArgumentException e)
            {
                // Disable the modulator if it's not compatible with the device and let the user know
                Debug.LogWarning($"Modulator {GetType().Name} is not compatible with the current device and will be disabled. {e}");
                enabled = false;
            }
        }

        protected virtual void OnDisable()
        {
            _tactilityManager?.Unsubscribe(this);
        }

        public abstract ModulationData? GetModulationData();
        public abstract bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig);
    }
}
