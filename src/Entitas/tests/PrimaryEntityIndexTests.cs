using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class PrimaryEntityIndexTests
    {
        const string TestKey = "Test";

        readonly IContext<Entity> _context = new TestContext();
        readonly PrimaryEntityIndex<Entity, string> _index;

        public PrimaryEntityIndexTests() => _index = CreateIndex();

        [Fact]
        public void HasNoEntities()
        {
            _index.GetEntity("unknown").Should().BeNull();
        }

        [Fact]
        public void GetsEntityForKey()
        {
            var entity = CreateNameAgeEntity();
            _index.GetEntity(TestKey).Should().BeSameAs(entity);
        }

        [Fact]
        public void RetainsEntity()
        {
            var entity = CreateNameAgeEntity();
            entity.retainCount.Should().Be(3); // Context, Group, EntityIndex
        }

        [Fact]
        public void HasExistingEntity()
        {
            var entity = CreateNameAgeEntity();
            CreateIndex().GetEntity(TestKey).Should().BeSameAs(entity);
        }

        [Fact]
        public void ReleasesAndRemovesEntityWhenComponentGetsRemoved()
        {
            var entity = CreateNameAgeEntity();
            entity.RemoveComponent(IndexA);
            _index.GetEntity(TestKey).Should().BeNull();
            entity.retainCount.Should().Be(1); // Context
        }

        [Fact]
        public void ThrowsWhenAddingEntityForTheSameKey()
        {
            CreateNameAgeEntity();
            FluentActions.Invoking(CreateNameAgeEntity)
                .Should().Throw<EntityIndexException>();
        }

        [Fact]
        public void PrimaryIndexToString()
        {
            _index.ToString().Should().Be($"PrimaryEntityIndex(TestIndex)");
        }

        [Fact]
        public void DeactivateClearsIndexAndReleasesEntity()
        {
            var entity = CreateNameAgeEntity();
            _index.Deactivate();
            _index.GetEntity(TestKey).Should().BeNull();
            entity.retainCount.Should().Be(2); // Context, Group
        }

        [Fact]
        public void DoesNotAddEntitiesWhenDeactivated()
        {
            _index.Deactivate();
            CreateNameAgeEntity();
            _index.GetEntity(TestKey).Should().BeNull();
        }

        [Fact]
        public void HasExistingEntityWhenReactivated()
        {
            var entity = CreateNameAgeEntity();
            _index.Deactivate();
            _index.Activate();
            _index.GetEntity(TestKey).Should().BeSameAs(entity);
        }

        [Fact]
        public void AddsEntitiesWhenReactivated()
        {
            _index.Deactivate();
            _index.Activate();
            var entity = CreateNameAgeEntity();
            _index.GetEntity(TestKey).Should().BeSameAs(entity);
        }

        PrimaryEntityIndex<Entity, string> CreateIndex()
        {
            return new("TestIndex", _context.GetGroup(Matcher<Entity>.AllOf(IndexA)),
                (e, c) => (c is NameAgeComponent nameAge ? nameAge : (NameAgeComponent)e.GetComponent(IndexA)).Name);
        }

        Entity CreateNameAgeEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new NameAgeComponent {Name = TestKey});
            return entity;
        }
    }
}
