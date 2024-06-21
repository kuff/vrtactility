using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Tactility.Ball
{
    public class TrialManager : MonoBehaviour
    {
        [FormerlySerializedAs("currentForceLevel")]
        public int targetForceLevel;
        public int fileLineIndex;
        public List<Material> materials;
        
        private GrabAndMoveScenario _currentScenario;
        [FormerlySerializedAs("_fileLineIndex")]
        private string[] _fileLines;

        [SerializeField]
        private ParticleSystem _explodingParticles;
        private Renderer _renderer;

        private Material _defaultMaterial;
        

        private void Start()
        {
            _currentScenario = FindObjectOfType<GrabAndMoveScenario>();

            _currentScenario.WhenOnSuccess += HandleTaskComplete;
            _currentScenario.WhenOnFailure += HandleTaskComplete;
            // HandleTaskComplete();
            _renderer = _currentScenario.floatable.GetComponent<Renderer>();
            _defaultMaterial = _renderer.material;
        }

        private void Update()
        {
            if (_fileLines == null)
            {
                _fileLines = new string[] { };
                var textAsset = Resources.Load<TextAsset>("Tactility/TestOrder_fixedPos");
                _fileLines = textAsset.text.Split("\r\n");
                HandleTaskComplete(ScenarioTrigger.Idle);
            }
            else
            {
                if (_currentScenario.grabbable.isGrabbed && !(SceneManager.GetActiveScene().buildIndex==3 || SceneManager.GetActiveScene().buildIndex == 4))
                {
                    _renderer.material = materials[_currentScenario.currentForceLevel];
                }
                else
                {
                    _renderer.material = _defaultMaterial;
                }
            }
        }

        private (Vector3, Vector3) GetNextPositions()
        {
            var resultString = _fileLines[fileLineIndex];
            var targetForceLevels = new int[] { 1, 3, 6, 5, 2, 4, 2, 1, 5, 6, 4, 3, 3, 1, 5, 6, 4, 2, 5, 1, 4, 2, 3, 6, 2, 4, 3, 5, 6, 1 };

            var targetPositions = new List<Vector3>
            {
                new Vector3(-0.25f, 0.9f, 0.5f),
                new Vector3(-0.1f, 1.1f, 0.5f),
                new Vector3(0.1f, 0.9f, 0.5f),
                new Vector3(0.1f, 1.1f, 0.5f),
                new Vector3(-0.1f, 0.9f, 0.7f),
                new Vector3(-0.1f, 1.1f, 0.7f),
                new Vector3(0.1f, 0.9f, 0.7f),
                new Vector3(0.1f, 1.1f, 0.7f)
            };
            fileLineIndex++;
            Logger.LogSceneChange(fileLineIndex, targetForceLevel);

            if (SceneManager.GetActiveScene().buildIndex != 1)
            {
                // Increment targetForceLevel when modulus of 6 is 0
                //if (fileLineIndex % 5 == 1)
                //{
                targetForceLevel = targetForceLevels[fileLineIndex-1];

                // Reset targetForceLevel when it reaches 6
                if (fileLineIndex == 30)
                {
                    Invoke("LoadNextScene", 0.5f);
                }
                //}
            }
            else
            {
                if (fileLineIndex < 13)
                {
                    targetForceLevel = targetForceLevels[fileLineIndex-1];
                }
                else
                {
                    Invoke("LoadNextScene", 0.5f);                    
                }
            }

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
                    _explodingParticles.transform.position = _currentScenario.floatable.transform.position;
                    _currentScenario.floatable.GetComponent<Renderer>()!.enabled = false;
                    _explodingParticles.Play();
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
            _currentScenario.floatable.GetComponent<Renderer>()!.enabled = true;
        }
        
        private void SetNextPosition()
        {
            var nextPositions = GetNextPositions();
            _currentScenario.originPosition = nextPositions.Item1;
            _currentScenario.targetPosition = nextPositions.Item2;
            
            // Print item1 and 2
            // Debug.Log(nextPositions.Item1);
            // Debug.Log(nextPositions.Item2);
            
            _currentScenario.floatable.OriginPoint = _currentScenario.originPosition;
            _currentScenario.grabbable.allowGrabbing = true;
            
            // Set currentMaterial in accordance with targetForceLevel
            _currentScenario.floatable.GetComponent<Renderer>()!.enabled = true;

            // Logger.LogPositions(_currentScenario.originPosition, _currentScenario.targetPosition);
            // _currentScenario.floatable.GetComponent<Renderer>()!.material = materials[targetForceLevel - 1];
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
