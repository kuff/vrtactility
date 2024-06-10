// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
#endregion

namespace Tactility.Calibration.Interface
{
    public class SceneSelectionManager : DropdownManager<string>
    {
        private string _selectedSceneName;

        protected override List<string> GetAllItems()
        {
            var sceneNames = new List<string>();
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName != SceneManager.GetActiveScene().name)
                {
                    sceneNames.Add(sceneName);
                }
            }
            return sceneNames;
        }

        protected override void SetSelectedItem(string sceneName)
        {
            _selectedSceneName = sceneName;
        }

        protected override string GetItemName(string item)
        {
            return item;
        }

        public void LoadSelectedScene()
        {
            UpdateSelectedItem();
            if (!string.IsNullOrEmpty(_selectedSceneName))
            {
                SceneManager.LoadScene(_selectedSceneName);
            }
        }
    }
}
