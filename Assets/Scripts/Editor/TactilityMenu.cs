using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Tactility.Box;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    public abstract class TactilityMenu
    {
        [MenuItem("Tactility/Show Battery Info", false, 1)]
        private static void ShowBatteryInfo()
        {
            EditorWindow.GetWindow(typeof(BatteryInfoWindow), false, "Tactility Info");
        }
    
        [MenuItem("Tactility/Find Box Port", false, 2)]
        private static void FindPort()
        {
            Debug.Log("Searching for Tactility box...");
            var portNames = SerialPort.GetPortNames();
            string foundPort = null;
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
                    serialPort.ReadTimeout = 500;  // Set timeouts to prevent hanging
                    serialPort.WriteTimeout = 500;

                    serialPort.Open();
                    // Write the command to the port
                    serialPort.WriteLine("iam TACTILITY");
                    // Give the device a bit of time to respond
                    Thread.Sleep(100);
                        
                    // Read the response
                    var response = serialPort.ReadLine();
                    // Check if the response is what you expect from the Tactility box
                    if (!response.Contains("Re:[] new connection") &&
                        !response.Contains("Re:[] re-connection") &&
                        !response.Contains("Re:[] ok"))
                    {
                        continue;
                    }
                    
                    foundPort = portName;
                    EditorGUIUtility.systemCopyBuffer = foundPort;
                    Debug.Log($"Tactility box found on port {foundPort}. Copied to clipboard.");
                    break;
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

            if (foundPort == null)
            {
                Debug.LogError("Tactility box not found.");
            }
        }

        [MenuItem("Tactility/Documentation", false, 50)]
        private static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/kuff/vrtactility/wiki");
        }
        
        [MenuItem("Tactility/Open Persistent Data Path", false, 3)]
        private static void OpenPersistentDataPath()
        {
            var path = Application.persistentDataPath;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Application.platform)
            {
                // Open the folder in the file explorer based on the operating system
                case RuntimePlatform.WindowsEditor:
                    Process.Start("explorer.exe", path.Replace('/', '\\'));
                    break;
                case RuntimePlatform.OSXEditor:
                    Process.Start("open", path);
                    break;
                case RuntimePlatform.LinuxEditor:
                    Process.Start("xdg-open", path);
                    break;
                default:
                    Debug.LogError("Unsupported platform.");
                    break;
            }
        }

        // Custom Editor Window to display the info
        public class BatteryInfoWindow : EditorWindow
        {
            private void OnGUI()
            {
                var stimBoxData = StimBoxData.Instance;
            
                if (stimBoxData != null)
                {
                    EditorGUILayout.LabelField("Capacity (%):", stimBoxData.capacity);
                    EditorGUILayout.LabelField("Voltage (V):", stimBoxData.voltage);
                    EditorGUILayout.LabelField("Current (mA):", stimBoxData.current);
                    EditorGUILayout.LabelField("Temperature (\u00b0C):", stimBoxData.temperature);
                    EditorGUILayout.LabelField("Last Updated:", stimBoxData.GetTimeSinceLastUpdate());
                }
                else
                {
                    EditorGUILayout.LabelField("StimBoxData not found!");
                }
            }
        }
    }
}