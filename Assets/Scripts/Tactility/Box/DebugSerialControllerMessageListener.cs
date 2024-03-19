// Copyright (C) 2024 Peter Leth

#region
// using Tactility.Calibration.Old;
using UnityEngine;
#endregion

namespace Tactility.Box
{
    // NOTE: This class is also used in the Tactility.Calibration.Old namespace to make the calibration scene work
    //       meaning it is technically not just for debugging and will break things if removed, which is kind of a
    //       mess but will stay like this till the new calibration gui is implemented...
    public class DebugSerialControllerMessageListener : MonoBehaviour
    {
        // [SerializeField] private ConnectDevice cd;
        // [SerializeField] private UiManager uim;

        private bool _isSuccessfullyConnected;
        private bool _receivedValidGreeting;

        // Invoked when a line of data is received from the serial device.
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once ArrangeTypeMemberModifiers
        private void OnMessageArrived(string msg)
        {
            SetBattery(msg);

            if (!_receivedValidGreeting && _isSuccessfullyConnected)
            {
                _receivedValidGreeting = msg is "Re:[] new connection" or "Re:[] re-connection" or "Re:[] ok";
                SetConnectionStatus(_receivedValidGreeting);
            }

            Debug.Log("Inbound response: " + msg);
        }

        private void SetBattery(string response)
        {
            // Check if response string contains the known battery response sequence and ignore it if it does not
            const string checkString = "Re:[] battery *capacity=";
            if (response.Length < checkString.Length || response[..checkString.Length] != checkString)
            {
            }

            // uim.SetBatteryLevel(response);
        }

        // Invoked when a connect/disconnect event occurs. The parameter 'success'
        // will be 'true' upon connection, and 'false' upon disconnection or
        // failure to connect.
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once UnusedMember.Local
        private void OnConnectionEvent(bool success)
        {
            if (!success)
            {
                SetConnectionStatus(false);
            }
            else
            {
                _isSuccessfullyConnected = true;
            }
        }

        private void SetConnectionStatus(bool success)
        {
            // Manage ConnectDevice state
            // if (success)
            // {
            //     cd.ConnectDevicePanel.SetActive(false);
            // }
            // else
            // {
            //     cd.ConnectingText.alpha = 0;
            // }

            // Log connection status
            Debug.Log(success
                ? "Connection established"
                : "Connection attempt failed or disconnection detected");
        }
    }
}
