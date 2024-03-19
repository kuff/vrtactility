// Copyright (C) 2024 Peter Leth

#region
using Tactility.Box;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public class ProjectInfoDisplay : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The text object to display the project info.")]
        private Text infoText;
        private AbstractBoxController _boxController;

        private void Start()
        {
            _boxController = FindObjectOfType<AbstractBoxController>();
        }

        private void Update()
        {
            if (infoText != null && _boxController != null)
            {
                // Format the string to display project info and box connection status
                infoText.text = $"Version: {Application.version} | Box Connected: {(_boxController.IsConnected ? "Yes" : "No")} | Port: {_boxController.Port} | Battery: {_boxController.Battery} | Voltage: {_boxController.Voltage} | Current: {_boxController.Current} | Temperature: {_boxController.Temperature}";
            }
        }
    }
}
