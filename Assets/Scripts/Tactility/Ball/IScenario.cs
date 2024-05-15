// Copyright (C) 2024 Peter Leth

namespace Tactility.Ball
{
    public enum CauseOfFailure
    {
        LossOfGrab,
        TooLittlePressure,
        TooMuchPressure,
    }
    
    public delegate void ScenarioOnSuccess();
    public delegate void ScenarioOnFailure(CauseOfFailure cause);

    public interface IScenario
    {
        public float Progress { get; }

        event ScenarioOnSuccess WhenOnSuccess;
        event ScenarioOnFailure WhenOnFailure;
    }
}
