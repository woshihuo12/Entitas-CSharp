using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class EntityTests
    {
        IEntity _entity;

        public EntityTests() => _entity = CreateEntity();

        [Fact]
        public void HasDefaultContextInfo()
        {
            _entity.contextInfo.name.Should().Be("No Context");
            _entity.contextInfo.componentNames.Length.Should().Be(TotalComponents);
            _entity.contextInfo.componentTypes.Should().BeNull();
            for (var i = 0; i < _entity.contextInfo.componentNames.Length; i++)
                _entity.contextInfo.componentNames[i].Should().Be(i.ToString());
        }

        [Fact]
        public void InitializesEnity()
        {
            var contextInfo = new ContextInfo(null, null, null);
            var componentPools = new Stack<IComponent>[42];
            _entity = new Entity();
            _entity.Initialize(1, TotalComponents, componentPools, contextInfo);

            _entity.isEnabled.Should().BeTrue();
            _entity.creationIndex.Should().Be(1);
            _entity.totalComponents.Should().Be(TotalComponents);
            _entity.componentPools.Should().BeSameAs(componentPools);
            _entity.contextInfo.Should().BeSameAs(contextInfo);
        }

        [Fact]
        public void ReactivatesEntityAfterBeingDesroyed()
        {
            var contextInfo = new ContextInfo(null, null, null);
            var componentPools = new Stack<IComponent>[42];
            _entity = new Entity();
            _entity.Initialize(1, TotalComponents, componentPools, contextInfo);

            _entity.InternalDestroy();
            _entity.Reactivate(42);

            _entity.isEnabled.Should().BeTrue();
            _entity.creationIndex.Should().Be(42);
            _entity.totalComponents.Should().Be(TotalComponents);
            _entity.componentPools.Should().BeSameAs(componentPools);
            _entity.contextInfo.Should().BeSameAs(contextInfo);
        }

        [Fact]
        public void ThrowsWhenGettingComponentAtIndexWhichHasNotBeenAdded()
        {
            _entity.Invoking(e => e.GetComponent(IndexA))
                .Should().Throw<EntityDoesNotHaveComponentException>();
        }

        [Fact]
        public void GetsTotalComponentsCountWhenEmpty()
        {
            _entity.totalComponents.Should().Be(TotalComponents);
        }

        [Fact]
        public void GetsEmptyArrayOfComponentsWhenNoComponentsWereAdded()
        {
            _entity.GetComponents().Should().BeEmpty();
        }

        [Fact]
        public void GetsEmptyArrayOfComponentIndexesWhenNoComponentsWereAdded()
        {
            _entity.GetComponentIndices().Should().BeEmpty();
        }

        [Fact]
        public void DoesNotHaveComponentAtIndexWhenNoComponentWasAdded()
        {
            _entity.HasComponent(IndexA).Should().BeFalse();
        }

        [Fact]
        public void DoeNotHaveComponentsAtIndexesWhenNoComponentsWereAdded()
        {
            _entity.HasComponents(IndexesA).Should().BeFalse();
        }

        [Fact]
        public void AddsComponent()
        {
            var componentA = new ComponentA();
            _entity.AddComponent(IndexA, componentA);
            AssertHasComponentA(_entity, componentA);
        }

        [Fact]
        public void ThrowsWhenAttemptingToRemoveComponentAtIndexWhichHasNotBeenAdded()
        {
            _entity.Invoking(e => e.RemoveComponent(IndexA))
                .Should().Throw<EntityDoesNotHaveComponentException>();
        }

        [Fact]
        public void ReplacingNonExistingComponentAddsComponent()
        {
            var componentA = new ComponentA();
            _entity.ReplaceComponent(IndexA, componentA);
            AssertHasComponentA(_entity, componentA);
        }

        [Fact]
        public void ThrowsWhenAddingComponentAtTheSameIndexTwice()
        {
            CreateEntityA().Invoking(e => e.AddComponent(IndexA, new ComponentA()))
                .Should().Throw<EntityAlreadyHasComponentException>();
        }

        [Fact]
        public void RemovesComponentAtIndex()
        {
            var entity = CreateEntityA();
            entity.RemoveComponent(IndexA);
            AssertHasNotComponentA(entity);
        }

        [Fact]
        public void ReplacesExistingComponent()
        {
            var entity = CreateEntityA();
            var newComponentA = new ComponentA();
            entity.ReplaceComponent(IndexA, newComponentA);
            AssertHasComponentA(entity, newComponentA);
        }

        [Fact]
        public void DoesNotHaveComponentsAtIndexesWhenNotAllComponentsWereAdded()
        {
            CreateEntityA().HasComponents(IndexesAb).Should().BeFalse();
        }

        [Fact]
        public void HasAnyComponentsAtIndexesWhenAnyComponentWasAdded()
        {
            CreateEntityA().HasAnyComponent(IndexesAb).Should().BeTrue();
        }

        [Fact]
        public void HasOtherComponent()
        {
            CreateEntityAb().HasComponent(IndexB).Should().BeTrue();
        }

        [Fact]
        public void GetsAllComponents()
        {
            var entity = CreateEntityAb();
            var components = entity.GetComponents();
            components.Length.Should().Be(2);
            components.Should().Contain(entity.GetComponent(IndexA));
            components.Should().Contain(entity.GetComponent(IndexB));
        }

        [Fact]
        public void GetsAllComponentIndexes()
        {
            var entity = CreateEntityAb();
            var indexes = entity.GetComponentIndices();
            indexes.Length.Should().Be(2);
            indexes.Should().Contain(IndexA);
            indexes.Should().Contain(IndexB);
        }

        [Fact]
        public void HasComponentsAtIndexesWhenAllComponentsWereAdded()
        {
            CreateEntityAb().HasComponents(IndexesAb).Should().BeTrue();
        }

        [Fact]
        public void RemovesAllComponents()
        {
            var entity = CreateEntityAb();
            entity.RemoveAllComponents();
            entity.HasComponent(IndexA).Should().BeFalse();
            entity.HasComponent(IndexB).Should().BeFalse();
            entity.GetComponents().Should().BeEmpty();
            entity.GetComponentIndices().Should().BeEmpty();
        }

        [Fact]
        public void CanToStringAndDoesNotRemoveComponentSuffix()
        {
            var entity = CreateEntityAb();
            entity.AddComponent(0, new TestComponent());
            entity.ToString().Should().Be("Entity_0(TestComponent, ComponentA, ComponentB)");
        }

        [Fact]
        public void UsesComponentToString()
        {
            var entity = CreateEntityAb();
            entity.AddComponent(0, new NameAgeComponent {Name = "Test", Age = 42});
            entity.ToString().Should().Be("Entity_0(NameAge(Test, 42), ComponentA, ComponentB)");
        }

        [Fact]
        public void UsesFullComponentNameWithNamespaceIfToStringIsNotImplemented()
        {
            var entity = CreateEntityAb();
            entity.AddComponent(0, new MyNamespaceComponent());
            entity.ToString().Should().Be("Entity_0(Entitas.Tests.MyNamespaceComponent, ComponentA, ComponentB)");
        }

        [Fact]
        public void GetsComponentPool()
        {
            _entity.GetComponentPool(IndexA).Count.Should().Be(0);
        }

        [Fact]
        public void GetsSameComponentPoolInstance()
        {
            _entity.GetComponentPool(IndexA).Should().BeSameAs(_entity.GetComponentPool(IndexA));
        }

        [Fact]
        public void PushesComponentToComponentPoolWhenRemoved()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            var component = _entity.GetComponent(IndexA);
            _entity.RemoveComponent(IndexA);

            var componentPool = _entity.GetComponentPool(IndexA);
            componentPool.Count.Should().Be(1);
            componentPool.Pop().Should().BeSameAs(component);
        }

        [Fact]
        public void CreatesNewComponentWhenComponentPoolIsEmpty()
        {
            var type = typeof(NameAgeComponent);
            var component = _entity.CreateComponent(0, type);
            component.GetType().Should().Be(type);

            var nameAgeComponent = ((NameAgeComponent)component);
            nameAgeComponent.Name.Should().BeNull();
            nameAgeComponent.Age.Should().Be(0);
        }

        [Fact]
        public void GetsPooledComponentWhenComponentPoolIsNotEmpty()
        {
            var component = new NameAgeComponent();
            _entity.AddComponent(0, component);
            _entity.RemoveComponent(0);

            var newComponent = (NameAgeComponent)_entity.CreateComponent(0, typeof(NameAgeComponent));
            newComponent.Should().BeSameAs(component);
        }

        [Fact]
        public void DispatchesOnComponentAddedWhenAddingComponent()
        {
            var componentA = new ComponentA();
            var didDispatch = 0;
            _entity.OnComponentAdded += (entity, index, component) =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(componentA);
            };
            _entity.OnComponentRemoved += delegate { throw new Exception(); };
            _entity.OnComponentReplaced += delegate { throw new Exception(); };
            _entity.AddComponent(IndexA, componentA);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DispatchesOnComponentRemovedWhenRemovingComponent()
        {
            var componentA = new ComponentA();
            _entity.AddComponent(IndexA, componentA);
            var didDispatch = 0;
            _entity.OnComponentRemoved += (entity, index, component) =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(componentA);
            };
            _entity.OnComponentAdded += delegate { throw new Exception(); };
            _entity.OnComponentReplaced += delegate { throw new Exception(); };
            _entity.RemoveComponent(IndexA);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DispatchesOnComponentRemovedBeforePushingComponentToPool()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            _entity.OnComponentRemoved += (entity, index, component) => { component.Should().NotBeSameAs(entity.CreateComponent(index, component.GetType())); };
            _entity.RemoveComponent(IndexA);
        }

        [Fact]
        public void DispatchesOnComponentReplacedWhenReplacingComponent()
        {
            var componentA = new ComponentA();
            _entity.AddComponent(IndexA, componentA);
            var newComponentA = new ComponentA();
            var didDispatch = 0;
            _entity.OnComponentReplaced += (entity, index, previousComponent, newComponent) =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
                index.Should().Be(IndexA);
                previousComponent.Should().BeSameAs(componentA);
                newComponent.Should().BeSameAs(newComponentA);
            };
            _entity.OnComponentAdded += delegate { throw new Exception(); };
            _entity.OnComponentRemoved += delegate { throw new Exception(); };

            _entity.ReplaceComponent(IndexA, newComponentA);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void ProvidesPreviousAndNewComponentOnComponentReplacedWhenReplacingWithSameComponent()
        {
            var componentA = new ComponentA();
            var didDispatch = 0;
            _entity.OnComponentReplaced += (entity, _, previousComponent, newComponent) =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
                previousComponent.Should().BeSameAs(componentA);
                newComponent.Should().BeSameAs(componentA);
            };

            _entity.AddComponent(IndexA, componentA);
            _entity.ReplaceComponent(IndexA, componentA);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DoesNotDispatchAnythingWhenReplacingNonExistingComponentWithNull()
        {
            _entity.OnComponentAdded += delegate { throw new Exception(); };
            _entity.OnComponentReplaced += delegate { throw new Exception(); };
            _entity.OnComponentRemoved += delegate { throw new Exception(); };
            _entity.ReplaceComponent(IndexA, null);
        }

        [Fact]
        public void DispatchesOnComponentAddedWhenAttemptingToReplaceComponentWhichHasNotBeenAdded()
        {
            var componentA = new ComponentA();
            var didDispatch = 0;
            _entity.OnComponentAdded += (entity, index, component) =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
                index.Should().Be(IndexA);
                component.Should().BeSameAs(componentA);
            };
            _entity.OnComponentReplaced += delegate { throw new Exception(); };
            _entity.OnComponentRemoved += delegate { throw new Exception(); };

            _entity.ReplaceComponent(IndexA, componentA);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DispatchesOnComponentRemovedWhenReplacingComponentWithNull()
        {
            var componentA = new ComponentA();
            _entity.AddComponent(IndexA, componentA);
            var didDispatch = 0;
            _entity.OnComponentRemoved += (_, _, component) =>
            {
                didDispatch += 1;
                component.Should().BeSameAs(componentA);
            };
            _entity.OnComponentAdded += delegate { throw new Exception(); };
            _entity.OnComponentReplaced += delegate { throw new Exception(); };

            _entity.ReplaceComponent(IndexA, null);
            didDispatch.Should().Be(1);
        }

        [Fact]
        public void DispatchesOnComponentRemovedWhenRemovingAllComponents()
        {
            var didDispatch = 0;
            _entity.AddComponent(IndexA, new ComponentA());
            _entity.AddComponent(IndexB, new ComponentB());
            _entity.OnComponentRemoved += delegate { didDispatch += 1; };
            _entity.RemoveAllComponents();
            didDispatch.Should().Be(2);
        }

        [Fact]
        public void DispatchesOnDestroyWhenCallingDestroy()
        {
            var didDestroy = 0;
            _entity.OnDestroyEntity += delegate { didDestroy += 1; };
            _entity.Destroy();
            didDestroy.Should().Be(1);
        }

        [Fact]
        public void RetainsEntity()
        {
            _entity.retainCount.Should().Be(0);
            _entity.Retain(this);
            _entity.retainCount.Should().Be(1);
            if (_entity.aerc is SafeAERC safeAerc)
                safeAerc.owners.Should().Contain(this);
        }

        [Fact]
        public void ReleasesEntity()
        {
            _entity.Retain(this);
            _entity.Release(this);
            _entity.retainCount.Should().Be(0);
            if (_entity.aerc is SafeAERC safeAerc)
                safeAerc.owners.Should().NotContain(this);
        }

        [Fact]
        public void ThrowsWhenReleasingMoreThanItHasBeenRetained()
        {
            _entity.Invoking(e =>
            {
                e.Retain(this);
                e.Release(this);
                e.Release(this);
            }).Should().Throw<EntityIsNotRetainedByOwnerException>();
        }

        [Fact]
        public void ThrowsWhenRetainingTwiceWithSameOwner()
        {
            var owner = new object();
            _entity.Invoking(e =>
            {
                e.Retain(owner);
                e.Retain(owner);
            }).Should().Throw<EntityIsAlreadyRetainedByOwnerException>();
        }

        [Fact]
        public void ThrowsWhenReleasingWithUnknownOwner()
        {
            var owner = new object();
            var unknownOwner = new object();
            _entity.Invoking(e =>
            {
                e.Retain(owner);
                e.Release(unknownOwner);
            }).Should().Throw<EntityIsNotRetainedByOwnerException>();
        }

        [Fact]
        public void ThrowsWhenReleasingWithOwnerWhichDoesNotRetainEntityAnymore()
        {
            var owner1 = new object();
            var owner2 = new object();
            _entity.Invoking(e =>
            {
                e.Retain(owner1);
                e.Retain(owner2);
                e.Release(owner2);
                e.Release(owner2);
            }).Should().Throw<EntityIsNotRetainedByOwnerException>();
        }

        [Fact]
        public void DoesNotDispatchOnEntityReleasedWhenRetaining()
        {
            _entity.OnEntityReleased += delegate { throw new Exception(); };
            _entity.Retain(this);
        }

        [Fact]
        public void DispatchesOnEntityReleasedWhenRetainAndRelease()
        {
            var didDispatch = 0;
            _entity.OnEntityReleased += entity =>
            {
                didDispatch += 1;
                entity.Should().BeSameAs(_entity);
            };

            _entity.Retain(this);
            _entity.Release(this);

            didDispatch.Should().Be(1);
        }

        [Fact]
        public void CachesComponents()
        {
            var entity = CreateEntityA();
            entity.GetComponents().Should().BeSameAs(entity.GetComponents());
        }

        [Fact]
        public void UpdatesCacheWhenNewComponentWasAdded()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponents();
            entity.AddComponent(IndexB, new ComponentB());
            entity.GetComponents().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenComponentWasRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponents();
            entity.RemoveComponent(IndexA);
            entity.GetComponents().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenComponentWasReplaced()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponents();
            entity.ReplaceComponent(IndexA, new ComponentA());
            entity.GetComponents().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateCacheWhenComponentWasReplacedWithSameComponent()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponents();
            entity.ReplaceComponent(IndexA, entity.GetComponent(IndexA));
            entity.GetComponents().Should().BeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenAllComponentsWereRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponents();
            entity.RemoveAllComponents();
            entity.GetComponents().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void CachesComponentIndexes()
        {
            var entity = CreateEntityA();
            entity.GetComponentIndices().Should().BeSameAs(entity.GetComponentIndices());
        }

        [Fact]
        public void UpdatesIndexCacheWhenNewComponentWasAdded()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponentIndices();
            entity.AddComponent(IndexB, new ComponentB());
            entity.GetComponentIndices().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesIndexCacheWhenComponentWasRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponentIndices();
            entity.RemoveComponent(IndexA);
            entity.GetComponentIndices().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateIndexCacheWhenComponentWasReplaced()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponentIndices();
            entity.ReplaceComponent(IndexA, new ComponentA());
            entity.GetComponentIndices().Should().BeSameAs(cache);
        }

        [Fact]
        public void UpdatesIndexCacheWhenAddingNewComponentWithReplaceComponent()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponentIndices();
            entity.ReplaceComponent(0, new NameAgeComponent());
            entity.GetComponentIndices().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesIndexCacheWhenAllComponentsWereRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.GetComponentIndices();
            entity.RemoveAllComponents();
            entity.GetComponentIndices().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void CachesEntityDescription()
        {
            var entity = CreateEntityA();
            entity.ToString().Should().BeSameAs(entity.ToString());
        }

        [Fact]
        public void UpdatesStringCacheWhenNewComponentWasAdded()
        {
            var entity = CreateEntityA();
            var cache = entity.ToString();
            entity.AddComponent(IndexB, new ComponentB());
            entity.ToString().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesStringCacheWhenComponentWasRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.ToString();
            entity.RemoveComponent(IndexA);
            entity.ToString().Should().NotBeSameAs(cache);
        }

        // TODO Should cache be invalidated in case component has custom ToString()?
        [Fact]
        public void DoesNotUpdateStringCacheWhenComponentWasReplaced()
        {
            var entity = CreateEntityA();
            var cache = entity.ToString();
            entity.ReplaceComponent(IndexA, new ComponentA());
            entity.ToString().Should().BeSameAs(cache);
        }

        [Fact]
        public void UpdatesStringCacheWhenAllComponentsWereRemoved()
        {
            var entity = CreateEntityA();
            var cache = entity.ToString();
            entity.RemoveAllComponents();
            entity.ToString().Should().NotBeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateStringCacheWhenEntityGetsRetained()
        {
            var entity = CreateEntityA();
            var cache = entity.ToString();
            entity.Retain(this);
            entity.ToString().Should().BeSameAs(cache);
        }

        [Fact]
        public void DoesNotUpdateStringCacheWhenEntityGetsReleased()
        {
            var entity = CreateEntityA();
            entity.Retain(this);
            entity.Retain(new object());
            var cache = entity.ToString();
            entity.Release(this);
            entity.ToString().Should().BeSameAs(cache);
        }

        [Fact]
        public void ReleasedEntityDoesNotHaveUpdatedStringCache()
        {
            var entity = CreateEntityA();
            entity.Retain(this);
            var cache = entity.ToString();
            entity.OnEntityReleased += e => { e.ToString().Should().BeSameAs(cache); };
            entity.Release(this);
        }

        [Fact]
        public void UpdatesStringCacheWhenRemoveAllComponentsIsCalledEvenIfEntityHasNoComponents()
        {
            var cache = _entity.ToString();
            _entity.RemoveAllComponents();
            _entity.ToString().Should().NotBeSameAs(cache);
        }

        static void AssertHasComponentA(IEntity entity, IComponent component)
        {
            entity.GetComponent(IndexA).Should().BeSameAs(component);

            var components = entity.GetComponents();
            components.Length.Should().Be(1);
            components.Should().Contain(component);

            var indices = entity.GetComponentIndices();
            indices.Length.Should().Be(1);
            indices.Should().Contain(IndexA);

            entity.HasComponent(IndexA).Should().BeTrue();
            entity.HasComponents(IndexesA).Should().BeTrue();
            entity.HasAnyComponent(IndexesA).Should().BeTrue();
        }

        static void AssertHasNotComponentA(IEntity entity)
        {
            var components = entity.GetComponents();
            components.Length.Should().Be(0);

            var indices = entity.GetComponentIndices();
            indices.Length.Should().Be(0);

            entity.HasComponent(IndexA).Should().BeFalse();
            entity.HasComponents(IndexesA).Should().BeFalse();
            entity.HasAnyComponent(IndexesA).Should().BeFalse();
        }
    }
}
