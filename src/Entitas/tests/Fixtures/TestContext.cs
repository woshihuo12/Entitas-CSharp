namespace Entitas.Tests.Fixtures
{
    public class TestContext : Context<Entity>
    {
        public TestContext() : base(TestHelper.TotalComponents, () => new Entity()) { }

        public TestContext(int totalComponents, int startCreationIndex, ContextInfo contextInfo)
            : base(totalComponents, startCreationIndex, contextInfo, (entity) => new SafeAERC(entity), () => new Entity()) { }
    }
}
