namespace Internal.Ball
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
