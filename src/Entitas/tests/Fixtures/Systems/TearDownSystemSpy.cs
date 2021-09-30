namespace Entitas.Tests.Fixtures.Systems
{
    public class TearDownSystemSpy : ITearDownSystem
    {
        public int DidTearDown { get; private set; }

        public void TearDown() => DidTearDown += 1;
    }
}
