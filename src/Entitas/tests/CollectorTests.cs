using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class CollectorTests
    {
        readonly IContext<Entity> _context;
        readonly IGroup<Entity> _groupA;
        readonly ICollector<Entity> _collector;

        public CollectorTests()
        {
            _context = new TestContext();
            _groupA = _context.GetGroup(Matcher<Entity>.AllOf(IndexA));
            _collector = new Collector<Entity>(_groupA, GroupEvent.Added);
        }

        [Fact]
        public void IsEmpty()
        {
            _collector.collectedEntities.Should().BeEmpty();
        }

        [Fact]
        public void ReturnsCollectedEntitiesWhenGroupEventAdded()
        {
            var entity = CreateEntityA();
            var entities = _collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
        }

        [Fact]
        public void OnlyCollectsMatchingEntities()
        {
            CreateEntityB();
            var entities = _collector.collectedEntities;
            entities.Count.Should().Be(0);
        }

        [Fact]
        public void CollectsEntitiesOnlyOnce()
        {
            var entity = CreateEntityA();
            entity.RemoveComponent(IndexA);
            entity.AddComponent(IndexA, new ComponentA());
            var entities = _collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
        }

        [Fact]
        public void ClearsCollectedEntities()
        {
            CreateEntityA();
            _collector.ClearCollectedEntities();
            _collector.collectedEntities.Should().BeEmpty();
        }

        [Fact]
        public void ClearsCollectedEntitiesOnDeactivation()
        {
            CreateEntityA();
            _collector.Deactivate();
            _collector.collectedEntities.Should().BeEmpty();
        }

        [Fact]
        public void DoesNotCollectEntitiesWhenDeactivated()
        {
            _collector.Deactivate();
            CreateEntityA();
            _collector.collectedEntities.Should().BeEmpty();
        }

        [Fact]
        public void ContinuesCollectingWhenActivated()
        {
            _collector.Deactivate();
            CreateEntityA();
            _collector.Activate();
            var entity = CreateEntityA();
            var entities = _collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
        }

        [Fact]
        public void CollectorToString()
        {
            _collector.ToString().Should().Be($"Collector(Group(AllOf({IndexA})))");
        }

        [Fact]
        public void RetainsEntityEvenAfterDestroy()
        {
            var entity = CreateEntityA();
            var didExecute = 0;
            entity.OnEntityReleased += delegate { didExecute += 1; };
            entity.Destroy();
            entity.retainCount.Should().Be(1);

            if (entity.aerc is SafeAERC safeAerc)
                safeAerc.owners.Should().Contain(_collector);

            didExecute.Should().Be(0);
        }

        [Fact]
        public void ReleasesEntityWhenClearingCollectedEntities()
        {
            var entity = CreateEntityA();
            entity.Destroy();
            _collector.ClearCollectedEntities();
            entity.retainCount.Should().Be(0);
        }

        [Fact]
        public void RetainsEntitiesOnlyOnce()
        {
            var entity = CreateEntityA();
            entity.ReplaceComponent(IndexA, new ComponentA());
            entity.Destroy();
            entity.retainCount.Should().Be(1);
        }

        [Fact]
        public void ReturnsCollectedEntitiesWhenGroupEventRemoved()
        {
            var collector = new Collector<Entity>(_groupA, GroupEvent.Removed);
            var entity = CreateEntityA();
            collector.collectedEntities.Should().BeEmpty();
            entity.RemoveComponent(IndexA);
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
        }

        [Fact]
        public void ReturnsCollectedEntitiesWhenGroupEventAddedOrRemoved()
        {
            var collector = new Collector<Entity>(_groupA, GroupEvent.AddedOrRemoved);
            var entity = CreateEntityA();
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
            collector.ClearCollectedEntities();
            entity.RemoveComponent(IndexA);
            entities = collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entity);
        }

        [Fact]
        public void ThrowsWhenGroupCountDoesNotEqualGroupEventCount()
        {
            FluentActions.Invoking(() => new Collector<Entity>(
                new[] {_groupA},
                new[]
                {
                    GroupEvent.Added,
                    GroupEvent.Added
                }
            )).Should().Throw<CollectorException>();
        }

        [Fact]
        public void ReturnsCollectedEntitiesForMultipleGroupsWhenGroupEventAdded()
        {
            var collector = new Collector<Entity>(
                new[] {_groupA, _context.GetGroup(Matcher<Entity>.AllOf(IndexB))},
                new[] {GroupEvent.Added, GroupEvent.Added}
            );
            var entityA = CreateEntityA();
            var entityB = CreateEntityB();
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(2);
            entities.Should().Contain(entityA);
            entities.Should().Contain(entityB);
        }

        [Fact]
        public void CollectorWithMultipleGroupsToString()
        {
            new Collector<Entity>(
                new[] {_groupA, _context.GetGroup(Matcher<Entity>.AllOf(IndexB))},
                new[] {GroupEvent.Added, GroupEvent.Added}
            ).ToString().Should().Be($"Collector(Group(AllOf({IndexA})), Group(AllOf({IndexB})))");
        }

        [Fact]
        public void ReturnsCollectedEntitiesForMultipleGroupsWhenGroupEventRemoved()
        {
            var collector = new Collector<Entity>(
                new[] {_groupA, _context.GetGroup(Matcher<Entity>.AllOf(IndexB))},
                new[] {GroupEvent.Removed, GroupEvent.Removed}
            );
            var entityA = CreateEntityA();
            var entityB = CreateEntityB();
            collector.collectedEntities.Should().BeEmpty();
            entityA.RemoveComponent(IndexA);
            entityB.RemoveComponent(IndexB);
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(2);
            entities.Should().Contain(entityA);
            entities.Should().Contain(entityB);
        }

        [Fact]
        public void ReturnsCollectedEntitiesForMultipleGroupsWhenGroupEventAddedOrRemoved()
        {
            var collector = new Collector<Entity>(
                new[] {_groupA, _context.GetGroup(Matcher<Entity>.AllOf(IndexB))},
                new[] {GroupEvent.AddedOrRemoved, GroupEvent.AddedOrRemoved}
            );
            var entityA = CreateEntityA();
            var entityB = CreateEntityB();
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(2);
            entities.Should().Contain(entityA);
            entities.Should().Contain(entityB);
            collector.ClearCollectedEntities();

            entityA.RemoveComponent(IndexA);
            entityB.RemoveComponent(IndexB);
            entities = collector.collectedEntities;
            entities.Count.Should().Be(2);
            entities.Should().Contain(entityA);
            entities.Should().Contain(entityB);
        }

        [Fact]
        public void ReturnsCollectedEntitiesForMultipleGroupsWithMixedGroupEvents()
        {
            var collector = new Collector<Entity>(
                new[] {_groupA, _context.GetGroup(Matcher<Entity>.AllOf(IndexB))},
                new[] {GroupEvent.Added, GroupEvent.Removed}
            );
            var entityA = CreateEntityA();
            var entityB = CreateEntityB();
            var entities = collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entityA);
            collector.ClearCollectedEntities();

            entityA.RemoveComponent(IndexA);
            entityB.RemoveComponent(IndexB);
            entities = collector.collectedEntities;
            entities.Count.Should().Be(1);
            entities.Should().Contain(entityB);
        }

        Entity CreateEntityA()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            return entity;
        }

        Entity CreateEntityB()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexB, new ComponentB());
            return entity;
        }
    }
}
