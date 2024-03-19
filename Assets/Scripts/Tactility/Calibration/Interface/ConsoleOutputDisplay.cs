// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public class ConsoleOutputDisplay : MonoBehaviour
    {
        [Tooltip("The maximum number of messages to display in the console. If the number of messages exceeds this value, the oldest messages will be removed.")]
        public int maxMessages = 50;

        [SerializeField]
        [Tooltip("The text object to display the console output.")]
        private Text consoleOutput;

        private readonly Queue<string> _logMessages = new Queue<string>();

        private void Awake()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            var newLogEntry = $"[{type}] {logString}\n";
            if (type == LogType.Exception)
            {
                newLogEntry += $"StackTrace: {stackTrace}\n";
            }

            // Add the new log entry to the queue
            _logMessages.Enqueue(newLogEntry);

            // If we have more log messages than the maximum, dequeue the oldest
            while (_logMessages.Count > maxMessages)
            {
                _logMessages.Dequeue();
            }

            // Rebuild the display text
            consoleOutput.text = string.Join("", _logMessages.ToArray());
        }

        public void ClearConsole()
        {
            _logMessages.Clear();
            consoleOutput.text = string.Empty;
        }
    }
}
