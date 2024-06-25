using UnityEngine;
using UnityEngine.SceneManagement;
namespace Tactility.Task
{
    public class SceneLoader : MonoBehaviour
    {
        void OnEnable()
        {
            // Subscribe to the sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Define what happens when the scene is loaded
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("New scene loaded: " + scene.name);
            int currentIndexScene = SceneManager.GetActiveScene().buildIndex;

            Logger.SceneIndex(currentIndexScene);
        }
    }
}
