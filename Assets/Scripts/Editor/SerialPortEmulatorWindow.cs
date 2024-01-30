using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SerialPortEmulatorWindow : EditorWindow
    {
        private EmulatorSettings _settings;

        [MenuItem("Tactility/Serial Port Emulator")]
        public static void ShowWindow()
        {
            GetWindow<SerialPortEmulatorWindow>("Serial Port Emulator");
        }

        private void OnEnable()
        {
            _settings = EmulatorSettings.Instance;
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Emulator Settings not found!", MessageType.Error);
                return;
            }

            GUILayout.Label("Configure Serial Port", EditorStyles.boldLabel);
            _settings.comPort = EditorGUILayout.TextField("COM Port", _settings.comPort);
            _settings.baudRate = EditorGUILayout.IntField("Baud Rate", _settings.baudRate);
            _settings.enableLogging = EditorGUILayout.Toggle("Enable Logging", _settings.enableLogging);

            if (!GUILayout.Button(SerialPortEmulator.IsConnected ? "Disable Emulator" : "Enable Emulator")) 
                return;
            
            if (SerialPortEmulator.IsConnected)
                DisableEmulator();
            else
                EnableEmulator();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(_settings);
#endif
        }

        private void EnableEmulator()
        {
            // Set the emulator to start when play mode begins
            SessionState.SetBool("IsEmulatorEnabled", true);
            SerialPortEmulator.InitializePort(_settings.comPort, _settings.baudRate);
            SerialPortEmulator.OpenPort();
        }

        private static void DisableEmulator()
        {
            // Set the emulator to stop when play mode ends
            SessionState.SetBool("IsEmulatorEnabled", false);
            SerialPortEmulator.OnExit();
        }
    }
}
