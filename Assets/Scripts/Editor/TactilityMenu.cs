// Copyright (C) 2024 Peter Leth

#region
using System.Diagnostics;
using Tactility.Box;
using UnityEditor;
using UnityEngine;
using static Tactility.Calibration.Interface.BoxConnectionManager;
using Debug = UnityEngine.Debug;
#endregion

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
            var foundPort = ScanForBox();
            EditorGUIUtility.systemCopyBuffer = foundPort;
            Debug.Log($"Tactility box found on port {foundPort}. Copied to clipboard.");
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
