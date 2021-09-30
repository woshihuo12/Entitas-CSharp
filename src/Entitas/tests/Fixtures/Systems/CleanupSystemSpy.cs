namespace Entitas.Tests.Fixtures.Systems
{
    public class CleanupSystemSpy : ICleanupSystem
    {
        public int DidCleanup { get; private set; }

        public void Cleanup() => DidCleanup += 1;
    }
}
