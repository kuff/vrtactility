using System;
using System.Collections.Generic;
using Tactility.Box;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(AbstractBoxController))]
    public class TactilityManager : MonoBehaviour
    {
        [Tooltip("Millisecond interval in which modulation data is sent to the device if there is any data.")]
        public float updateInterval = 100f;
        
        private AbstractBoxController _boxController;

        private float[] _amps;
        private float[] _widths;
        
        private bool _newValuesSubmitted;
        private float _lastSendTime;

        protected void Start()
        {
            _boxController = GetComponent<AbstractBoxController>();
            _lastSendTime = Time.time * 1000;  // Convert to milliseconds
        }

        protected void Update()
        {
            // Only update after enough time has passed
            if (!(Time.time * 1000 - _lastSendTime >= updateInterval)) 
                return;
            
            // Only update if new values have been submitted since last update
            if (!_newValuesSubmitted)
            {
                // This is behaviour inherited from the previous implementation
                _boxController.ResetAllPads();  // Consider replacing with _boxController.DisableStimulation()
                return;
            }
            
            var encodedString = _boxController.GetEncodedString(_amps, _widths);
            _boxController.Send(encodedString);
            _lastSendTime = Time.time * 1000;  // Update last send time in milliseconds
                
            ResetModulationData();
        }

        private void ResetModulationData()
        {
            // Reset the arrays
            _amps = null;
            _widths = null;
            
            // Reset the flag
            _newValuesSubmitted = false;
        }
        
        // TODO: Below this will be refactored into a subscription model...

        public void UpdateModulation(ModulationData modulation)
        {
            _newValuesSubmitted = true;  // Set the flag to indicate that modulation data has been updated
            
            switch (modulation.Type)
            {
                case ModulationType.Amp:
                    _amps = modulation.Values;
                    break;
                case ModulationType.Pad:
                    break;
                case ModulationType.Width:
                    _widths = modulation.Values;
                    break;
                default:
                    // This should never happen
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void UpdateModulation(IEnumerable<ModulationData> modulations)
        {
            foreach (var data in modulations)
                UpdateModulation(data);
        }
    }
}
