using Entitas.Tests.Fixtures.Entities;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests.Fixtures
{
    public class TestContexts : IContexts
    {
        public IContext[] allContexts { get; }

        public Context<IMyEntity> Context1 => _context1;
        public Context<IMyEntity> Context2 => _context1;

        readonly Context<IMyEntity> _context1;
        readonly Context<IMyEntity> _context2;

        public TestContexts()
        {
            _context1 = new Context<IMyEntity>(TotalComponents, () => new TestEntity());
            _context2 = new Context<IMyEntity>(TotalComponents, () => new TestEntity());
        }
    }
}
