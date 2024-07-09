using System;
using System.Collections;
using System.Collections.Generic;
using Tactility.Box;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tactility.Task
{
    public class TrialManager : MonoBehaviour
    {
        [FormerlySerializedAs("currentForceLevel")]
        public int targetForceLevel;
        public int fileLineIndex;
        public List<Material> materials;
        public List<Material> targetForceHighlight_materials;
        public List<GameObject> targetForceHighlight_planes;
        private List<Renderer> _renderersTargetForce;

        private GrabAndMoveScenario _currentScenario;
        [FormerlySerializedAs("_fileLineIndex")]
        private string[] _fileLines;

        [SerializeField]
        private ParticleSystem _explodingParticles;
        private Renderer _renderer;

        private Material _defaultMaterial;

        public Text textBox;
        private string _triggerString;
        private int _maxRep;

        public Text textBox_modality;
        public GameObject tactilityManager;

        private AbstractBoxController _boxController;

        private bool isPaused;

        private void Start()
        {
            
            _currentScenario = FindObjectOfType<GrabAndMoveScenario>();
            _boxController = FindObjectOfType<AbstractBoxController>();

            _currentScenario.WhenOnSuccess += HandleTaskComplete;
            _currentScenario.WhenOnFailure += HandleTaskComplete;
            // HandleTaskComplete();
            _renderer = _currentScenario.floatable.GetComponent<Renderer>();
            _defaultMaterial = _renderer.material;

            _renderersTargetForce = new List<Renderer>(targetForceHighlight_planes.Count);
            for (int i = 0; i < targetForceHighlight_planes.Count; i++)
            {
                _renderersTargetForce.Add(targetForceHighlight_planes[i].GetComponent<Renderer>());
            }

            Transform[] children = tactilityManager.GetComponentsInChildren<Transform>(true);

            // Iterate through each child and check their active status
            foreach (Transform child in children)
            {
                // Skip the parent object itself
                if (child == tactilityManager.transform)
                    continue;

                // Check if the child is active
                bool isActive = child.gameObject.activeSelf;
                if (isActive)
                {
                    textBox_modality.text = child.gameObject.name;
                }
            }

            ResumeGame();

        }

        private void Update()
        {
            if (_fileLines == null)
            {
                _fileLines = new string[] { };
                var textAsset = Resources.Load<TextAsset>("Tactility/TestOrderFiles/TestOrder_fixedPos");
                _fileLines = textAsset.text.Split("\r\n");
                HandleTaskComplete(ScenarioTrigger.Idle);

                SetNextPosition();
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
            //var targetForceLevels = new int[] { 1, 3, 6, 5, 2, 4, 2, 1, 5, 6, 4, 3, 3, 1, 5, 6, 4, 2, 5, 1, 4, 2, 3, 6, 2, 4, 3, 5, 6, 1 };
            var targetForceLevels = new int[] { 1, 3, 5, 2, 4, 2, 1, 5, 4, 3, 3, 1, 5, 4, 2, 5, 1, 4, 2, 3, 2, 4, 3, 5, 1 };

            var targetPositions = new List<Vector3>
            {
                new Vector3(-0.2f, 0.9f, 0.5f),
                new Vector3(-0.1f, 1.1f, 0.5f),
                new Vector3(0.1f, 0.9f, 0.5f),
                new Vector3(0.1f, 1.1f, 0.5f),
                new Vector3(-0.1f, 0.9f, 0.7f),
                new Vector3(-0.1f, 1.1f, 0.7f),
                new Vector3(0.1f, 0.9f, 0.7f),
                new Vector3(0.1f, 1.1f, 0.7f)
            };
            fileLineIndex++;

            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 1:
                    _maxRep = 10;
                    _triggerString = "Familiarization";
                    if (fileLineIndex == 11)
                    {
                        TogglePause();
                    }
                    break;
                case 2:
                    _maxRep = 10;
                    _triggerString = "Training #1";
                    if (fileLineIndex == 11)
                    {
                        TogglePause();
                    }
                    break;
                case 3:
                    _maxRep = 25;
                    _triggerString = "Training #2";
                    if (fileLineIndex == 26)
                    {
                        TogglePause();
                    }
                    break;
                case 4:
                    _maxRep = 25;
                    _triggerString = "Validation";
                    if (fileLineIndex == 26)
                    {
                        TogglePause();
                    }
                    break;
            }

            targetForceLevel = targetForceLevels[fileLineIndex - 1];
            Logger.LogSceneChange(fileLineIndex, targetForceLevel);

            //Reset to baseline the color of all planes but the one of the target force level
            for (int i = 0; i < targetForceHighlight_planes.Count; i++)
            {
                _renderersTargetForce[i].material = targetForceHighlight_materials[0];
            }
            _renderersTargetForce[targetForceLevel-1].material = targetForceHighlight_materials[1];

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
            textBox.text = $"Scenario: {_triggerString}\nTrial: {fileLineIndex}/{_maxRep}";

            var nextPositions = GetNextPositions();
            _currentScenario.originPosition = nextPositions.Item1;
            _currentScenario.targetPosition = nextPositions.Item2;
            
            _currentScenario.floatable.OriginPoint = _currentScenario.originPosition;
            _currentScenario.grabbable.allowGrabbing = true;
            
            // Set currentMaterial in accordance with targetForceLevel
            _currentScenario.floatable.GetComponent<Renderer>()!.enabled = true;



            // Logger.LogPositions(_currentScenario.originPosition, _currentScenario.targetPosition);
            // _currentScenario.floatable.GetComponent<Renderer>()!.material = materials[targetForceLevel - 1];
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Logger.LogStimModality(textBox_modality.text);

            if (isPaused)
            {
                // Pause the game
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game
                Time.timeScale = 1;
            }
        }

        // This method can be called by UI elements to resume the game
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1;
            _boxController.ResetAllPads();
        }

    }
}
