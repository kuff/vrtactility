using System.Globalization;
using System.Text.RegularExpressions;
using Tactility.Calibration;
using UnityEngine;

// ReSharper disable StringLiteralTypo

namespace Tactility.Box
{
    public class GammaBoxController : AbstractBoxController
    {
        [SerializeField]
        [Tooltip("If enabled, the messenger will automatically attempt to connect using the specified serial port " +
                 "upon the game start. Ensure the correct port name is set in the SerialController. This is useful " +
                 "for scenarios where an immediate connection is desirable without requiring an explicit user action " +
                 "to initiate the connection.")]
        private bool connectOnAwake;
        
        private TactilityDeviceConfig _config;
        private bool _isSuccessfullyConnected;
        private bool _receivedValidGreeting;

        protected override void Start()
        {
            base.Start();
            
            _config = CalibrationManager.DeviceConfig;
            //Sc.SetTearDownFunction(DisableStimulation);
            if (connectOnAwake) Connect(Sc.portName);
        }

        public override void Connect(string port)
        {
            Sc.portName = port;
            Port = port;
        
            Sc.enabled = true;
            SendMany(new[]
            {
                "iam TACTILITY", 
                $"elec 1 *pads_qty {_config.numPads}", 
                "battery ?", 
                //$"freq {_config.baseFreq}"
            });
        }

        public override void EnableStimulation()
        {
            Send("stim on");
        }

        public override void DisableStimulation()
        {
            // Enclose in try/catch block
            try
            {
                Send("stim off");
            }
            catch (System.NullReferenceException e)
            {
                Debug.LogWarning("GammaBoxController.DisableStimulation errored, assuming the serial thread was already halted: " + e.Message);
            }
        }

        public override void ResetAllPads()
        {
            // NOTE: This was done in the previous implementation but may not be the best approach
            Send("velec 11 *selected 0");
        }
        
        public override string GetStimString(int[] pads, float[] amps, int[] widths)
        {
            // Define invariable parts of the command string
            const string invariablePart1 = "velec 11 *special_anodes 1 *name test *elec 1 *pads ";
            const string invariablePart2 = " *amp ";
            const string invariablePart3 = " *width ";
            const string finalPart = " *selected 1 *sync 0";

            // Initialize the variable parts of the command string
            var variablePart1 = "";
            var variablePart2 = "";
            var variablePart3 = "";

            for (var i = 0; i < amps.Length; i++)
            {
                if (_config.IsAnode(i) || pads[i] == 0) continue;
                
                var amplitudeValue = amps[i];
                var widthValue = widths[i];

                // Building each part of the command string, parsing floats with "." and not ","
                variablePart1 += $"{i + 1}=C,";
                variablePart2 += $"{i + 1}={amplitudeValue.ToString(CultureInfo.InvariantCulture)},";
                variablePart3 += $"{i + 1}={widthValue.ToString(CultureInfo.InvariantCulture)},";
            }

            // Trim the trailing commas from each part
            variablePart1 = variablePart1.TrimEnd(',');
            variablePart2 = variablePart2.TrimEnd(',');
            variablePart3 = variablePart3.TrimEnd(',');

            // Concatenate all parts to form the complete command string
            var completeString = invariablePart1 
                                 + variablePart1 
                                 + invariablePart2 
                                 + variablePart2 
                                 + invariablePart3 
                                 + variablePart3 
                                 + finalPart;

            return completeString;
        }

        protected override void OnMessageArrived(string message)
        {
            if (Battery is null) SetBoxInfo(message);
            
            if (!_receivedValidGreeting && _isSuccessfullyConnected)
            {
                _receivedValidGreeting = message is "Re:[] new connection" or "Re:[] re-connection" or "Re:[] ok";
                IsConnected = _receivedValidGreeting;
            }
        
            base.OnMessageArrived(message);
        }

        private void SetBoxInfo(string response)
        {
            // Check if response string contains the known battery response sequence and ignore it if it does not
            const string checkString = "Re:[] battery ";
            if (response.Length < checkString.Length || response[..checkString.Length] != checkString) 
                return;
        
            // Regular expression to match each key-value pair
            const string pattern = @"\*(capacity|voltage|current|temperature)=(?<value>-?\d+(\.\d+)?)";

            // Find matches
            var matches = Regex.Matches(response[checkString.Length..], pattern);
            
            // Parse and assign the values to the respective properties
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = float.Parse(match.Groups["value"].Value);

                switch (key)
                {
                    case "capacity":
                        Battery = value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "voltage":
                        Voltage = (value / 100).ToString(CultureInfo.InvariantCulture);
                        break;
                    case "current":
                        Current = (value / 100).ToString(CultureInfo.InvariantCulture);
                        break;
                    case "temperature":
                        Temperature = (value / 100).ToString(CultureInfo.InvariantCulture);
                        break;
                }
            }

            // Update the data asset
            var stimBoxData = StimBoxData.Instance;
            stimBoxData.UpdateData(Battery, Voltage, Current, Temperature);
        }
    
        protected override void OnConnectionEvent(bool wasSuccessful)
        {
            _isSuccessfullyConnected = wasSuccessful;
            if (!_isSuccessfullyConnected) _receivedValidGreeting = false;
        
            base.OnConnectionEvent(wasSuccessful);
        }
    }
}
