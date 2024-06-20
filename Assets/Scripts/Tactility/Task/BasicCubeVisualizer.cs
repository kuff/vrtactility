// Copyright (C) 2024 Peter Leth

#region
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

namespace Tactility.Task
{
    [RequireComponent(typeof(GrabAndMoveScenario))]
    public class BasicCubeVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Material transparentMaterial;
        private GrabAndMoveScenario _scenario;
        private GameObject _targetProgressInstance;
        private TrialManager _trialManager;
        private Renderer _renderer;
        
        private float wobbleFrequency = 35.0f; // Frequency of the wobble
        private float wobbleAmplitude = 5.0f; // Amplitude of the wobble

        private void Start()
        {
            _scenario = GetComponent<GrabAndMoveScenario>();
            _trialManager = FindObjectOfType<TrialManager>();
        }

        private void Update()
        {
            if (_scenario.grabbable.isGrabbed && _targetProgressInstance == null)
            {
                _targetProgressInstance = CreateCube(_scenario.grabbable.gameObject.transform.localScale.x);
                _targetProgressInstance.transform.position = _scenario.targetPosition;
                _renderer = _targetProgressInstance.GetComponent<Renderer>();
            }
            else if (!_scenario.grabbable.isGrabbed && _targetProgressInstance != null)
            {
                Destroy(_targetProgressInstance);
                //_targetProgressInstance == null;
            }
            
            if (_targetProgressInstance != null)
            {
                // Match rotation of the _scenario.grabbable.gameObject if _targetProgressInstance exists
                _targetProgressInstance.transform.rotation = _scenario.grabbable.gameObject.transform.rotation;
                
                // Scale the cube to match the progress of the grabbable object, starting at 0.5f and ending at 1f
                _targetProgressInstance.transform.localScale = Vector3.one * _scenario.grabbable.gameObject.transform.localScale.x * (0.5f + _scenario.Progress / 2f);
                
                // var targetForceLevel = _trialManager.targetForceLevel;
                var targetForceLevel = _trialManager.targetForceLevel;
                var currentMaterial = _trialManager.materials[targetForceLevel];
                _renderer.material = currentMaterial;
                
                if (_scenario.currentForceLevel != targetForceLevel && SceneManager.GetActiveScene().buildIndex != 4)
                {
                    // Apply subtle wobble effect
                    var wobble = Mathf.Sin(Time.time * wobbleFrequency) * wobbleAmplitude;
                    _targetProgressInstance.transform.rotation *= Quaternion.Euler(wobble, wobble, wobble);
                }
            }
        }

        private GameObject CreateCube(float size)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one * size;
            if (transparentMaterial != null)
            {
                cube.GetComponent<Renderer>().material = transparentMaterial;
            }
            
            // Make it so that nothing can collide with the cube
            cube.layer = 2;
            
            return cube;
        }
    }
}