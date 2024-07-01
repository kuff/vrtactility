// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tactility.Box;
using UnityEngine;
using static Tactility.Calibration.CalibrationManager;
using static UnityEditor.ShaderData;
#endregion

namespace Tactility.Modulation
{
    public class TactilityManager : MonoBehaviour
    {
        [Tooltip("The time interval in milliseconds at which to send modulation data to the device. This value should be greater than the device's minimum update interval.")]
        public float updateInterval = 100f;

        private readonly List<AbstractModulator> _modulators = new List<AbstractModulator>();

        private AbstractBoxController _boxController;
        private float[] _combinedAmps;

        private int[] _combinedPads;
        private int[] _combinedWidths;
        private int _frequency;
        private float _lastSendTime;
        private bool _hasNoData;

        private int[] _prevPads;
        private float[] _prevAmps;
        private int[] _prevWidths;
        private int _prevFrequency;

        protected void Start()
        {
            _boxController = FindObjectOfType<AbstractBoxController>();
            _lastSendTime = Time.time * 1000; // Convert to milliseconds
            InitializeModulationDataArrays();
        }

        protected void Update()
        {
            if (Time.time * 1000 - _lastSendTime < updateInterval)
            {
                return;
            }
            
            var wasDataRetrieved = NotifyModulators();
            if (!wasDataRetrieved)
            {
                if (_hasNoData)
                {
                    return;
                }
                
                _hasNoData = true;

                if (_boxController.IsStimEnabled())
                {
                    _boxController.ResetAllPads();
                    _prevPads = null;
                    _prevAmps = null;
                    _prevWidths = null;
                }
            }
            else
            {
                _hasNoData = false;
                SendCombinedModulationData();
                _lastSendTime = Time.time * 1000;
            }

            ResetCombinedModulationData(); // Reset for next cycle
        }

        private void InitializeModulationDataArrays()
        {
            var padCount = DeviceConfig.numPads;
            _combinedAmps = new float[padCount];
            _combinedWidths = new int[padCount];
            _frequency = DeviceConfig.baseFreq;
            SetDefaultPadValues();
        }
        
        private void SetDefaultPadValues()
        {
            var padCount = DeviceConfig.numPads;
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
                { ModulationType.Width, false }
            };
            var wasPadDataSupplied = false;

            // Notify each modulator to get its modulation data
            foreach (var modulationData in _modulators.Select(modulator => modulator.GetModulationData()).Where(modulationData => modulationData.HasValue))
            {
                CombineModulationData(modulationData.Value);
                suppliedData[modulationData.Value.Type] = true;
                if (modulationData.Value.Type == ModulationType.Pad)
                {
                    wasPadDataSupplied = true;
                }
            }

            // Pad modulator is not required, but if it's not supplied, we need to set default values
            if (!wasPadDataSupplied)
            {
                SetDefaultPadValues();
            }

            // Return false if any required data was not supplied
            if (!suppliedData.Values.All(value => value))
            {
                return false;
            }

            // Check if every TactilityData type is contained in _modulators, applying the given data types default values if not
            foreach (ModulationType type in Enum.GetValues(typeof(ModulationType)))
            {
                if (_modulators.TrueForAll(modulator => modulator.GetModulationData()?.Type != type))
                {
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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
                        var value = modulationData.Values[i] > 0
                            ? 1
                            : 0;
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
                        throw new ArgumentOutOfRangeException(nameof(modulationData), modulationData.Type, "Invalid modulation type");
                }
            }
        }

        private void SendCombinedModulationData()
        {
            if (_modulators.Count == 0)
            {
                return;
            }

            var freqString = _boxController.GetFreqString(_frequency, _prevFrequency);
            var isFreqNew = !string.IsNullOrEmpty(freqString);

            // Send the stim string if frequency has changed regardless of if the values are new or not
            var encodedString = _boxController.GetStimString(_combinedPads, _combinedAmps, _combinedWidths, isFreqNew ? null : _prevPads, isFreqNew ? null : _prevAmps, isFreqNew ? null : _prevWidths);

            if (!string.IsNullOrEmpty(encodedString))
            {
                _boxController.Send(encodedString);
            }
            if (isFreqNew)
            {
                _boxController.Send(freqString);
            }

            _prevPads = (int[])_combinedPads.Clone();
            _prevAmps = (float[])_combinedAmps.Clone();
            _prevWidths = (int[])_combinedWidths.Clone();
            _prevFrequency = _frequency;
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

            // TODO: Move this inside the above if statement
            if (_modulators.Count == 0)
            {
                _boxController.DisableStimulation();
            }
        }
    }
}
