// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
using System;
using System.IO.Ports;
using System.Threading;
using Tactility.Box;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public class BoxConnectionManager : MonoBehaviour
    {
        [SerializeField]
        private InputField portField;
        private AbstractBoxController _boxController;
        private InterfaceManager _interface;

        private bool _waitingForConnection;

        private void Start()
        {
            _boxController = FindObjectOfType<AbstractBoxController>();
            _interface = FindObjectOfType<InterfaceManager>();
        }

        private void Update()
        {
            if (!_waitingForConnection || !_boxController.IsConnected)
            {
                return;
            }

            _waitingForConnection = false;
            _interface.SetActiveScene("Calibration");
            DontDestroyOnLoad(_boxController.gameObject); // The box doesn't prevent its own destruction
        }

        public void TryConnect()
        {
            // If the port field is empty, scan for the box
            var port = string.IsNullOrWhiteSpace(portField.text) ? ScanForBox() : portField.text;

            _boxController.Connect(port);
            _waitingForConnection = true;
        }

        [CanBeNull]
        public static string ScanForBox()
        {
            Debug.Log("Searching for Gamma box...");
            var portNames = SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                using var serialPort = new SerialPort(portName);
                try
                {
                    // Configure serial port settings here if necessary
                    serialPort.BaudRate = 115200;
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.ReadTimeout = 500; // Set timeouts to prevent hanging
                    serialPort.WriteTimeout = 500;

                    serialPort.Open();
                    // Write the command to the port
                    serialPort.WriteLine("iam TACTILITY\r");
                    // Give the device a bit of time to respond
                    Thread.Sleep(100); // TODO: refactor as coroutine

                    // Read the response
                    var response = serialPort.ReadLine();
                    // Check if the response is what you expect from the Tactility box
                    if (!response.Contains("Re:[] new connection") &&
                        !response.Contains("Re:[] re-connection") &&
                        !response.Contains("Re:[] ok"))
                    {
                        continue;
                    }

                    return portName;
                }
                catch (TimeoutException)
                {
                    // Handle timeout exceptions (no response in a timely manner)
                    Debug.Log("No response from " + portName);
                }
                catch (Exception e)
                {
                    // Handle other exceptions such as port being in use, etc.
                    Debug.Log($"Failed to test port {portName}: {e.Message}");
                }
            }

            Debug.LogError("Tactility box not found.");
            return null;
        }
    }
}
