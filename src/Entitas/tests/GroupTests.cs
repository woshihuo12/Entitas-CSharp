using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class GroupTests
    {
        readonly IGroup<Entity> _groupA;
        readonly Entity _entityA1;
        readonly Entity _entityA2;

        public GroupTests()
        {
            _groupA = new Group<Entity>(Matcher<Entity>.AllOf(IndexA));
            _entityA1 = CreateEntityA();
            _entityA2 = CreateEntityA();
        }

        [Fact]
        public void DoesNotHaveEntitiesWhichHaveNotBeenAdded()
        {
            _groupA.GetEntities().Should().BeEmpty();
        }

        [Fact]
        public void ClearsBuffer()
        {
            var buffer = new List<Entity> {CreateEntity()};
            var retBuffer = _groupA.GetEntities(buffer);
            buffer.Should().BeEmpty();
            retBuffer.Should().BeSameAs(buffer);
        }

        [Fact]
        public void EmptyGroupIsEmpty()
        {
            _groupA.count.Should().Be(0);
        }

        [Fact]
        public void EmptyGroupDoesNotContainEntity()
        {
            _groupA.ContainsEntity(_entityA1).Should().BeFalse();
        }

        [Fact]
        public void AddsMatchingEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            AssertContains(_entityA1);
        }

        [Fact]
        public void FillsBufferWithEntities()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var buffer = new List<Entity>();
            _groupA.GetEntities(buffer);
            buffer.Count.Should().Be(1);
            buffer[0].Should().BeSameAs(_entityA1);
        }

        [Fact]
        public void ClearsBufferBeforeFilling()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var buffer = new List<Entity> {CreateEntity(), CreateEntity()};
            _groupA.GetEntities(buffer);
            buffer.Count.Should().Be(1);
            buffer[0].Should().BeSameAs(_entityA1);
        }

        [Fact]
        public void DoesNotAddSameEntityTwice()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.HandleEntitySilently(_entityA1);
            AssertContains(_entityA1);
        }

        [Fact]
        public void EnumeratesGroup()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var i = 0;
            Entity e = null;
            foreach (var entity in _groupA)
            {
                i++;
                e = entity;
            }

            i.Should().Be(1);
            e.Should().BeSameAs(_entityA1);
        }

        [Fact]
        public void ReturnsEnumerable()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.AsEnumerable().Single().Should().BeSameAs(_entityA1);
        }

        [Fact]
        public void RemovesEntityWhenEntityDoesNotMatchAnymore()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            AssertEmpty(_entityA1);
        }

        [Fact]
        public void DoesNotAddEntityWhenEntityIsNotEnabled()
        {
            _entityA1.InternalDestroy();
            _groupA.HandleEntitySilently(_entityA1);
            AssertEmpty(_entityA1);
        }

        [Fact]
        public void DoesNotAddEntityWhenNotMatching()
        {
            var entity = CreateEntity();
            entity.AddComponent(IndexB, new ComponentB());
            _groupA.HandleEntitySilently(entity);
            AssertEmpty(entity);
        }

        [Fact]
        public void GetsNullWhenSingleEntityDoesNotExist()
        {
            _groupA.GetSingleEntity().Should().BeNull();
        }

        [Fact]
        public void GetsSingleEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetSingleEntity().Should().BeSameAs(_entityA1);
        }

        [Fact]
        public void ThrowsWhenGettingSingleEntityWhenMultipleMatchingEntitiesExist()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.HandleEntitySilently(_entityA2);
            _groupA.Invoking(group => { group.GetSingleEntity(); })
                .Should().Throw<GroupSingleEntityException<Entity>>();
        }

        [Fact]
        public void DispatchesOnEntityAddedWhenMatchingEntityAdded()
        {
            var didDispatch = 0;
            _groupA.OnEntityAdded += (group, entity, index, component) =>
            {
                didDispatch++;
                group.Should().BeSameAs(_groupA);
                entity.Should().BeSameAs(_entityA1);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(_entityA1.GetComponent(IndexA));
            };
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };

            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DoesNotDispatchOnEntityAddedWhenMatchingEntityAlreadyHasBeenAdded()
        {
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            _groupA.OnEntityAdded += delegate { throw new Exception(); };
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
        }

        [Fact]
        public void DoesNotDispatchOnEntityAddedWhenEntityIsNotMatching()
        {
            var entity = CreateEntity();
            entity.AddComponent(IndexB, new ComponentB());
            _groupA.OnEntityAdded += delegate { throw new Exception(); };
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };
            _groupA.HandleEntity(entity, IndexB, entity.GetComponent(IndexB));
        }

        [Fact]
        public void DispatchesOnEntityRemovedWhenEntityGotRemoved()
        {
            var didDispatch = 0;
            var removedComponent = _entityA1.GetComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.OnEntityRemoved += (group, entity, index, component) =>
            {
                didDispatch++;
                group.Should().BeSameAs(_groupA);
                entity.Should().BeSameAs(_entityA1);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(removedComponent);
            };
            _groupA.OnEntityAdded += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };

            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, removedComponent);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DoesNotDispatchOnEntityRemovedWhenEntityDidNotGetRemoved()
        {
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            var removedComponent = _entityA1.GetComponent(IndexA);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, removedComponent);
        }

        [Fact]
        public void DispatchesOnEntityRemovedAndOnEntityAddedAndOnEntityUpdatedWhenUpdating()
        {
            _groupA.HandleEntitySilently(_entityA1);

            var removed = 0;
            var added = 0;
            var updated = 0;
            var oldComponentA = _entityA1.GetComponent(IndexA);
            var newComponentA = new ComponentA();

            _groupA.OnEntityRemoved += (group, entity, index, component) =>
            {
                removed += 1;
                group.Should().Be(_groupA);
                entity.Should().Be(_entityA1);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(oldComponentA);
            };
            _groupA.OnEntityAdded += (group, entity, index, component) =>
            {
                added += 1;
                group.Should().Be(_groupA);
                entity.Should().Be(_entityA1);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(newComponentA);
            };
            _groupA.OnEntityUpdated += (group, entity, index, previousComponent, newComponent) =>
            {
                updated += 1;
                group.Should().Be(_groupA);
                entity.Should().Be(_entityA1);
                index.Should().Be(IndexA);
                previousComponent.Should().BeSameAs(oldComponentA);
                newComponent.Should().BeSameAs(newComponentA);
            };

            _groupA.UpdateEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA), newComponentA);

            removed.Should().Be(1);
            added.Should().Be(1);
            updated.Should().Be(1);
        }

        [Fact]
        public void DoesNotDispatchOnEntityRemovedAndOnEntityAddedWhenUpdatingWhenGroupDoesNotContainEntity()
        {
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            _groupA.OnEntityAdded += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };
            IComponent component = new ComponentA();
            _groupA.UpdateEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA), component);
        }

        [Fact]
        public void RemovesAllEventHandlers()
        {
            _groupA.OnEntityAdded += delegate { throw new Exception(); };
            _groupA.OnEntityRemoved += delegate { throw new Exception(); };
            _groupA.OnEntityUpdated += delegate { throw new Exception(); };

            _groupA.RemoveAllEventHandlers();

            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));

            var component = _entityA1.GetComponent(IndexA);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, component);

            component = new ComponentA();
            _entityA1.AddComponent(IndexA, component);
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            _groupA.UpdateEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA), component);
        }

        [Fact]
        public void GetsCachedEntities()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetEntities().Should().BeSameAs(_groupA.GetEntities());
        }

        [Fact]
        public void UpdatesCacheWhenAddingNewMatchingEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetEntities();
            _groupA.HandleEntitySilently(_entityA2);
            _groupA.GetEntities().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateCacheWhenAddingNonMatchingEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetEntities();
            var entity = CreateEntity();
            _groupA.HandleEntitySilently(entity);
            _groupA.GetEntities().Should().BeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenRemovingEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetEntities();
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetEntities().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateCacheWhenRemovingEntityThatHasNotBeenAdded()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetEntities();
            _entityA2.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA2);
            _groupA.GetEntities().Should().BeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateCacheWhenUpdatingEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetEntities();
            IComponent component = new ComponentA();
            _groupA.UpdateEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA), component);
            _groupA.GetEntities().Should().BeSameAs(cache);
        }

        [Fact]
        public void GetsCachedSingleEntities()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetSingleEntity();
            _groupA.GetSingleEntity().Should().BeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenNewSingleEntityWasAdded()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetSingleEntity();
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.HandleEntitySilently(_entityA2);
            _groupA.GetSingleEntity().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenSingleEntityIsRemoved()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetSingleEntity();
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetSingleEntity().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateCacheWhenSingleEntityIsUpdated()
        {
            _groupA.HandleEntitySilently(_entityA1);
            var cache = _groupA.GetSingleEntity();
            IComponent component = new ComponentA();
            _groupA.UpdateEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA), component);
            _groupA.GetSingleEntity().Should().BeSameAs(cache);
        }

        [Fact]
        public void RetainsMatchedEntity()
        {
            _entityA1.retainCount.Should().Be(0);
            _groupA.HandleEntitySilently(_entityA1);
            _entityA1.retainCount.Should().Be(1);
        }

        [Fact]
        public void ReleasesRemovedEntity()
        {
            _groupA.HandleEntitySilently(_entityA1);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            _entityA1.retainCount.Should().Be(0);
        }

        [Fact]
        public void InvalidatesEntitiesCacheSilent()
        {
            var didExecute = 0;
            _entityA1.OnEntityReleased += delegate
            {
                didExecute += 1;
                _groupA.GetEntities().Length.Should().Be(0);
            };
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetEntities();
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            didExecute.Should().Be(1);
        }

        [Fact]
        public void InvalidatesEntitiesCache()
        {
            var didExecute = 0;
            _entityA1.OnEntityReleased += delegate
            {
                didExecute += 1;
                _groupA.GetEntities().Length.Should().Be(0);
            };
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            _groupA.GetEntities();
            var component = _entityA1.GetComponent(IndexA);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, component);
            didExecute.Should().Be(1);
        }

        [Fact]
        public void InvalidatesSingleEntityCacheSilent()
        {
            var didExecute = 0;
            _entityA1.OnEntityReleased += delegate
            {
                didExecute += 1;
                _groupA.GetSingleEntity().Should().BeNull();
            };
            _groupA.HandleEntitySilently(_entityA1);
            _groupA.GetSingleEntity();
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntitySilently(_entityA1);
            didExecute.Should().Be(1);
        }

        [Fact]
        public void InvalidatesSingleEntityCache()
        {
            var didExecute = 0;
            _entityA1.OnEntityReleased += delegate
            {
                didExecute += 1;
                _groupA.GetSingleEntity().Should().BeNull();
            };
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            _groupA.GetSingleEntity();
            var component = _entityA1.GetComponent(IndexA);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, component);
            didExecute.Should().Be(1);
        }

        [Fact]
        public void RetainsEntityUntilAfterEventHandlersWereCalled()
        {
            _groupA.HandleEntity(_entityA1, IndexA, _entityA1.GetComponent(IndexA));
            var didDispatch = 0;
            _groupA.OnEntityRemoved += (_, entity, _, _) =>
            {
                didDispatch += 1;
                entity.retainCount.Should().Be(1);
            };
            var component = _entityA1.GetComponent(IndexA);
            _entityA1.RemoveComponent(IndexA);
            _groupA.HandleEntity(_entityA1, IndexA, component);
            didDispatch.Should().Be(1);
            _entityA1.retainCount.Should().Be(0);
        }

        [Fact]
        public void CanToString()
        {
            var matcher = Matcher<Entity>.AllOf(Matcher<Entity>.AllOf(0), Matcher<Entity>.AllOf(1));
            var group = new Group<Entity>(matcher);
            group.ToString().Should().Be("Group(AllOf(0, 1))");
        }

        void AssertContains(params Entity[] expectedEntities)
        {
            _groupA.count.Should().Be(expectedEntities.Length);

            var entities = _groupA.GetEntities();
            entities.Length.Should().Be(expectedEntities.Length);

            foreach (var e in expectedEntities)
            {
                entities.Should().Contain(e);
                _groupA.ContainsEntity(e).Should().BeTrue();
            }
        }

        void AssertEmpty(Entity entity)
        {
            _groupA.count.Should().Be(0);
            _groupA.GetEntities().Should().BeEmpty();
            _groupA.ContainsEntity(entity).Should().BeFalse();
        }
    }
}
