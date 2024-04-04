// Copyright (C) 2024 Peter Leth

#region
using Tactility.Box;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    [RequireComponent(typeof(InterfaceManager))]
    public class ProjectInfoDisplay : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The text object to display the project info.")]
        private Text infoText;
        private InterfaceManager _interface;
        private AbstractBoxController _controller;

        private void Start()
        {
            _interface = GetComponent<InterfaceManager>();
            _controller = FindObjectOfType<AbstractBoxController>();
        }

        private void Update()
        {
            if (infoText != null && _controller != null)
            {
                // Format the string to display project info and box connection status
                infoText.text = $"Version: {Application.version} | Interface: {_interface.GetActiveSceneName()} | Box Connected: {(_controller.IsConnected ? "Yes" : "No")} | Port: {_controller.Port ?? "N/A"} | Battery: {_controller.Battery ?? "N/A"} | Voltage: {_controller.Voltage ?? "N/A"} | Current: {_controller.Current ?? "N/A"} | Temperature: {_controller.Temperature ?? "N/A"}";
            }
        }
    }
}
