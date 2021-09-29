using System;
using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class ContextTests
    {
        readonly IMatcher<Entity> _matcherA = Matcher<Entity>.AllOf(IndexA);
        readonly IMatcher<Entity> _matcherB = Matcher<Entity>.AllOf(IndexB);
        readonly IMatcher<Entity> _matcherAb = Matcher<Entity>.AllOf(new[] {IndexA, IndexB});

        IContext<Entity> _context = new TestContext();

        [Fact]
        public void IncrementsCreationIndex()
        {
            _context.CreateEntity().creationIndex.Should().Be(0);
            _context.CreateEntity().creationIndex.Should().Be(1);
        }

        [Fact]
        public void StartsWithGivenCreationIndex()
        {
            new TestContext(TotalComponents, 42, null).CreateEntity().creationIndex.Should().Be(42);
        }

        [Fact]
        public void HasNoEntitiesWhenNoEntitiesWereCreated()
        {
            _context.GetEntities().Should().BeEmpty();
        }

        [Fact]
        public void EntityCountIsZero()
        {
            _context.count.Should().Be(0);
        }

        [Fact]
        public void CreatesEntity()
        {
            var entity = _context.CreateEntity();
            entity.Should().NotBeNull();
            entity.GetType().Should().Be(typeof(Entity));
            entity.totalComponents.Should().Be(_context.totalComponents);
            entity.isEnabled.Should().BeTrue();
        }

        [Fact]
        public void EntityHasDefaultContextInfo()
        {
            _context.contextInfo.name.Should().Be("Unnamed Context");
            _context.contextInfo.componentNames.Length.Should().Be(TotalComponents);
            for (var i = 0; i < _context.contextInfo.componentNames.Length; i++)
            {
                _context.contextInfo.componentNames[i].Should().Be("Index " + i);
            }
        }

        [Fact]
        public void CreatesComponentPools()
        {
            _context.componentPools.Should().NotBeNull();
            _context.componentPools.Length.Should().Be(TotalComponents);
        }

        [Fact]
        public void CreatesEntityWithComponentPools()
        {
            _context.CreateEntity().componentPools.Should().BeSameAs(_context.componentPools);
        }

        [Fact]
        public void CanToString()
        {
            _context.ToString().Should().Be("Unnamed Context");
        }

        [Fact]
        public void HasCustomContextInfo()
        {
            var contextInfo = SetCustomContext();
            _context.contextInfo.Should().BeSameAs(contextInfo);
        }

        [Fact]
        public void CreatesEntityWithCustomContextInfo()
        {
            var contextInfo = SetCustomContext();
            _context.CreateEntity().contextInfo.Should().BeSameAs(contextInfo);
        }

        [Fact]
        public void ThrowsWhenComponentNamesIsNotSameLengthAsTotalComponents()
        {
            FluentActions.Invoking(() => new TestContext(2, 0, new ContextInfo(string.Empty, new[] {"ComponentA"}, new[] {typeof(ComponentA)})))
                .Should().Throw<ContextInfoException>();
        }

        [Fact]
        public void GetsTotalEntityCount()
        {
            _context.CreateEntity();
            _context.count.Should().Be(1);
        }

        [Fact]
        public void ContainsCreatedEntities()
        {
            var entity = _context.CreateEntity();
            _context.HasEntity(entity).Should().BeTrue();
        }

        [Fact]
        public void ReturnsAllCreatedEntities()
        {
            var e1 = _context.CreateEntity();
            var e2 = _context.CreateEntity();
            var entities = _context.GetEntities();
            entities.Length.Should().Be(2);
            entities.Should().Contain(e1);
            entities.Should().Contain(e2);
        }

        [Fact]
        public void DestroysAndRemovesEntity()
        {
            var entity = _context.CreateEntity();
            entity.Destroy();
            _context.HasEntity(entity).Should().BeFalse();
            _context.count.Should().Be(0);
            _context.GetEntities().Should().BeEmpty();
        }

        [Fact]
        public void DestroysEntityAndRemovesAllItsComponents()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();
            entity.GetComponents().Should().BeEmpty();
        }

        [Fact]
        public void RemovesOnDestroyEntityHandler()
        {
            var didDestroy = 0;
            _context.OnEntityWillBeDestroyed += delegate { didDestroy += 1; };
            _context.CreateEntity().Destroy();
            _context.CreateEntity().Destroy();
            didDestroy.Should().Be(2);
        }

        [Fact]
        public void DestroysAllEntities()
        {
            var entity = _context.CreateEntity();
            _context.CreateEntity();
            _context.DestroyAllEntities();
            _context.HasEntity(entity).Should().BeFalse();
            _context.count.Should().Be(0);
            _context.GetEntities().Should().BeEmpty();
            entity.GetComponents().Should().BeEmpty();
        }

        [Fact]
        public void EnsuresSameDeterministicOrderWhenGettingEntitiesAfterDestroyingAllEntities()
        {
            // This is a Unity specific problem. Run Unity Test Tools with in the Entitas.Unity project

            const int numEntities = 10;

            for (var i = 0; i < numEntities; i++)
                _context.CreateEntity();

            var order1 = new int[numEntities];
            var entities1 = _context.GetEntities();
            for (var i = 0; i < numEntities; i++)
                order1[i] = entities1[i].creationIndex;

            _context.DestroyAllEntities();
            _context.ResetCreationIndex();

            for (var i = 0; i < numEntities; i++)
                _context.CreateEntity();

            var order2 = new int[numEntities];
            var entities2 = _context.GetEntities();
            for (var i = 0; i < numEntities; i++)
                order2[i] = entities2[i].creationIndex;

            order1.Should().BeEquivalentTo(order2);
        }

        [Fact]
        public void ThrowsWhenDestroyingAllEntitiesAndThereAreStillEntitiesRetained()
        {
            _context.CreateEntity().Retain(new object());
            _context.Invoking(context => context.DestroyAllEntities())
                .Should().Throw<ContextStillHasRetainedEntitiesException>();
        }

        [Fact]
        public void CachesEntities()
        {
            _context.CreateEntity();
            _context.GetEntities().Should().BeSameAs(_context.GetEntities());
        }

        [Fact]
        public void UpdatesEntitiesCacheWhenCreatingEntity()
        {
            _context.CreateEntity();
            var entities = _context.GetEntities();
            _context.CreateEntity();
            _context.GetEntities().Should().NotBeSameAs(entities);
        }

        [Fact]
        public void UpdatesEntitiesCacheWhenDestroyingEntity()
        {
            var entity = _context.CreateEntity();
            var entities = _context.GetEntities();
            entity.Destroy();
            _context.GetEntities().Should().NotBeSameAs(entities);
        }

        [Fact]
        public void DispatchesOnEntityCreatedWhenCreatingNewEntity()
        {
            var didDispatch = 0;
            IEntity eventEntity = null;
            _context.OnEntityCreated += (c, e) =>
            {
                didDispatch += 1;
                eventEntity = e;
                c.Should().BeSameAs(_context);
            };

            var entity = _context.CreateEntity();
            didDispatch.Should().Be(1);
            eventEntity.Should().BeSameAs(entity);
        }

        [Fact]
        public void DispatchesOnEntityWillBeDestroyedWhenDestroyingEntity()
        {
            var didDispatch = 0;
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            _context.OnEntityWillBeDestroyed += (c, e) =>
            {
                didDispatch += 1;
                c.Should().BeSameAs(_context);
                e.Should().BeSameAs(e);
                e.HasComponent(IndexA).Should().BeTrue();
                e.isEnabled.Should().BeTrue();

                ((IContext<Entity>)c).GetEntities().Length.Should().Be(0);
            };
            _context.GetEntities();
            entity.Destroy();
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DispatchesOnEntityDestroyedWhenDestroyingEntity()
        {
            var didDispatch = 0;
            var entity = _context.CreateEntity();
            _context.OnEntityDestroyed += (p, e) =>
            {
                didDispatch += 1;
                p.Should().BeSameAs(_context);
                e.Should().BeSameAs(e);
                e.HasComponent(IndexA).Should().BeFalse();
                e.isEnabled.Should().BeFalse();
            };
            entity.Destroy();
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void ReleasesEntityAfterOnEntityDestroyed()
        {
            var didDispatch = 0;
            var entity = _context.CreateEntity();
            _context.OnEntityDestroyed += (_, e) =>
            {
                didDispatch += 1;
                e.retainCount.Should().Be(1);
                var newEntity = _context.CreateEntity();
                newEntity.Should().NotBeNull();
                newEntity.Should().NotBeSameAs(e);
            };
            entity.Destroy();
            var reusedEntity = _context.CreateEntity();
            reusedEntity.Should().BeSameAs(entity);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void ThrowsWhenEntityIsReleasedBeforeIIsDestroyed()
        {
            _context.CreateEntity().Invoking(entity => entity.Release(_context))
                .Should().Throw<EntityIsNotDestroyedException>();
        }

        [Fact]
        public void DispatchesOnGroupCreatedWhenCreatingNewGroup()
        {
            var didDispatch = 0;
            IGroup eventGroup = null;
            _context.OnGroupCreated += (p, g) =>
            {
                didDispatch += 1;
                p.Should().BeSameAs(_context);
                eventGroup = g;
            };
            var group = _context.GetGroup(_matcherA);
            didDispatch.Should().Be(1);
            eventGroup.Should().BeSameAs(group);
        }

        [Fact]
        public void DoesNotDispatchOnGroupCreatedWhenGroupAlreadyExists()
        {
            _context.GetGroup(_matcherA);
            _context.OnGroupCreated += delegate { throw new Exception(); };
            _context.GetGroup(_matcherA);
        }

        [Fact]
        public void RemovesAllExternalDelegatesWhenDestroyingEntity()
        {
            var e = _context.CreateEntity();
            e.OnComponentAdded += delegate { throw new Exception(); };
            e.OnComponentRemoved += delegate { throw new Exception(); };
            e.OnComponentReplaced += delegate { throw new Exception(); };
            e.Destroy();
            var e2 = _context.CreateEntity();
            e2.Should().BeSameAs(e);
            e2.AddComponent(IndexA, new ComponentA());
            e2.ReplaceComponent(IndexA, new ComponentA());
            e2.RemoveComponent(IndexA);
        }

        [Fact]
        public void WillNotRemoveExternalDelegatesForOnEntityReleased()
        {
            var e = _context.CreateEntity();
            var didRelease = 0;
            e.OnEntityReleased += delegate { didRelease += 1; };
            e.Destroy();
            didRelease.Should().Be(1);
        }

        [Fact]
        public void RemovesAllExternalDelegatesFromOnEntityReleasedAfterBeingDispatched()
        {
            var e = _context.CreateEntity();
            var didRelease = 0;
            e.OnEntityReleased += delegate { didRelease += 1; };
            e.Destroy();
            e.Retain(this);
            e.Release(this);
            didRelease.Should().Be(1);
        }

        [Fact]
        public void RemovesAllExternalDelegatesFromOnEntityReleasedAfterBeingDispatchedWithDelayedRelease()
        {
            var entity = _context.CreateEntity();
            var didRelease = 0;
            entity.OnEntityReleased += delegate { didRelease += 1; };
            entity.Retain(this);
            entity.Destroy();
            didRelease.Should().Be(0);
            entity.Release(this);
            didRelease.Should().Be(1);

            entity.Retain(this);
            entity.Release(this);
            didRelease.Should().Be(1);
        }

        [Fact]
        public void GetsEntityFromObjectPool()
        {
            var e = _context.CreateEntity();
            e.Should().NotBeNull();
            e.GetType().Should().Be(typeof(Entity));
        }

        [Fact]
        public void DestroysEntityWhenPushingBackToObjectPool()
        {
            var e = _context.CreateEntity();
            e.AddComponent(IndexA, new ComponentA());
            e.Destroy();
            e.HasComponent(IndexA).Should().BeFalse();
        }

        [Fact]
        public void ReturnsPushedEntity()
        {
            var e = _context.CreateEntity();
            e.AddComponent(IndexA, new ComponentA());
            e.Destroy();
            var entity = _context.CreateEntity();
            entity.HasComponent(IndexA).Should().BeFalse();
            entity.Should().BeSameAs(e);
        }

        [Fact]
        public void OnlyReturnsReleasedEntities()
        {
            var e1 = _context.CreateEntity();
            e1.Retain(this);
            e1.Destroy();
            var e2 = _context.CreateEntity();
            e2.Should().NotBeSameAs(e1);
            e1.Release(this);
            var e3 = _context.CreateEntity();
            e3.Should().BeSameAs(e1);
        }

        [Fact]
        public void ReturnsNewEntity()
        {
            var e1 = _context.CreateEntity();
            e1.AddComponent(IndexA, new ComponentA());
            e1.Destroy();
            _context.CreateEntity();
            var e2 = _context.CreateEntity();
            e2.HasComponent(IndexA).Should().BeFalse();
            e2.Should().NotBeSameAs(e1);
        }

        [Fact]
        public void SetsUpEntityFromPool()
        {
            var e = _context.CreateEntity();
            var creationIndex = e.creationIndex;
            e.Destroy();
            var group = _context.GetGroup(_matcherA);

            e = _context.CreateEntity();
            e.creationIndex.Should().Be(creationIndex + 1);
            e.isEnabled.Should().BeTrue();

            e.AddComponent(IndexA, new ComponentA());
            group.GetEntities().Should().Contain(e);
        }

        [Fact]
        public void ThrowsWhenAddingComponentToDestroyedEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();

            entity.Invoking(e => e.AddComponent(IndexA, new ComponentA()))
                .Should().Throw<EntityIsNotEnabledException>();
        }

        [Fact]
        public void ThrowsWhenRemovingComponentFromDestroyedEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();

            entity.Invoking(e => e.RemoveComponent(IndexA))
                .Should().Throw<EntityIsNotEnabledException>();
        }

        [Fact]
        public void ThrowsWhenReplacingComponentOnDestroyedEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();

            entity.Invoking(e => e.ReplaceComponent(IndexA, new ComponentA()))
                .Should().Throw<EntityIsNotEnabledException>();
        }

        [Fact]
        public void ThrowsWhenReplacingComponentWithNullOnDestroyedEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();

            entity.Invoking(e => e.ReplaceComponent(IndexA, null))
                .Should().Throw<EntityIsNotEnabledException>();
        }

        [Fact]
        public void ThrowsWhenDestroyComponentToDestroyedEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.Destroy();

            entity.Invoking(e => e.Destroy())
                .Should().Throw<EntityIsNotEnabledException>();
        }

        [Fact]
        public void GetsEmptyGroupForMatcherWhenNoEntitiesWereCreated()
        {
            var group = _context.GetGroup(_matcherA);
            group.Should().NotBeNull();
            group.GetEntities().Should().BeEmpty();
        }

        [Fact]
        public void GetsGroupWithMatchingEntities()
        {
            var entityAb1 = _context.CreateEntity();
            entityAb1.AddComponent(IndexA, new ComponentA());
            entityAb1.AddComponent(IndexB, new ComponentB());

            var entityAb2 = _context.CreateEntity();
            entityAb2.AddComponent(IndexA, new ComponentA());
            entityAb2.AddComponent(IndexB, new ComponentB());

            var entityA = _context.CreateEntity();
            entityA.AddComponent(IndexA, new ComponentA());

            var entities = _context.GetGroup(_matcherAb).GetEntities();
            entities.Length.Should().Be(2);
            entities.Should().Contain(entityAb1);
            entities.Should().Contain(entityAb2);
        }

        [Fact]
        public void GetsCachedGroup()
        {
            _context.GetGroup(_matcherAb).Should().BeSameAs(_context.GetGroup(_matcherAb));
        }

        [Fact]
        public void CachedGroupContainsNewlyCreatedMatchingEntity()
        {
            var entityAb = _context.CreateEntity();
            entityAb.AddComponent(IndexA, new ComponentA());
            var group = _context.GetGroup(_matcherAb);
            entityAb.AddComponent(IndexB, new ComponentB());
            group.GetEntities().Should().Contain(entityAb);
        }

        [Fact]
        public void CachedGroupDoesNotContainEntityWhichAreNotMatchingAnymore()
        {
            var entityAb = _context.CreateEntity();
            entityAb.AddComponent(IndexA, new ComponentA());
            entityAb.AddComponent(IndexB, new ComponentB());
            var group = _context.GetGroup(_matcherAb);
            entityAb.RemoveComponent(IndexA);
            group.GetEntities().Should().NotContain(entityAb);
        }

        [Fact]
        public void RemovesDestroyedEntity()
        {
            var entityAb = _context.CreateEntity();
            entityAb.AddComponent(IndexA, new ComponentA());
            entityAb.AddComponent(IndexB, new ComponentB());
            var group = _context.GetGroup(_matcherAb);
            entityAb.Destroy();
            group.GetEntities().Should().NotContain(entityAb);
        }

        [Fact]
        public void GroupDispatchesOnEntityRemovedAndOnEntityAddedWhenReplacingComponents()
        {
            var entityAb = _context.CreateEntity();
            var componentA1 = new ComponentA();
            var componentA2 = new ComponentA();
            entityAb.AddComponent(IndexA, componentA1);
            entityAb.AddComponent(IndexB, new ComponentB());
            var group = _context.GetGroup(_matcherAb);
            var didDispatchRemoved = 0;
            var didDispatchAdded = 0;
            group.OnEntityRemoved += (g, e, i, c) =>
            {
                g.Should().BeSameAs(group);
                e.Should().BeSameAs(entityAb);
                i.Should().Be(IndexA);
                c.Should().BeSameAs(componentA1);
                didDispatchRemoved++;
            };
            group.OnEntityAdded += (g, e, i, c) =>
            {
                g.Should().BeSameAs(group);
                e.Should().BeSameAs(entityAb);
                i.Should().Be(IndexA);
                c.Should().BeSameAs(componentA2);
                didDispatchAdded++;
            };
            entityAb.ReplaceComponent(IndexA, componentA2);

            didDispatchRemoved.Should().Be(1);
            didDispatchAdded.Should().Be(1);
        }

        [Fact]
        public void GroupDispatchesOnEntityUpdatedWithPreviousAndCurrentComponentWhenReplacingComponent()
        {
            var entityAb = _context.CreateEntity();
            var componentA1 = new ComponentA();
            var componentA2 = new ComponentA();
            entityAb.AddComponent(IndexA, componentA1);
            entityAb.AddComponent(IndexB, new ComponentB());
            var updated = 0;
            var group = _context.GetGroup(_matcherAb);
            group.OnEntityUpdated += (g, e, i, previousComponent, newComponent) =>
            {
                updated += 1;
                g.Should().BeSameAs(group);
                e.Should().BeSameAs(entityAb);
                i.Should().Be(IndexA);
                previousComponent.Should().BeSameAs(componentA1);
                newComponent.Should().BeSameAs(componentA2);
            };

            entityAb.ReplaceComponent(IndexA, componentA2);
            updated.Should().Be(1);
        }

        [Fact]
        public void GroupWithNoneOfMatcherDoesNotDispatchOnEntityAddedWhenDestroyingEntity()
        {
            var entityAb = _context.CreateEntity();
            entityAb.AddComponent(IndexA, new ComponentA());
            entityAb.AddComponent(IndexB, new ComponentB());
            var matcher = Matcher<Entity>.AllOf(IndexB).NoneOf(IndexA);
            var group = _context.GetGroup(matcher);
            group.OnEntityAdded += delegate { throw new Exception(); };
            entityAb.Destroy();
        }

        [Fact]
        public void DispatchesGroupOnEntityAddedEventsAfterAllGroupsAreUpdated()
        {
            var groupAb = _context.GetGroup(_matcherAb);
            var groupB = _context.GetGroup(_matcherB);

            groupAb.OnEntityAdded += delegate { groupB.count.Should().Be(1); };

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexB, new ComponentB());
        }

        [Fact]
        public void DispatchesGroupOnEntityRemovedEventsAfterAllGroupsAreUpdated()
        {
            var groupB = _context.GetGroup(_matcherB);
            var groupAb = _context.GetGroup(_matcherAb);

            groupB.OnEntityRemoved += delegate { groupAb.count.Should().Be(0); };

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexB, new ComponentB());
            entity.RemoveComponent(IndexB);
        }

        [Fact]
        public void ThrowsWhenEntityIndexForKeyDoesNotExist()
        {
            _context.Invoking(context => context.GetEntityIndex("unknown"))
                .Should().Throw<ContextEntityIndexDoesNotExistException>();
        }

        [Fact]
        public void AddsEntityIndex()
        {
            var entityIndex = new PrimaryEntityIndex<Entity, string>(
                "TestIndex",
                _context.GetGroup(_matcherA),
                delegate { return string.Empty; }
            );
            _context.AddEntityIndex(entityIndex);
            _context.GetEntityIndex(entityIndex.name).Should().BeSameAs(entityIndex);
        }

        [Fact]
        public void ThrowsWhenAddingEntityIndexWithSameName()
        {
            var entityIndex = new PrimaryEntityIndex<Entity, string>(
                "TestIndex",
                _context.GetGroup(_matcherA),
                delegate { return string.Empty; }
            );
            _context.AddEntityIndex(entityIndex);
            _context.Invoking(context => context.AddEntityIndex(entityIndex))
                .Should().Throw<ContextEntityIndexDoesAlreadyExistException>();
        }

        [Fact]
        public void ResetsCreationIndex()
        {
            _context.CreateEntity();
            _context.ResetCreationIndex();
            _context.CreateEntity().creationIndex.Should().Be(0);
        }

        [Fact]
        public void RemovesOnEntityCreated()
        {
            _context.OnEntityCreated += delegate { throw new Exception(); };
            _context.RemoveAllEventHandlers();
            _context.CreateEntity();
        }

        [Fact]
        public void RemovesOnEntityWillBeDestroyed()
        {
            _context.OnEntityWillBeDestroyed += delegate { throw new Exception(); };
            _context.RemoveAllEventHandlers();
            _context.CreateEntity().Destroy();
        }

        [Fact]
        public void RemovesOnEntityDestroyed()
        {
            _context.OnEntityDestroyed += delegate { throw new Exception(); };
            _context.RemoveAllEventHandlers();
            _context.CreateEntity().Destroy();
        }

        [Fact]
        public void RemovesOnGroupCreated()
        {
            _context.OnGroupCreated += delegate { throw new Exception(); };
            _context.RemoveAllEventHandlers();
            _context.GetGroup(_matcherA);
        }

        [Fact]
        public void ClearsAllComponentPools()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexB, new ComponentB());
            entity.RemoveComponent(IndexA);
            entity.RemoveComponent(IndexB);

            _context.componentPools[IndexA].Count.Should().Be(1);
            _context.componentPools[IndexB].Count.Should().Be(1);

            _context.ClearComponentPools();

            _context.componentPools[IndexA].Count.Should().Be(0);
            _context.componentPools[IndexB].Count.Should().Be(0);
        }

        [Fact]
        public void ClearsSpecificComponentPool()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexB, new ComponentB());
            entity.RemoveComponent(IndexA);
            entity.RemoveComponent(IndexB);

            _context.ClearComponentPool(IndexB);

            _context.componentPools[IndexA].Count.Should().Be(1);
            _context.componentPools[IndexB].Count.Should().Be(0);
        }

        [Fact]
        public void OnlyClearsExistingComponentPool()
        {
            _context.ClearComponentPool(0);
        }

        [Fact]
        public void PopsNewListFromListPool()
        {
            var groupA = _context.GetGroup(_matcherA);
            var groupAb = _context.GetGroup(Matcher<Entity>.AnyOf(IndexA, IndexB));
            var groupAbc = _context.GetGroup(Matcher<Entity>.AnyOf(0, IndexA, IndexB));

            var didExecute = 0;
            groupA.OnEntityAdded += (_, e, _, _) =>
            {
                didExecute += 1;
                e.RemoveComponent(IndexA);
            };
            groupAb.OnEntityAdded += delegate { didExecute += 1; };
            groupAbc.OnEntityAdded += delegate { didExecute += 1; };

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());

            didExecute.Should().Be(3);
        }

        ContextInfo SetCustomContext()
        {
            var componentNames = new[] {"ComponentA", "ComponentB"};
            var componentTypes = new[] {typeof(ComponentA), typeof(ComponentB)};
            var contextInfo = new ContextInfo("My Context", componentNames, componentTypes);
            _context = new TestContext(componentNames.Length, 0, contextInfo);
            return contextInfo;
        }
    }
}
