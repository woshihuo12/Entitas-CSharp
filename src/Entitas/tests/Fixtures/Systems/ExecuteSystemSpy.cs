namespace Entitas.Tests.Fixtures.Systems
{
    public class ExecuteSystemSpy : IExecuteSystem
    {
        public int DidExecute { get; private set; }

        public void Execute() => DidExecute += 1;
    }
}
