using System;
using System.Collections;
using Tactility.Calibration;
using UnityEngine;

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
        public ModulationType Type;
        public float[] Values;
    }
    
    public abstract class AbstractModulator : MonoBehaviour
    {
        private TactilityManager _tactilityManager;
        // ReSharper disable once NotAccessedField.Local
        private CalibrationManager _calibrationManager;

        protected virtual IEnumerator Start()
        {
            _tactilityManager = FindObjectOfType<TactilityManager>();
            _calibrationManager = CalibrationManager.Instance;

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
                Debug.LogWarning($"Modulator {GetType().Name} is not compatible with the device. Disabling. {e}");
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