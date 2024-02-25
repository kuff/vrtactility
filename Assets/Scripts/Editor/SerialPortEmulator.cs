using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class SerialPortEmulator
    {
        private static SerialPort _serialPort;
        private static Thread _readThread;
        private static bool _keepReading;
        private static bool _receivedIamTactility;
        private static bool _isInPlayMode;
        
        private static double _lastHeartbeatTime;
        private static int _heartbeatIterator;
        private const double HeartbeatInterval = 3.0;  // seconds
        
        public static bool IsConnected => _serialPort is { IsOpen: true };
        public static readonly List<PadInfo> PadValues = new();
        public static bool StimulationEnabled { get; private set; }
        
        public struct PadInfo
        {
            public readonly bool IsAnode;
            public readonly float Amplitude;
            public readonly float Width;

            public PadInfo(bool isAnode, float amplitude = 0, float width = 0)
            {
                IsAnode = isAnode;
                Amplitude = amplitude;
                Width = width;
            }
        }

        public static void InitializePort(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
        }

        public static void OpenPort()
        {
            if (_serialPort == null)
            {
                Debug.LogError("Serial port not initialized");
                return;
            }

            try
            {
                _serialPort.Open();
                _keepReading = true;
                StartReading();
                EditorApplication.update += EditorUpdate;
                Debug.Log($"Emulator ready and listening on {_serialPort.PortName}.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error opening serial port: " + ex.Message);
            }
        }

        private static void StartReading()
        {
            if (!_serialPort.IsOpen)
            {
                Debug.LogError("Serial port not open");
                return;
            }

            _readThread = new Thread(ReadPort);
            _readThread.Start();
            
            for (var i = 0; i < 32; i++)
            {
                PadValues.Add(new PadInfo(true));  // Assume all pads are anodes initially
            }
        }

        private static void ReadPort()
        {
            while (_keepReading)
            {
                if (!_serialPort.IsOpen || _serialPort.BytesToRead <= 0) continue;
                
                try
                {
                    var message = _serialPort.ReadLine();
                    HandleIncomingMessage(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error reading from serial port: " + ex.Message);
                    // Log when the connection is lost
                    Debug.Log("Connection to the serial port lost.");
                    OnExit();
                }
            }
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static void HandleIncomingMessage(string message)
        {
            if (EmulatorSettings.Instance.enableLogging)
                Debug.Log($"[SerialPortEmulator] Received: {message}");

            // Update stimulation state based on received messages
            switch (message.Trim())
            {
                case "stim on\r":
                    StimulationEnabled = true;
                    SendResponse("Re:[] ok");
                    break;
                case "stim off\r":
                    StimulationEnabled = false;
                    SendResponse("Re:[] ok");
                    break;
            }
            
            // Check if the received message is "iam TACTILITY" and set the flag
            if (message == "iam TACTILITY\r") 
                _receivedIamTactility = true;
            
            // Parse pad values
            if (message.StartsWith("velec"))
            {
                ParsePadValues(message);
            }
            
            var response = message switch
            {
                "iam TACTILITY\r" => "Re:[] re-connection",
                "elec 1 *pads_qty 32\r" => "Re:[] ok",
                "battery ?\r" => "Re:[] battery *capacity=21% *voltage=3.63V *current=-91.59mA",  // TODO: missing temperature
                "freq 50\r" => "Re:[] ok",
                _ => "(Emulator) unrecognized command"
            };
        
            if (response != "(Emulator) unrecognized command")
                SendResponse(response);
            else
                SendResponse(response + $": \"{message}\"", showWarning:true);
        }
        
        private static void ParsePadValues(string message)
        {
            try
            {
                var padsMatch = Regex.Matches(message, @"\*amp (\d+)=([0-9.]+)|\*width (\d+)=([0-9.]+)");

                foreach (Match match in padsMatch)
                {
                    if (match.Groups[1].Success)  // Amplitude value
                    {
                        var padIndex = int.Parse(match.Groups[1].Value) - 1;  // Adjust index to be 0-based
                        var amplitudeValue = float.Parse(match.Groups[2].Value);
                        var currentPadInfo = PadValues[padIndex];
                        PadValues[padIndex] = new PadInfo(false, amplitudeValue, currentPadInfo.Width);
                    }
                    else if (match.Groups[3].Success)  // Width value
                    {
                        var padIndex = int.Parse(match.Groups[3].Value) - 1;  // Adjust index to be 0-based
                        var widthValue = float.Parse(match.Groups[4].Value);
                        var currentPadInfo = PadValues[padIndex];
                        PadValues[padIndex] = new PadInfo(false, currentPadInfo.Amplitude, widthValue);
                    }
                }
                SendResponse("Re:[] ok");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing pad values: " + ex.Message);
                SendResponse($"(Emulator) error during pad value parsing: message: \"{message}\", error: {ex.Message}", 
                    showWarning:true);
            }
        }

        private static void SendResponse(string response, bool showWarning = false)
        {
            if (!_serialPort.IsOpen) return;
    
            try
            {
                _serialPort.WriteLine(response);
                if (!EmulatorSettings.Instance.enableLogging) return;
                
                if (showWarning)
                    Debug.LogWarning($"[SerialPortEmulator] Sent: {response}");
                else
                    Debug.Log($"[SerialPortEmulator] Sent: {response}");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error writing to serial port: " + ex.Message);
            }
        }

        private static void EditorUpdate()
        {
            // Only send heartbeats if in play mode and "iam TACTILITY" has been received
            if (_isInPlayMode && _receivedIamTactility)
            {
                var currentTime = EditorApplication.timeSinceStartup;
                if (currentTime > _lastHeartbeatTime + HeartbeatInterval)
                {
                    var periodicMessage = $"[{_heartbeatIterator}] tic *stim 20";
                    SendResponse(periodicMessage);
                    _lastHeartbeatTime = currentTime;
                    _heartbeatIterator++;
                }
            }
        }

        // Call this method when the application or editor is closed
        public static void OnExit()
        {
            if (_serialPort is not { IsOpen: true }) return;
            
            _keepReading = false;
            _serialPort.Close();
            _readThread.Join();
            EditorApplication.update -= EditorUpdate;
            
            Debug.Log("Emulator exited successfully.");
        }
        
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            if (!SessionState.GetBool("IsEmulatorEnabled", false) || IsConnected) 
                return;
            
            // Initialize and open the port if the emulator is enabled
            var settings = EmulatorSettings.Instance;
            InitializePort(settings.comPort, settings.baudRate);
            OpenPort();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    // Stop and clean up resources if the emulator is enabled
                    _isInPlayMode = false;
                    _receivedIamTactility = false; // Reset the flag when exiting play mode
                    if (SessionState.GetBool("IsEmulatorEnabled", false))
                    {
                        OnExit();
                    }
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    // Reinitialize if needed
                    _isInPlayMode = false;
                    if (SessionState.GetBool("IsEmulatorEnabled", false) && !IsConnected)
                    {
                        //OpenPort();
                    }
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    _isInPlayMode = true;
                    if (SessionState.GetBool("IsEmulatorEnabled", false) && !IsConnected)
                    {
                        // Initialize and open the port if the emulator is enabled
                        //OpenPort();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
