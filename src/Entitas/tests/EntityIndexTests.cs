using System.Collections.Generic;
using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class EntityIndexTests
    {
        const string TestKey = "Test";

        readonly IContext<Entity> _context = new TestContext();
        readonly EntityIndex<Entity, string> _index;

        public EntityIndexTests() => _index = CreateIndex();

        [Fact]
        public void HasNoEntities()
        {
            _index.GetEntities("unknown").Should().BeEmpty();
        }

        [Fact]
        public void GetsEntitiesForKey()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            var entities = _index.GetEntities(TestKey);
            entities.Count.Should().Be(2);
            entities.Should().Contain(entity1);
            entities.Should().Contain(entity2);
        }

        [Fact]
        public void RetainsEntity()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            entity1.retainCount.Should().Be(3); // Context, Group, EntityIndex
            entity2.retainCount.Should().Be(3); // Context, Group, EntityIndex

            if (entity1.aerc is SafeAERC safeAerc1)
                safeAerc1.owners.Should().Contain(_index);

            if (entity1.aerc is SafeAERC safeAerc2)
                safeAerc2.owners.Should().Contain(_index);
        }

        [Fact]
        public void HasExistingEntities()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            var entities = CreateIndex().GetEntities(TestKey);
            entities.Count.Should().Be(2);
            entities.Should().Contain(entity1);
            entities.Should().Contain(entity2);
        }

        [Fact]
        public void ReleasesAndRemovesEntityWhenComponentGetsRemoved()
        {
            var entity = CreateNameAgeEntity();
            CreateNameAgeEntity();
            entity.RemoveComponent(IndexA);
            _index.GetEntities(TestKey).Count.Should().Be(1);
            entity.retainCount.Should().Be(1); // Context
        }

        [Fact]
        public void IndexToString()
        {
            _index.ToString().Should().Be("EntityIndex(TestIndex)");
        }

        [Fact]
        public void DeactivateClearsIndexAndReleasesEntity()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            _index.Deactivate();
            _index.GetEntities(TestKey).Should().BeEmpty();
            entity1.retainCount.Should().Be(2); // Context, Group
            entity2.retainCount.Should().Be(2); // Context, Group
        }

        [Fact]
        public void DoesNotAddEntitiesWhenDeactivated()
        {
            _index.Deactivate();
            CreateNameAgeEntity();
            _index.GetEntities(TestKey).Should().BeEmpty();
        }

        [Fact]
        public void HasExistingEntitiesWhenReactivated()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            _index.Deactivate();
            _index.Activate();
            var entities = _index.GetEntities(TestKey);
            entities.Count.Should().Be(2);
            entities.Should().Contain(entity1);
            entities.Should().Contain(entity2);
        }

        [Fact]
        public void AddsNewEntitiesWhenReactivated()
        {
            var entity1 = CreateNameAgeEntity();
            var entity2 = CreateNameAgeEntity();
            _index.Deactivate();
            _index.Activate();
            var entity3 = CreateNameAgeEntity();
            var entities = _index.GetEntities(TestKey);
            entities.Count.Should().Be(3);
            entities.Should().Contain(entity1);
            entities.Should().Contain(entity2);
            entities.Should().Contain(entity3);
        }

        [Fact]
        public void GetsLastComponentThatTriggeredAddingEntityToGroup()
        {
            IComponent receivedComponent = null;
            var group = _context.GetGroup(Matcher<Entity>.AllOf(IndexA, IndexB));
            new EntityIndex<Entity, string>("TestIndex", group, (_, c) =>
            {
                receivedComponent = c;
                return ((NameAgeComponent)c).Name;
            });

            var nameAgeComponent1 = new NameAgeComponent {Name = "Name 1"};
            var nameAgeComponent2 = new NameAgeComponent {Name = "Name 2"};

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, nameAgeComponent1);
            entity.AddComponent(IndexB, nameAgeComponent2);

            receivedComponent.Should().BeSameAs(nameAgeComponent2);
        }

        [Fact]
        public void SupportsNoneOfMatcher()
        {
            var receivedComponents = new List<IComponent>();
            var nameAgeComponent1 = new NameAgeComponent {Name = "Name 1"};
            var nameAgeComponent2 = new NameAgeComponent {Name = "Name 2"};
            var group = _context.GetGroup(Matcher<Entity>.AllOf(IndexA).NoneOf(IndexB));
            var index = new EntityIndex<Entity, string>("TestIndex", group, (e, c) =>
            {
                receivedComponents.Add(c);
                return c == nameAgeComponent1
                    ? ((NameAgeComponent)c).Name
                    : ((NameAgeComponent)e.GetComponent(IndexA)).Name;
            });

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, nameAgeComponent1);
            entity.AddComponent(IndexB, nameAgeComponent2);

            receivedComponents.Count.Should().Be(2);
            receivedComponents[0].Should().Be(nameAgeComponent1);
            receivedComponents[1].Should().Be(nameAgeComponent2);
            index.GetEntities(TestKey).Count.Should().Be(0);
        }

        EntityIndex<Entity, string> CreateIndex()
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
