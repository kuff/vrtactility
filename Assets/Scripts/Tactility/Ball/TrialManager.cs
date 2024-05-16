using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tactility.Ball
{
    public class TrialManager : MonoBehaviour
    {
        [FormerlySerializedAs("currentForceLevel")]
        public int targetForceLevel;
        public int fileLineIndex;
        [SerializeField]
        private List<Material> materials;
        
        private GrabAndMoveScenario _currentScenario;
        [FormerlySerializedAs("_fileLineIndex")]
        private string[] _fileLines;
        
        private void Start()
        {
            _currentScenario = FindObjectOfType<GrabAndMoveScenario>();

            _currentScenario.WhenOnSuccess += HandleTaskComplete;
            _currentScenario.WhenOnFailure += HandleTaskComplete;
            // HandleTaskComplete();
        }

        private void Update()
        {
            if (_fileLines == null)
            {
                _fileLines = new string[] { };
                var textAsset = Resources.Load<TextAsset>("Tactility/TestOrder30");
                _fileLines = textAsset.text.Split("\r\n");
                HandleTaskComplete(ScenarioTrigger.Idle);
            }
        }

        private (Vector3, Vector3) GetNextPositions()
        {
            var resultString = _fileLines[fileLineIndex];
            var targetPositions = new List<Vector3>
            {
                new Vector3(-0.1f, 0.9f, 0.5f),
                new Vector3(-0.1f, 1.1f, 0.5f),
                new Vector3(0.1f, 0.9f, 0.5f),
                new Vector3(0.1f, 1.1f, 0.5f),
                new Vector3(-0.1f, 0.9f, 0.7f),
                new Vector3(-0.1f, 1.1f, 0.7f),
                new Vector3(0.1f, 0.9f, 0.7f),
                new Vector3(0.1f, 1.1f, 0.7f)
            };
            fileLineIndex++;
            
            // Increment targetForceLevel when modulus of 6 is 0
            if (fileLineIndex % 6 == 1)
            {
                targetForceLevel++;
                
                // Reset targetForceLevel when it reaches 6
                if (targetForceLevel == 6)
                {
                    targetForceLevel = 1;
                }
            }
            
            Debug.Log("Origin position: " + resultString[0]);
            Debug.Log("Target position: " + resultString[2]);

            var originIndex = int.Parse(resultString[0].ToString());
            var targetIndex = int.Parse(resultString[2].ToString());
            return (targetPositions[originIndex - 1],targetPositions[targetIndex - 1]);
        }

        private void HandleTaskComplete(ScenarioTrigger cause)
        {
            // Play animation corresponding to each failure cause
            switch (cause)
            {
                case ScenarioTrigger.LossOfGrab:
                case ScenarioTrigger.TooLittlePressure:
                    _currentScenario.grabbable.allowGrabbing = false;

                    // Simulate gravity for but a moment
                    var cubeRigidbody = _currentScenario.floatable.GetComponent<Rigidbody>();
                    cubeRigidbody.useGravity = true;

                    // var tempOriginPosition = _currentScenario.floatable.transform.position;
                    // tempOriginPosition.y = 0.25f;
                    // _currentScenario.floatable.OriginPoint = tempOriginPosition;
                    
                    break;
                case ScenarioTrigger.TooMuchPressure:
                    _currentScenario.grabbable.allowGrabbing = false;
                    break;
                case ScenarioTrigger.Idle:
                    SetNextPosition();
                    return;
                case ScenarioTrigger.Success:
                    _currentScenario.grabbable.allowGrabbing = false;
                    _currentScenario.floatable.GetComponent<Renderer>()!.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cause), cause, null);
            }
            
            // Launch coroutine to get next position
            StartCoroutine(AfterFailure());
        }
        
        private IEnumerator AfterFailure()
        {
            yield return new WaitForSeconds(1f);

            SetNextPosition();
        }
        
        private void SetNextPosition()
        {
            var nextPositions = GetNextPositions();
            _currentScenario.originPosition = nextPositions.Item1;
            _currentScenario.targetPosition = nextPositions.Item2;
            
            // Print item1 and 2
            Debug.Log(nextPositions.Item1);
            Debug.Log(nextPositions.Item2);
            
            _currentScenario.floatable.OriginPoint = _currentScenario.originPosition;
            _currentScenario.grabbable.allowGrabbing = true;
            
            // Set currentMaterial in accordance with targetForceLevel
            _currentScenario.floatable.GetComponent<Renderer>()!.enabled = true;
            _currentScenario.floatable.GetComponent<Renderer>()!.material = materials[targetForceLevel - 1];
        }
    }
}
