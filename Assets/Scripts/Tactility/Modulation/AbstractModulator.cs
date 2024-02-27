using System;
using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(TactilityManager))]
    public abstract class AbstractModulator : MonoBehaviour
    {
        private TactilityManager _tactilityManager;
        // ReSharper disable once NotAccessedField.Local
        private CalibrationManager _calibrationManager;

        protected virtual void Start()
        {
            _tactilityManager = GetComponent<TactilityManager>();
            _calibrationManager = CalibrationManager.Instance;

            try
            {
                _tactilityManager.Subscribe(this);
            }
            catch (ArgumentException e)
            {
                // Disable the modulator if it's not compatible with the device and let the user know
                Debug.LogWarning($"Modulator {GetType().Name} is not compatible with the device. Disabling. {e}");
                enabled = false;
            }
        }
        
        private void OnDestroy()
        {
            _tactilityManager.Unsubscribe(this);
        }

        public abstract ModulationData GetModulationData();
        public abstract bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig);
    }
}
