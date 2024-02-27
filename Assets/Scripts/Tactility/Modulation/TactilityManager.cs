using System;
using System.Collections.Generic;
using Tactility.Box;
using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(AbstractBoxController))]
    public class TactilityManager : MonoBehaviour
    {
        public float updateInterval = 100f;

        private AbstractBoxController _boxController;
        private float _lastSendTime;

        private readonly List<AbstractModulator> _modulators = new();

        // Store the highest value for each pad
        private float[] _combinedAmps;
        private float[] _combinedWidths;

        protected void Start()
        {
            _boxController = GetComponent<AbstractBoxController>();
            _lastSendTime = Time.time * 1000;  // Convert to milliseconds
            InitializeModulationDataArrays();
        }

        protected void Update()
        {
            if (Time.time * 1000 - _lastSendTime < updateInterval) return;

            NotifyModulators();
            SendCombinedModulationData();
            ResetCombinedModulationData();  // Reset for next cycle
            _lastSendTime = Time.time * 1000;
        }

        private void InitializeModulationDataArrays()
        {
            var padCount = CalibrationManager.DeviceConfig.numPads;
            _combinedAmps = new float[padCount];
            _combinedWidths = new float[padCount];
        }

        private void NotifyModulators()
        {
            foreach (var modulator in _modulators)
            {
                var modulationData = modulator.GetModulationData();
                CombineModulationData(modulationData);
            }
        }

        private void CombineModulationData(ModulationData modulationData)
        {
            if (modulationData.Values == null) return;

            // Check that the modulation data is the correct length
            if (modulationData.Values.Length != _combinedAmps.Length)
                throw new ArgumentException($"Modulation data length ({modulationData.Values.Length}) does not match " +
                                            $"the number of pads ({_combinedAmps.Length})");
            
            // Check that the modulation data is not negative
            if (Array.Exists(modulationData.Values, value => value < 0))
                throw new ArgumentException("Modulation data cannot contain negative values");
            
            // Check that modulation data is not greater than the device's max values
            if (modulationData.Type == ModulationType.Amp)
            {
                if (Array.Exists(modulationData.Values, value => value > CalibrationManager.DeviceConfig.maxAmp))
                    throw new ArgumentException("Modulation data cannot exceed the device's max amplitude");
            }
            else
            {
                if (Array.Exists(modulationData.Values, value => value > CalibrationManager.DeviceConfig.maxWidth))
                    throw new ArgumentException("Modulation data cannot exceed the device's max pulse width");
            }
            
            // Check that the modulation data is not all zeros in amplitude or width
            if (modulationData.Type == ModulationType.Amp)
            {
                if (Array.TrueForAll(modulationData.Values, value => value.Equals(0)))
                    throw new ArgumentException("Modulation data cannot be all zeros for amplitude");
            }
            else
            {
                if (Array.TrueForAll(modulationData.Values, value => value.Equals(0)))
                    throw new ArgumentException("Modulation data cannot be all zeros for pulse width");
            }
            
            for (var i = 0; i < modulationData.Values.Length; i++)
            {
                switch (modulationData.Type)
                {
                    case ModulationType.Amp:
                        _combinedAmps[i] = Mathf.Max(_combinedAmps[i], modulationData.Values[i]);
                        break;
                    case ModulationType.Width:
                        _combinedWidths[i] = Mathf.Max(_combinedWidths[i], modulationData.Values[i]);
                        break;
                }
            }
        }

        private void SendCombinedModulationData()
        {
            var encodedString = _boxController.GetEncodedString(_combinedAmps, _combinedWidths);
            _boxController.Send(encodedString);
        }

        private void ResetCombinedModulationData()
        {
            Array.Clear(_combinedAmps, 0, _combinedAmps.Length);
            Array.Clear(_combinedWidths, 0, _combinedWidths.Length);
        }

        public void Subscribe(AbstractModulator modulator)
        {
            if (!modulator.IsCompatibleWithDevice(CalibrationManager.DeviceConfig))
                throw new ArgumentException($"Modulator {modulator} has declared itself as incompatible with " +
                                            $"the current device ({CalibrationManager.DeviceConfig.deviceName})");
            
            if (!_modulators.Contains(modulator))
                _modulators.Add(modulator);
            else
                Debug.LogWarning($"Modulator {modulator} is already subscribed to TactilityManager");
        }

        public void Unsubscribe(AbstractModulator modulator)
        {
            if (_modulators.Contains(modulator))
                _modulators.Remove(modulator);
        }
    }
}