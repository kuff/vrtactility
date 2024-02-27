using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SerialPortEmulatorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private EmulatorSettings _settings;

        private double _lastRepaintTime = 0;
        private const double RepaintInterval = 0.1;  // Repaint every 0.1 seconds

        [MenuItem("Tactility/Serial Port Emulator")]
        public static void ShowWindow()
        {
            GetWindow<SerialPortEmulatorWindow>("Serial Port Emulator");
        }

        private void OnEnable()
        {
            _settings = EmulatorSettings.Instance;
            EditorApplication.update += RequestRepaint;  // Subscribe
        }

        private void OnDisable()
        {
            EditorApplication.update -= RequestRepaint;  // Unsubscribe
        }

        private void RequestRepaint()
        {
            if (!(EditorApplication.timeSinceStartup - _lastRepaintTime > RepaintInterval)) return;
            
            _lastRepaintTime = EditorApplication.timeSinceStartup;
            Repaint(); // Force the window to repaint
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Emulator Settings not found!", MessageType.Error);
                return;
            }

            GUILayout.Label("Configure Serial Port", EditorStyles.boldLabel);
    
            // Use GUIContent for labels with tooltips
            _settings.comPort = EditorGUILayout.TextField(new GUIContent("COM Port", "Enter the COM port the emulator will use"), _settings.comPort);
            _settings.baudRate = EditorGUILayout.IntField(new GUIContent("Baud Rate", "Set the baud rate for the serial connection"), _settings.baudRate);
            _settings.enableLogging = EditorGUILayout.Toggle(new GUIContent("Enable Logging", "Toggle logging of all serial communications"), _settings.enableLogging);

            if (GUILayout.Button(SerialPortEmulator.IsConnected ? "Disable Emulator" : "Enable Emulator"))
            {
                if (SerialPortEmulator.IsConnected)
                    DisableEmulator();
                else
                    EnableEmulator();
            }

            // Display stimulation state
            GUILayout.Space(10);
            var stimStateText = SerialPortEmulator.StimulationEnabled ? "Stimulation is ON" : "Stimulation is OFF";
            EditorGUILayout.LabelField("Stimulation State", stimStateText, EditorStyles.boldLabel);

            // Display Pad Information
            GUILayout.Space(10);
            GUILayout.Label("Pad Information", EditorStyles.boldLabel);
            for (var i = 0; i < SerialPortEmulator.PadValues.Count; i++)
            {
                var padInfo = SerialPortEmulator.PadValues[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Pad {i + 1}: ", GUILayout.Width(50));
                // Parse floats with "." and not ","
                // var amp = padInfo.Amplitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                // var width = padInfo.Width.ToString(System.Globalization.CultureInfo.InvariantCulture);
                // GUILayout.Label(padInfo.IsAnode ? "Anode" : $"Amp: {amp} Width: {width}");
                GUILayout.Label(padInfo.IsAnode ? "Anode" : $"Amp: {padInfo.Amplitude} Width: {padInfo.Width}");
                GUILayout.EndHorizontal();
            }

#if UNITY_EDITOR  // NOTE: Is this pragma even necessary?
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_settings);
            }
#endif
        }

        private void EnableEmulator()
        {
            SessionState.SetBool("IsEmulatorEnabled", true);
            SerialPortEmulator.InitializePort(_settings.comPort, _settings.baudRate);
            SerialPortEmulator.OpenPort();
        }

        private static void DisableEmulator()
        {
            SessionState.SetBool("IsEmulatorEnabled", false);
            SerialPortEmulator.OnExit();
        }
    }
}
