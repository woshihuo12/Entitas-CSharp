namespace Entitas.Tests.Fixtures.Systems
{
    public class InitializeSystemSpy : IInitializeSystem
    {
        public int DidInitialize { get; private set; }

        public void Initialize() => DidInitialize += 1;
    }
}
