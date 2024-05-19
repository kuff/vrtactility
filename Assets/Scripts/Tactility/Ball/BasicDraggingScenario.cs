using UnityEngine;
namespace Tactility.Ball
{
    public class BasicDraggingScenario : MonoBehaviour, IScenario
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public float Progress { get; }
#pragma warning disable CS0067 // The event is never used
        public event ScenarioOnSuccess WhenOnSuccess;
        public event ScenarioOnFailure WhenOnFailure;
#pragma warning restore CS0067 // The event is never used
    }
}
