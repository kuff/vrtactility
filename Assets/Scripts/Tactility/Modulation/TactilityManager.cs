using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tactility.Box;
using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(AbstractBoxController))]
    public class TactilityManager : MonoBehaviour
    {
        [Tooltip("The time interval in milliseconds at which to send modulation data to the device. This value should be " +
                 "greater than the device's minimum update interval.")]
        public float updateInterval = 100f;

        private AbstractBoxController _boxController;
        private float _lastSendTime;
        
        private readonly List<AbstractModulator> _modulators = new();

        // Store the highest value for each pad
        private int[] _combinedPads;
        private float[] _combinedAmps;
        private int[] _combinedWidths;
        private int _frequency;

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
            _combinedWidths = new int[padCount];
            _frequency = CalibrationManager.DeviceConfig.baseFreq;
            
            // Initialize _combinedPads with all 1s to default them on
            _combinedPads = new int[padCount];
            for (var i = 0; i < padCount; i++) _combinedPads[i] = 1;
        }

        private void NotifyModulators()
        {
            // Notify each modulator to get its modulation data
            // Also check if any pad data was supplied, otherwise enable all pads
            // var padDataSupplied = false;
            // foreach (var modulator in _modulators)
            // {
            //     var modulationData = modulator.GetModulationData();
            //     CombineModulationData(modulationData);
            //     if (modulationData.Type == ModulationType.Pad) padDataSupplied = true;
            // }
            //
            // if (padDataSupplied) return;
            // for (var i = 0; i < _combinedPads.Length; i++) _combinedPads[i] = 1;
            // Notify each modulator to get its modulation data
            foreach (var modulator in _modulators)
            {
                var modulationData = modulator.GetModulationData();
                CombineModulationData(modulationData);
            }
            
            // Check if every TactilityData type is contained in _modulators
            // Apply the given data types default values if not
            foreach (ModulationType type in Enum.GetValues(typeof(ModulationType)))
            {
                if (_modulators.TrueForAll(modulator => modulator.GetModulationData().Type != type))
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (type)
                    {
                        case ModulationType.Pad:
                            for (var i = 0; i < _combinedPads.Length; i++) _combinedPads[i] = 1;
                            break;
                        case ModulationType.Amplitude:
                            for (var i = 0; i < _combinedAmps.Length; i++) _combinedAmps[i] = CalibrationManager.BaseAmps[i];
                            break;
                        case ModulationType.Width:
                            for (var i = 0; i < _combinedWidths.Length; i++) _combinedWidths[i] = CalibrationManager.BaseWidths[i];
                            break;
                        // TODO: Add frequency?..
                    }
                }
            }
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void CombineModulationData(ModulationData modulationData)
        {
            if (modulationData.Values == null) return;

            // If we're dealing with frequency, check that the values are of length 1
            if (modulationData.Type == ModulationType.Frequency && modulationData.Values.Length != 1)
                throw new ArgumentException("Frequency modulation data must be of length 1");
            
            // Check that the modulation data is the correct length
            if (modulationData.Type != ModulationType.Frequency && modulationData.Values.Length != _combinedPads.Length)
                throw new ArgumentException($"Modulation data length ({modulationData.Values.Length}) does not match " +
                                            $"the number of pads ({_combinedAmps.Length})");
            
            // Check that the modulation data is not negative
            if (Array.Exists(modulationData.Values, value => value < 0))
                throw new ArgumentException("Modulation data cannot contain negative values");

            // Check that modulation data is not greater than the device's max values
            switch (modulationData.Type)
            {
                case ModulationType.Amplitude when Array.Exists(modulationData.Values, value => value > CalibrationManager.DeviceConfig.maxAmp):
                    throw new ArgumentException("Modulation data cannot exceed the device's max amplitude");
                case ModulationType.Width when Array.Exists(modulationData.Values, value => value > CalibrationManager.DeviceConfig.maxWidth):
                    throw new ArgumentException("Modulation data cannot exceed the device's max pulse width");
                case ModulationType.Frequency when Array.Exists(modulationData.Values, value => value > CalibrationManager.DeviceConfig.maxFreq):
                    throw new ArgumentException("Modulation data cannot exceed the device's max frequency");
            }

            // Check that the modulation data is not all zeros in amplitude or width
            // switch (modulationData.Type)
            // {
            //     case ModulationType.Amplitude when Array.TrueForAll(modulationData.Values, value => value.Equals(0)):
            //         throw new ArgumentException("Modulation data cannot be all zeros for amplitude");
            //     case ModulationType.Width when Array.TrueForAll(modulationData.Values, value => value.Equals(0)):
            //         throw new ArgumentException("Modulation data cannot be all zeros for pulse width");
            // }
            
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
                        _frequency = Mathf.Max(_frequency, (int)modulationData.Values[i]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SendCombinedModulationData()
        {
            if (_modulators.Count == 0) return;
            
            //var freqString = _boxController.GetFrequencyString(_frequency);
            var encodedString = _boxController.GetStimString(_combinedPads, _combinedAmps, _combinedWidths);
            //_boxController.SendMany(new []{ freqString, encodedString });
            _boxController.Send(encodedString);
        }

        private void ResetCombinedModulationData()
        {
            Array.Clear(_combinedAmps, 0, _combinedAmps.Length);
            Array.Clear(_combinedWidths, 0, _combinedWidths.Length);
            _frequency = CalibrationManager.DeviceConfig.baseFreq;
        }

        public void Subscribe(AbstractModulator modulator)
        {
            // Find out which derived class the modulator is
            /*switch (modulator)
            {
                case ConstantAmpModulator _:
                    break;
                case ConstantWidthModulator _:
                    break;
                case StepwisePadModulator _:
                    break;
                case StepwiseWidthModulator _:
                    break;
                default:
                    throw new ArgumentException($"Modulator {modulator} is not a valid derived class of AbstractModulator");
            }*/
            
            if (!modulator.IsCompatibleWithDevice(CalibrationManager.DeviceConfig))
                throw new ArgumentException($"Modulator {modulator} has declared itself incompatible with the " +
                                            $"current device ({CalibrationManager.DeviceConfig.deviceName})");
            
            if (!_modulators.Contains(modulator))
            {
                _modulators.Add(modulator);
                if (_modulators.Count == 1) _boxController.EnableStimulation();
            }
            else
                Debug.LogWarning($"Modulator {modulator} is already subscribed to TactilityManager");
        }

        public void Unsubscribe(AbstractModulator modulator)
        {
            if (_modulators.Contains(modulator))
                _modulators.Remove(modulator);
            
            if (_modulators.Count == 0) _boxController.DisableStimulation();
        }
    }
}