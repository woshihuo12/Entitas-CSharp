using System.Linq;
using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class MultiKeyEntityIndexTests
    {
        const string TestKey = "Test";

        readonly IContext<Entity> _context = new TestContext();
        readonly EntityIndex<Entity, string> _index;

        public MultiKeyEntityIndexTests() => _index = CreateIndex();

        [Fact]
        public void HasNoEntities()
        {
            _index.GetEntities("unknown").Should().BeEmpty();
        }

        [Fact]
        public void GetsEntityForKey()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            _index.GetEntities("1").Single().Should().BeSameAs(entity1);
            _index.GetEntities("2").Should().Contain(entity1);
            _index.GetEntities("2").Should().Contain(entity2);
            _index.GetEntities("3").Single().Should().BeSameAs(entity2);
        }

        [Fact]
        public void RetainsEntity()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            entity1.retainCount.Should().Be(3);
            entity2.retainCount.Should().Be(3);

            if (entity1.aerc is SafeAERC safeAerc1)
                safeAerc1.owners.Should().Contain(_index);

            if (entity1.aerc is SafeAERC safeAerc2)
                safeAerc2.owners.Should().Contain(_index);
        }

        [Fact]
        public void HasExistingEntities()
        {
            CreateNameAgeEntity();
            CreateNameAgeEntity();
            var index = CreateIndex();
            index.GetEntities("1").Count.Should().Be(1);
            index.GetEntities("2").Count.Should().Be(2);
            index.GetEntities("3").Count.Should().Be(1);
        }

        [Fact]
        public void ReleasesAndRemovesEntityWhenComponentGetsRemoved()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            entity1.RemoveComponent(IndexA);
            _index.GetEntities("1").Count.Should().Be(0);
            _index.GetEntities("2").Count.Should().Be(1);
            _index.GetEntities("3").Count.Should().Be(1);
            entity1.retainCount.Should().Be(1);
            entity2.retainCount.Should().Be(3);

            if (entity1.aerc is SafeAERC safeAerc1)
                safeAerc1.owners.Should().NotContain(_index);

            if (entity2.aerc is SafeAERC safeAerc2)
                safeAerc2.owners.Should().Contain(_index);
        }

        [Fact]
        public void HasExistingEntitiesWhenReactivated()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            _index.Deactivate();
            _index.Activate();
            _index.GetEntities("1").First().Should().BeSameAs(entity1);
            _index.GetEntities("2").Should().Contain(entity1);
            _index.GetEntities("2").Should().Contain(entity2);
            _index.GetEntities("3").First().Should().BeSameAs(entity2);
        }

        EntityIndex<Entity, string> CreateIndex()
        {
            return new("TestIndex", _context.GetGroup(Matcher<Entity>.AllOf(IndexA)),
                (e, _) => e.creationIndex == 0
                    ? new[] {"1", "2"}
                    : new[] {"2", "3"});
        }

        Entity CreateNameAgeEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new NameAgeComponent {Name = TestKey});
            return entity;
        }
    }
}
