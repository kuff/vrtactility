using UnityEngine;

namespace Internal
{
    public class StimBoxMessenger : AbstractMessenger
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
            }, 200f);
        }

        public override void EnableStimulation()
        {
            Send("stim on");
        }

        public override void DisableStimulation()
        {
            Send("stim off");
        }

        protected override void OnMessageArrived(string msg)
        {
            SetBattery(msg);
        
            if (!_receivedValidGreeting && _isSuccessfullyConnected)
            {
                _receivedValidGreeting = msg is "Re:[] new connection" or "Re:[] re-connection" or "Re:[] ok";
                IsConnected = _receivedValidGreeting;
            }
            else IsConnected = false;
        
            base.OnMessageArrived(msg);
        }

        private void SetBattery(string response)
        {
            // Check if response string contains the known battery response sequence and ignore it if it does not
            const string checkString = "Re:[] battery *capacity=";
            if (response.Length < checkString.Length || response[..checkString.Length] != checkString) 
                return;
        
            Battery = response;
        }
    
        protected override void OnConnectionEvent(bool success)
        {
            _isSuccessfullyConnected = success;
            if (!_isSuccessfullyConnected) _receivedValidGreeting = false;
        
            base.OnConnectionEvent(success);
        }
    }
}
