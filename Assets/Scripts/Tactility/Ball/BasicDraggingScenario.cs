using UnityEngine;
namespace Tactility.Ball
{
    public class BasicDraggingScenario : MonoBehaviour, IScenario
    {
        public float Progress { get; }
        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;
    }
}
