using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tactility.Box;
using UnityEngine;
using static Tactility.Calibration.CalibrationManager;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(AbstractBoxController))]
    public class TactilityManager : MonoBehaviour
    {
        [Tooltip("The time interval in milliseconds at which to send modulation data to the device. This value should be greater than the device's minimum update interval.")]
        public float updateInterval = 100f;

        private AbstractBoxController _boxController;
        private float _lastSendTime;
        
        private readonly List<AbstractModulator> _modulators = new List<AbstractModulator>();

        private int[] _combinedPads;
        private float[] _combinedAmps;
        private int[] _combinedWidths;
        private int _frequency;

        protected void Start()
        {
            _boxController = GetComponent<AbstractBoxController>();
            _lastSendTime = Time.time * 1000; // Convert to milliseconds
            InitializeModulationDataArrays();
        }

        protected void Update()
        {
            if (Time.time * 1000 - _lastSendTime < updateInterval)
            {
                return;
            }

            var wasSuccessful = NotifyModulators();
            if (!wasSuccessful)
            {
                _boxController.ResetAllPads();
                return;
            }
            
            SendCombinedModulationData();
            ResetCombinedModulationData(); // Reset for next cycle
            _lastSendTime = Time.time * 1000;
        }

        private void InitializeModulationDataArrays()
        {
            var padCount = DeviceConfig.numPads;
            _combinedAmps = new float[padCount];
            _combinedWidths = new int[padCount];
            _frequency = DeviceConfig.baseFreq;
            
            // Initialize _combinedPads with all 1s to default them on
            _combinedPads = new int[padCount];
            for (var i = 0; i < padCount; i++)
            {
                _combinedPads[i] = 1;
            }
        }

        private bool NotifyModulators()
        {
            // We just do this to keep track of if we've received all the required data
            var suppliedData = new Dictionary<ModulationType, bool>
            {
                { ModulationType.Amplitude, false },
                { ModulationType.Width, false },
            };
            
            // Notify each modulator to get its modulation data
            foreach (var modulator in _modulators)
            {
                var modulationData = modulator.GetModulationData();
                if (modulationData.HasValue)
                {
                    CombineModulationData(modulationData.Value);
                    suppliedData[modulationData.Value.Type] = true;
                }
            }
            
            // Return false if any required data was not supplied
            if (!suppliedData.Values.All(value => value))
            {
                return false;
            }

            // Check if every TactilityData type is contained in _modulators
            // Apply the given data types default values if not
            foreach (ModulationType type in Enum.GetValues(typeof(ModulationType)))
            {
                if (_modulators.TrueForAll(modulator => modulator.GetModulationData()?.Type != type))
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (type)
                    {
                        case ModulationType.Pad:
                            for (var i = 0; i < _combinedPads.Length; i++)
                            {
                                _combinedPads[i] = 1;
                            }
                            break;
                        case ModulationType.Amplitude:
                            for (var i = 0; i < _combinedAmps.Length; i++)
                            {
                                _combinedAmps[i] = BaseAmps[i];
                            }
                            break;
                        case ModulationType.Width:
                            for (var i = 0; i < _combinedWidths.Length; i++)
                            {
                                _combinedWidths[i] = BaseWidths[i];
                            }
                            break;
                        // TODO: Add frequency?..
                    }
                }
            }

            return true;
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void CombineModulationData(ModulationData modulationData)
        {
            if (modulationData.Values == null)
            {
                return;
            }

            // If we're dealing with frequency, check that the values are of length 1
            if (modulationData.Type == ModulationType.Frequency && modulationData.Values.Length != 1)
            {
                throw new ArgumentException("Frequency modulation data must be of length 1");
            }

            // Check that the modulation data is the correct length
            if (modulationData.Type != ModulationType.Frequency && modulationData.Values.Length != _combinedPads.Length)
            {
                throw new ArgumentException($"Modulation data length ({modulationData.Values.Length}) does not match the number of pads ({_combinedAmps.Length})");
            }

            // Check that the modulation data is not negative
            if (Array.Exists(modulationData.Values, value => value < 0))
            {
                throw new ArgumentException("Modulation data cannot contain negative values");
            }

            // Check that modulation data is not greater than the device's max values
            switch (modulationData.Type)
            {
                case ModulationType.Amplitude when Array.Exists(modulationData.Values, value => value > DeviceConfig.maxAmp):
                    throw new ArgumentException("Modulation data cannot exceed the device's max amplitude");
                case ModulationType.Width when Array.Exists(modulationData.Values, value => value > DeviceConfig.maxWidth):
                    throw new ArgumentException("Modulation data cannot exceed the device's max pulse width");
                case ModulationType.Frequency when Array.Exists(modulationData.Values, value => value > DeviceConfig.maxFreq):
                    throw new ArgumentException("Modulation data cannot exceed the device's max frequency");
            }

            for (var i = 0; i < modulationData.Values.Length; i++)
            {
                switch (modulationData.Type)
                {
                    case ModulationType.Pad:
                        var value = modulationData.Values[i] > 0 ? 1 : 0;
                        _combinedPads[i] = value;
                        break;
                    case ModulationType.Amplitude:
                        _combinedAmps[i] = Mathf.Max(_combinedAmps[i], modulationData.Values[i]);
                        break;
                    case ModulationType.Width:
                        _combinedWidths[i] = Mathf.Max(_combinedWidths[i], (int)modulationData.Values[i]);
                        break;
                    case ModulationType.Frequency:
                        _frequency = (int)modulationData.Values[i];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SendCombinedModulationData()
        {
            if (_modulators.Count == 0)
            {
                return;
            }

            var encodedString = _boxController.GetStimString(_combinedPads, _combinedAmps, _combinedWidths);
            var freqString = _boxController.GetFreqString(_frequency);
            _boxController.Send(encodedString);
            _boxController.Send(freqString);
        }

        private void ResetCombinedModulationData()
        {
            Array.Clear(_combinedAmps, 0, _combinedAmps.Length);
            Array.Clear(_combinedWidths, 0, _combinedWidths.Length);
            _frequency = DeviceConfig.baseFreq;
        }

        public void Subscribe(AbstractModulator modulator)
        {
            if (!modulator.IsCompatibleWithDevice(DeviceConfig))
            {
                throw new ArgumentException($"Modulator {modulator} has declared itself incompatible with the current device ({DeviceConfig.deviceName})");
            }

            if (!_modulators.Contains(modulator))
            {
                _modulators.Add(modulator);
                if (_modulators.Count == 1)
                {
                    _boxController.EnableStimulation();
                }
            }
            else
            {
                Debug.LogWarning($"Modulator {modulator} is already subscribed to TactilityManager");
            }
        }

        public void Unsubscribe(AbstractModulator modulator)
        {
            if (_modulators.Contains(modulator))
            {
                _modulators.Remove(modulator);
            }

            if (_modulators.Count == 0)
            {
                _boxController.DisableStimulation();
            }
        }
    }
}