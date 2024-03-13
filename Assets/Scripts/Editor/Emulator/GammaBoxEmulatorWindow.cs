using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Emulator
{
    public class GammaBoxEmulatorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private EmulatorSettings _settings;
        private double _lastRepaintTime;
        private const double RepaintInterval = 0.1; // Repaint every 0.1 seconds

        // Tracking time durations between messages
        private double _shortestDuration = double.MaxValue;
        private double _longestDuration = double.MinValue;
        private double? _lastMessageTime;

        [MenuItem("Tactility/Gamma Box Emulator")]
        public static void ShowWindow()
        {
            GetWindow<GammaBoxEmulatorWindow>("Gamma Box Emulator");
        }

        private void OnEnable()
        {
            _settings = EmulatorSettings.Instance;
            EditorApplication.update += RequestRepaint; // Subscribe
        }

        private void OnDisable()
        {
            EditorApplication.update -= RequestRepaint; // Unsubscribe
        }

        private void RequestRepaint()
        {
            if (!(EditorApplication.timeSinceStartup - _lastRepaintTime > RepaintInterval))
            {
                return;
            }

            _lastRepaintTime = EditorApplication.timeSinceStartup;
            Repaint(); // Force the window to repaint
        }

        private void UpdateDurations(double newMessageTime)
        {
            if (newMessageTime <= 0)
            {
                return; // Ignore invalid times (e.g. 0)
            }
            if (_lastMessageTime.HasValue)
            {
                var duration = newMessageTime - _lastMessageTime.Value;
                if (duration < _shortestDuration)
                {
                    _shortestDuration = duration;
                }
                if (duration > _longestDuration)
                {
                    _longestDuration = duration;
                }
            }
            _lastMessageTime = newMessageTime;
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

            if (GUILayout.Button(GammaBoxEmulator.IsConnected ? "Disable Emulator" : "Enable Emulator"))
            {
                if (GammaBoxEmulator.IsConnected)
                {
                    DisableEmulator();
                }
                else
                {
                    EnableEmulator();
                }
            }
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Space(10);
            GUILayout.Label("Stimulation State", EditorStyles.boldLabel);
            
            GUILayout.Label(GammaBoxEmulator.IsConnected ? "CONNECTED" : "DISCONNECTED", EditorStyles.wordWrappedLabel);
            GUILayout.Label(GammaBoxEmulator.StimulationEnabled ? "STIM ON" : "STIM OFF", EditorStyles.wordWrappedLabel);
            GUILayout.Label($"FREQUENCY: {GammaBoxEmulator.GlobalFrequency} Hz", EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);
            GUILayout.Label("Pad Information", EditorStyles.boldLabel);
            try
            {
                for (var i = 0; i < GammaBoxEmulator.PadValues.Count; i++)
                {
                    var padInfo = GammaBoxEmulator.PadValues[i];
                    GUILayout.Label($"Pad {i + 1}: " + (padInfo.IsAnode
                        ? "Anode"
                        : $"Cathode - Amp: {padInfo.Amplitude}, Width: {padInfo.Width}"));
                }
            }
            catch (Exception e)
            {
                // We don't care about exceptions here
            }

            // Display durations
            // GUILayout.Space(10);
            // GUILayout.Label("Durations Between Messages", EditorStyles.boldLabel);
            // GUILayout.Label($"Shortest Duration: {_shortestDuration:F2} s");
            // GUILayout.Label($"Longest Duration: {_longestDuration:F2} s");

            // Recent Messages
            GUILayout.Space(10);
            GUILayout.Label("Recent Messages", EditorStyles.boldLabel);

            try
            {
                foreach (var message in GammaBoxEmulator.ExposedMessages)
                {
                    UpdateDurations(message.relativeTime);
                    GUILayout.Label($"({(message.relativeTime * 1000):F2} ms) {message.message}");
                }

                if (!GammaBoxEmulator.ExposedMessages.Any())
                {
                    GUILayout.Label("No messages received yet.");
                }
            }
            catch (Exception e)
            {
                // We don't care about exceptions here
            }

            GUILayout.EndScrollView();
        }

        private void EnableEmulator()
        {
            SessionState.SetBool("IsEmulatorEnabled", true);
            GammaBoxEmulator.InitializePort(_settings.comPort, _settings.baudRate);
            GammaBoxEmulator.OpenPort();
        }

        private static void DisableEmulator()
        {
            SessionState.SetBool("IsEmulatorEnabled", false);
            GammaBoxEmulator.OnExit();
        }
    }
}