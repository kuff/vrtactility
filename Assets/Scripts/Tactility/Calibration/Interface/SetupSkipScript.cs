using Tactility.Box;
using UnityEngine;
namespace Tactility.Calibration.Interface
{
    [RequireComponent(typeof(InterfaceManager))]
    public class SetupSkipScript : MonoBehaviour
    {
        [SerializeField]
        private GameObject skipFromScene;
        [SerializeField]
        private GameObject skipToScene;
        
        private InterfaceManager _interfaceManager;
        
        private void OnEnable()
        {
            _interfaceManager = GetComponent<InterfaceManager>();
            _interfaceManager.OnSceneChanged += SkipSetupIfAlreadyConnected;
        }
        
        private void SkipSetupIfAlreadyConnected(GameObject oldScene, GameObject newScene)
        {
            if (newScene != skipFromScene)
            {
                return;
            }
            
            var isConnected = FindObjectOfType<AbstractBoxController>()?.IsConnected ?? false;
            if (isConnected)
            {
                _interfaceManager.SetActiveScene(skipToScene.name);
            }
        }
        
        private void OnDisable()
        {
            _interfaceManager.OnSceneChanged -= SkipSetupIfAlreadyConnected;
        }
    }
}
