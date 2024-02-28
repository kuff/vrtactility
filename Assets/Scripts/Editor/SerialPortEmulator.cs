using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
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
            switch (message)
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
                return;
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
                // Resetting PadValues assuming all pads could be anodes initially
                for (var i = 0; i < 32; i++)
                {
                    PadValues[i] = new PadInfo(true);  // Assume all pads are anodes initially
                }

                // Splitting the message on spaces to isolate each segment
                var segments = message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Check for special anodes
                var specialAnodes = segments.Contains("*special_anodes 1");

                // Temporary storage to hold parsed values before updating PadValues
                var tempPadValues = new Dictionary<int, PadInfo>();

                for (var i = 1; i < segments.Length; i += 2)
                {
                    var segment = segments[i];
                    var prevSegment = segments[i - 1];
                    if (prevSegment.StartsWith("*pads"))
                    {
                        var padPairs = segment.Split(',');
                        foreach (var pair in padPairs)
                        {
                            var parts = pair.Split('=');
                            if (parts.Length != 2) continue;

                            var padIndex = int.Parse(parts[0]) - 1;
                            // Assuming pads are cathodes if specified, and anodes otherwise
                            tempPadValues[padIndex] = new PadInfo(false);  // Setting specified pads as cathodes
                        }
                    }
                    else if (prevSegment.StartsWith("*amp") || prevSegment.StartsWith("*width"))
                    {
                        var isAmplitude = prevSegment.StartsWith("*amp");
                        var valuePairs = segment.Split(',');
                        foreach (var pair in valuePairs)
                        {
                            var parts = pair.Split('=');
                            if (parts.Length != 2) continue;

                            var padIndex = int.Parse(parts[0]) - 1;
                            // Parse float with "." and not ","
                            var value = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                            if (!tempPadValues.ContainsKey(padIndex))
                                tempPadValues[padIndex] = new PadInfo(tempPadValues.ContainsKey(padIndex) && tempPadValues[padIndex].IsAnode, 0, 0);

                            if (isAmplitude)
                                tempPadValues[padIndex] = new PadInfo(tempPadValues[padIndex].IsAnode, value, tempPadValues[padIndex].Width);
                            else
                                tempPadValues[padIndex] = new PadInfo(tempPadValues[padIndex].IsAnode, tempPadValues[padIndex].Amplitude, value);
                        }
                    }
                }

                // Update PadValues based on tempPadValues
                foreach (var (key, value) in tempPadValues)
                {
                    PadValues[key] = value;
                }

                // If special_anodes is set to 1, update all non-specified pads to be anodes
                if (specialAnodes)
                {
                    for (var i = 0; i < PadValues.Count; i++)
                    {
                        if (!tempPadValues.ContainsKey(i)) // For non-specified pads, set them as anodes
                        {
                            PadValues[i] = new PadInfo(true);
                        }
                    }
                }

                // Log updated PadValues for verification
                if (EmulatorSettings.Instance.enableLogging)
                {
                    Debug.Log("Updated PadValues:");
                    for (var i = 0; i < PadValues.Count; i++)
                    {
                        var padInfo = PadValues[i];
                        Debug.Log($"Pad {i}: Anode = {padInfo.IsAnode}, Amplitude = {padInfo.Amplitude}, Width = {padInfo.Width}");
                    }
                }

                SendResponse("Re:[] ok");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing pad values: " + ex.Message);
                SendResponse($"(Emulator) error during pad value parsing: message: \"{message}\", error: {ex.Message}", showWarning: true);
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
