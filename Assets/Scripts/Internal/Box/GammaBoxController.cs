using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Internal.Box
{
    public class GammaBoxController : AbstractBoxController
    {
        [SerializeField]
        [Tooltip("If enabled, the messenger will automatically attempt to connect using the specified serial port upon the game start. Ensure the correct port name is set in the SerialController. This is useful for scenarios where an immediate connection is desirable without requiring an explicit user action to initiate the connection.")]
        private bool connectOnAwake;
        
        private bool _isSuccessfullyConnected;
        private bool _receivedValidGreeting;
    
        protected override void Start()
        {
            base.Start();
            Sc.SetTearDownFunction(DisableStimulation);
            
            if (connectOnAwake) Connect(Sc.portName);
        }

        public override void Connect(string port)
        {
            Sc.portName = port;
            Port = port;
        
            Sc.enabled = true;
            SendManyDelayed(new[]
            {
                "iam TACTILITY", 
                "elec 1 *pads_qty 32", 
                "battery ?", 
                "freq 50"
            });
        }

        public override void EnableStimulation()
        {
            Send("stim on");
        }

        public override void DisableStimulation()
        {
            Send("stim off");
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
