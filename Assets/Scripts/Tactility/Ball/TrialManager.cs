using System.Collections.Generic;
using UnityEngine;

namespace Tactility.Ball
{
    public class TrialManager : MonoBehaviour
    {
        public int currentForceLevel;
        [SerializeField]
        private List<Material> materials;
        
        private GrabAndMoveScenario _currentScenario;
        private int _fileLineIndex;
        private string[] _fileLines;
        
        private void Start()
        {
            _currentScenario = FindObjectOfType<GrabAndMoveScenario>();

            _currentScenario.WhenOnSuccess += SetNextPositions;
            _currentScenario.WhenOnFailure += SetNextPositions;
            // SetNextPositions();
        }

        private void Update()
        {
            if (_fileLines == null)
            {
                _fileLines = new string[] { };
                var textAsset = Resources.Load<TextAsset>("Tactility/TestOrder30");
                _fileLines = textAsset.text.Split("\r\n");
                SetNextPositions();
            }
        }

        private (Vector3, Vector3) GetNextPositions()
        {
            var resultString = _fileLines[_fileLineIndex];
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
            _fileLineIndex++;
            
            // Increment currentForceLevel when modulus of 6 is 0
            if (_fileLineIndex % 6 == 0)
            {
                currentForceLevel++;
                
                // Reset currentForceLevel to 0 when it reaches 6
                if (currentForceLevel == 6)
                {
                    currentForceLevel = 0;
                }
            }

            var originIndex = int.Parse(resultString[0].ToString());
            var targetIndex = int.Parse(resultString[2].ToString());
            return (targetPositions[originIndex],targetPositions[targetIndex]);
        }

        private void SetNextPositions()
        {
            var nextPositions = GetNextPositions();
            _currentScenario.originPosition = nextPositions.Item1;
            _currentScenario.targetPosition = nextPositions.Item2;
            
            // Print item1 and 2
            Debug.Log(nextPositions.Item1);
            Debug.Log(nextPositions.Item2);
            
            _currentScenario.floatable.OriginPoint = _currentScenario.originPosition;
            
            // Set currentMaterial in accordance with currentForceLevel
            _currentScenario.floatable.GetComponent<Renderer>()!.material = materials[currentForceLevel];
        }
    }
}
