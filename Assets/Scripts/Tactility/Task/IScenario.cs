// Copyright (C) 2024 Peter Leth

namespace Tactility.Task
{
    public enum ScenarioTrigger
    {
        Success,
        Idle,
        LossOfGrab,
        TooLittlePressure,
        TooMuchPressure,
    }
    
    public delegate void ScenarioOnSuccess(ScenarioTrigger cause);
    public delegate void ScenarioOnFailure(ScenarioTrigger cause);

    public interface IScenario
    {
        public float Progress { get; }

        event ScenarioOnSuccess WhenOnSuccess;
        event ScenarioOnFailure WhenOnFailure;
    }
}
