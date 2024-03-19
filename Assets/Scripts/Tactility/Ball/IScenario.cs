// Copyright (C) 2024 Peter Leth

namespace Tactility.Ball
{
    public delegate void ScenarioOnSuccess();
    public delegate void ScenarioOnFailure();

    public interface IScenario
    {
        public float Progress { get; }

        event ScenarioOnSuccess WhenOnSuccess;
        event ScenarioOnFailure WhenOnFailure;
    }
}
