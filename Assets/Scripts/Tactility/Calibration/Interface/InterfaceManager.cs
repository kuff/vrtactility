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

        // Define a delegate for the scene changed event
        public delegate void SceneChangedEventHandler(GameObject oldScene, GameObject newScene);
        
        // Define the event based on the delegate
        public event SceneChangedEventHandler OnSceneChanged;
        
        private void Start()
        {
            // Check if there is a scene to load after the next scene is loaded, otherwise load the first scene
            var sceneAfterLoad = PlayerPrefs.GetString("UISceneAfterLoad");
            if (!string.IsNullOrEmpty(sceneAfterLoad))
            {
                SetActiveScene(sceneAfterLoad);
                PlayerPrefs.DeleteKey("UISceneAfterLoad");
            }
            else
            {
                SetActiveScene(uiScenes[0].name);
            }
        }

        public void SetActiveScene(string sceneName)
        {
            // Catch if the scene name is invalid
            if (uiScenes.Find(scene => scene.name == sceneName) == null)
            {
                // Raise an exception
                throw new KeyNotFoundException($"The scene name {sceneName} was not found in the list of UI scenes.");
            }
            
            // Ignore if the scene is already active
            if (_activeScene != null && _activeScene.name == sceneName)
            {
                return;
            }

            var oldScene = _activeScene;

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

            // Raise the event if there are any subscribers
            OnSceneChanged?.Invoke(oldScene, _activeScene);
        }

        public string GetActiveSceneName()
        {
            return _activeScene.name;
        }

        public static void SetSceneAfterLoad(string name)
        {
            // Save the scene name to be loaded after the next scene is loaded
            PlayerPrefs.SetString("UISceneAfterLoad", name);
        }
    }
}
