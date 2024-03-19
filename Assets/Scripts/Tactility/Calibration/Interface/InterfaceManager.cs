// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Tactility.Calibration.Interface
{
    public class InterfaceManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("List of UI scene GameObjects. Each GameObject represents a different UI scene.")]
        private List<GameObject> uiScenes;

        private GameObject _activeScene;

        private void Start()
        {
            SetActiveScene(uiScenes[0].name);
        }

        public void SetActiveScene(string sceneName)
        {
            // Catch if the scene name is invalid
            if (uiScenes.Find(scene => scene.name == sceneName) == null)
            {
                // Raise an exception
                throw new KeyNotFoundException($"The scene name {sceneName} was not found in the list of UI scenes.");
            }

            foreach (var scene in uiScenes)
            {
                if (scene.name == sceneName)
                {
                    scene.SetActive(true);
                    _activeScene = scene;
                }
                else
                {
                    scene.SetActive(false);
                }
            }
        }

        public string GetActiveSceneName()
        {
            return _activeScene.name;
        }
    }
}
