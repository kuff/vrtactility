using System;
using Internal.Box;
using UnityEngine;

namespace Internal.Modulation
{
    [RequireComponent(typeof(AbstractBoxController))]
    [RequireComponent(typeof(IDeviceStringEncoder))]
    public class TactilityManager : MonoBehaviour
    {
        [Tooltip("Millisecond interval in which modulation data is sent to the device if there is any data.")]
        public float updateInterval = 100f;
        
        private AbstractBoxController _boxController;
        private IDeviceStringEncoder _deviceStringEncoder;
        
        private float[] _pads;
        private float[] _amps;
        private float[] _widths;
        
        private bool _hasBeenUpdated;
        private float _lastSendTime;

        protected void Start()
        {
            _boxController = GetComponent<AbstractBoxController>();
            _deviceStringEncoder = GetComponent<IDeviceStringEncoder>();
            
            _lastSendTime = Time.time * 1000;  // Convert to milliseconds
        }

        protected void Update()
        {
            if (Time.time * 1000 - _lastSendTime >= updateInterval && _hasBeenUpdated)
            {
                var encodedString = _deviceStringEncoder.EncodeCommandString(_pads, _amps, _widths);
                _boxController.Send(encodedString);
                _lastSendTime = Time.time * 1000;  // Update last send time in milliseconds
                
                ResetModulationData();
            }
        }

        private void ResetModulationData()
        {
            // Reset the arrays
            _pads = null;
            _amps = null;
            _widths = null;
            
            // Reset the flag
            _hasBeenUpdated = false;
        }

        public void UpdateModulation(ModulationData modulationData)
        {
            _hasBeenUpdated = true;  // Set the flag to indicate that modulation data has been updated
            
            switch (modulationData.Type)
            {
                case ModulationType.Amp:
                    _amps = modulationData.Values;
                    break;
                case ModulationType.Pad:
                    _pads = modulationData.Values;
                    break;
                case ModulationType.Width:
                    _widths = modulationData.Values;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
