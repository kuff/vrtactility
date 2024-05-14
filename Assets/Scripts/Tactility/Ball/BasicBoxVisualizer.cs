// Copyright (C) 2024 Peter Leth

#region
using UnityEngine;
#endregion

namespace Tactility.Ball
{
    [RequireComponent(typeof(GrabAndMoveScenario))]
    public class BasicBoxVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Material transparentMaterial;
        private GrabAndMoveScenario _scenario;
        private GameObject _targetProgressInstance;

        private void Start()
        {
            _scenario = GetComponent<GrabAndMoveScenario>();
        }

        private void Update()
        {
            if (_scenario.grabbable.isGrabbed && _targetProgressInstance == null)
            {
                _targetProgressInstance = CreateCube(_scenario.grabbable.gameObject.transform.localScale.x);
                _targetProgressInstance.transform.position = _scenario.targetPosition;
            }
            else if (!_scenario.grabbable.isGrabbed && _targetProgressInstance != null)
            {
                Destroy(_targetProgressInstance);
                //_targetProgressInstance == null;
            }
            
            // Match rotation of the _scenario.grabbable.gameObject if _targetProgressInstance exists
            if (_targetProgressInstance != null)
            {
                _targetProgressInstance.transform.rotation = _scenario.grabbable.gameObject.transform.rotation;
            }
            
            // Scale the cube to match the progress of the grabbable object, starting at 0.5f and ending at 1f
            if (_targetProgressInstance != null)
            {
                _targetProgressInstance.transform.localScale = Vector3.one * _scenario.grabbable.gameObject.transform.localScale.x * (0.5f + _scenario.Progress / 2f);
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
