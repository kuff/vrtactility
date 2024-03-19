// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public class SceneSelectionManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The dropdown menu for selecting the scene.")]
        private Dropdown sceneDropdown;

        [SerializeField]
        [CanBeNull]
        [Tooltip("The default scene to use if no other is selected. It is not required that this field be specified.")]
        private string defaultSceneName;

        // Hidden in inspector, but publicly accessible to other scripts if needed
        [HideInInspector]
        public string selectedSceneName;

        // Populate the dropdown with the available scene names
        private void Start()
        {
            sceneDropdown.ClearOptions();
            var sceneNames = new List<string>();

            // Get all scenes added to the build settings
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);

                // Ignore the current scene
                if (sceneName == SceneManager.GetActiveScene().name)
                {
                    continue;
                }

                sceneNames.Add(sceneName);
            }

            // If a default scene is set, add it first
            if (!string.IsNullOrWhiteSpace(defaultSceneName) && sceneNames.Contains(defaultSceneName))
            {
                sceneDropdown.options.Add(new Dropdown.OptionData(defaultSceneName));
                sceneNames.Remove(defaultSceneName); // Prevent adding it twice
            }

            // Add the rest of the scene names to the dropdown
            foreach (var sceneName in sceneNames)
            {
                sceneDropdown.options.Add(new Dropdown.OptionData(sceneName));
            }
        }

        // Set the selectedSceneName to the scene name corresponding to the selected dropdown option
        public void SetSelectedScene()
        {
            selectedSceneName = sceneDropdown.options[sceneDropdown.value].text;
        }

        // Method to load the selected scene, intended to be invoked by a button
        public void LoadSelectedScene()
        {
            if (!string.IsNullOrEmpty(selectedSceneName))
            {
                SceneManager.LoadScene(selectedSceneName);
            }
        }
    }
}
