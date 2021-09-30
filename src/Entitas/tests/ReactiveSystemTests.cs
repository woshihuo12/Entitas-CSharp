using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Systems;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class ReactiveSystemTests
    {
        readonly IMatcher<Entity> _matcherAb = Matcher<Entity>.AllOf(IndexA, IndexB);

        readonly TestContext _context = new();
        readonly ReactiveSystemSpy _systemAdded;
        readonly ReactiveSystemSpy _systemRemoved;

        public ReactiveSystemTests()
        {
            _systemAdded = new ReactiveSystemSpy(_context.CreateCollector(_matcherAb.Added()));
            _systemRemoved = new ReactiveSystemSpy(_context.CreateCollector(_matcherAb.Removed()));
        }

        [Fact]
        public void DoesNotExecuteWhenNoEntitiesWereCollected()
        {
            _systemAdded.Execute();
            AssertEntities(_systemAdded, null);
        }

        [Fact]
        public void ExecutesWhenAdded()
        {
            var entity = CreateEntityAb();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity);
        }

        [Fact]
        public void ExecutesOnlyOnceWhenAdded()
        {
            var entity = CreateEntityAb();
            _systemAdded.Execute();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity);
        }

        [Fact]
        public void RetainsAndReleasesCollectedEntities()
        {
            var entity = CreateEntityAb();
            entity.retainCount.Should().Be(3); // retained by context, group and collector
            _systemAdded.Execute();
            entity.retainCount.Should().Be(2); // retained by context and group
        }

        [Fact]
        public void CollectsEntitiesChangedBySystem()
        {
            var entity = CreateEntityAb();
            _systemAdded.ExecuteAction = entities => { entities[0].ReplaceComponent(IndexA, new ComponentA()); };
            _systemAdded.Execute();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity, 2);
        }

        [Fact]
        public void CollectsCreatedEntitiesInExecute()
        {
            var entity1 = CreateEntityAb();
            Entity entity2 = null;
            _systemAdded.ExecuteAction = delegate { entity2 ??= CreateEntityAb(); };
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity1);
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity2, 2);
        }

        [Fact]
        public void DoesNotExecuteWhenNotAdded()
        {
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systemAdded.Execute();
            AssertEntities(_systemAdded, null);
        }

        [Fact]
        public void Deactivates()
        {
            _systemAdded.Deactivate();
            CreateEntityAb();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, null);
        }

        [Fact]
        public void Activates()
        {
            _systemAdded.Deactivate();
            _systemAdded.Activate();
            var entity = CreateEntityAb();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, entity);
        }

        [Fact]
        public void Clears()
        {
            CreateEntityAb();
            _systemAdded.Clear();
            _systemAdded.Execute();
            AssertEntities(_systemAdded, null);
        }

        [Fact]
        public void ReactiveSystemToString()
        {
            _systemAdded.ToString().Should().Be("ReactiveSystem(ReactiveSystemSpy)");
        }

        [Fact]
        public void ExecutesWhenRemoved()
        {
            var entity = CreateEntityAb();
            _systemRemoved.Execute();
            AssertEntities(_systemRemoved, null);
            entity.RemoveComponent(IndexA);
            _systemRemoved.Execute();
            AssertEntities(_systemRemoved, entity);
        }

        [Fact]
        public void ExecutesOnlyOnceWhenRemoved()
        {
            var entity = CreateEntityAb();
            entity.RemoveComponent(IndexA);
            _systemRemoved.Execute();
            _systemRemoved.Execute();
            AssertEntities(_systemRemoved, entity);
        }

        [Fact]
        public void DoesNotExecuteWhenNotRemoved()
        {
            var entity = CreateEntityAb();
            entity.AddComponent(IndexC, new ComponentC());
            entity.RemoveComponent(IndexC);
            _systemRemoved.Execute();
            AssertEntities(_systemRemoved, null);
        }

        [Fact]
        public void RetainsEntitiesUntilExecuteCompleted()
        {
            _systemAdded.Deactivate();
            var entity = CreateEntityAb();
            var didExecute = 0;
            _systemRemoved.ExecuteAction = entities =>
            {
                didExecute += 1;
                entities[0].retainCount.Should().Be(1);
            };

            entity.Destroy();
            _systemRemoved.Execute();
            didExecute.Should().Be(1);
            entity.retainCount.Should().Be(0);
        }

        [Fact]
        public void ExecutesWhenAddedOrRemoved()
        {
            var system = new ReactiveSystemSpy(_context.CreateCollector(_matcherAb.AddedOrRemoved()));
            var entity = CreateEntityAb();
            system.Execute();
            AssertEntities(system, entity);
            entity.RemoveComponent(IndexA);
            system.Execute();
            AssertEntities(system, entity, 2);
        }

        [Fact]
        public void ExecutesWhenTriggeredByCollector()
        {
            var context1 = new TestContext();
            var context2 = new TestContext();
            var system = new ReactiveSystemSpy(new Collector<Entity>(
                new[]
                {
                    context1.GetGroup(Matcher<Entity>.AllOf(IndexA)),
                    context2.GetGroup(Matcher<Entity>.AllOf(IndexB))
                },
                new[]
                {
                    GroupEvent.Added,
                    GroupEvent.Removed
                })
            );

            var entityA = context1.CreateEntity();
            entityA.AddComponent(IndexA, new ComponentA());
            context2.CreateEntity().AddComponent(IndexA, new ComponentA());

            var entityB1 = context1.CreateEntity();
            entityB1.AddComponent(IndexB, new ComponentB());
            var entityB2 = context2.CreateEntity();
            entityB2.AddComponent(IndexB, new ComponentB());

            system.Execute();
            AssertEntities(system, entityA);
            entityB1.RemoveComponent(IndexB);
            entityB2.RemoveComponent(IndexB);
            system.Execute();
            AssertEntities(system, entityB2, 2);
        }

        [Fact]
        public void FiltersEntities()
        {
            _systemAdded.Deactivate();
            _systemRemoved.Deactivate();
            var system = new ReactiveSystemSpy(_context.CreateCollector(_matcherAb),
                e => ((NameAgeComponent)e.GetComponent(IndexA)).Age > 42);

            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexC, new ComponentC());

            var entityAb1 = _context.CreateEntity();
            entityAb1.AddComponent(IndexA, new NameAgeComponent {Age = 10});
            entityAb1.AddComponent(IndexB, new ComponentB());

            var entityAb2 = _context.CreateEntity();
            entityAb2.AddComponent(IndexA, new NameAgeComponent {Age = 50});
            entityAb2.AddComponent(IndexB, new ComponentB());

            var didExecute = 0;
            system.ExecuteAction = delegate
            {
                didExecute += 1;
                entityAb1.retainCount.Should().Be(2); // retained by context, group
                entityAb2.retainCount.Should().Be(3); // retained by context, group and collector
            };

            system.Execute();
            didExecute.Should().Be(1);

            system.Execute();

            system.Entities.Length.Should().Be(1);
            system.Entities[0].Should().BeSameAs(entityAb2);

            entityAb1.retainCount.Should().Be(2); // retained by context and group
            entityAb2.retainCount.Should().Be(2); // retained by context and group
        }

        [Fact]
        public void ClearsReactiveSystemAfterExecute()
        {
            var system = new ReactiveSystemSpy(_context.CreateCollector(_matcherAb));
            system.ExecuteAction = entities => entities[0].ReplaceComponent(IndexA, new ComponentA());
            var entity = CreateEntityAb();
            system.Execute();
            system.Clear();
            system.Execute();
            AssertEntities(system, entity);
        }

        static void AssertEntities(IReactiveSystemSpy system, Entity entity, int didExecute = 1)
        {
            if (entity == null)
            {
                system.DidExecute.Should().Be(0);
                system.Entities.Should().BeNull();
            }
            else
            {
                system.DidExecute.Should().Be(didExecute);
                system.Entities.Length.Should().Be(1);
                system.Entities.Should().Contain(entity);
            }
        }

        Entity CreateEntityAb()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            entity.AddComponent(IndexB, new ComponentB());
            return entity;
        }
    }
}
