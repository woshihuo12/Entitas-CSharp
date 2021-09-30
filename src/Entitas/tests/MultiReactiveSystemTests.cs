using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Entities;
using FluentAssertions;
using Xunit;

namespace Entitas.Tests
{
    public class MultiReactiveSystemTests
    {
        readonly MultiReactiveSystemSpy _multiReactiveSystem;
        readonly MultiTriggeredMultiReactiveSystemSpy _multiTriggeredReactiveSystem;
        readonly IMyEntity _entity1;
        readonly IMyEntity _entity2;
        readonly IMyEntity _entity3;

        public MultiReactiveSystemTests()
        {
            var contexts = new TestContexts();
            _multiReactiveSystem = new MultiReactiveSystemSpy(contexts)
            {
                ExecuteAction = entities =>
                {
                    foreach (var e in entities)
                        e.NameAge.Age += 10;
                }
            };

            _entity1 = contexts.Context1.CreateEntity();
            _entity1.AddNameAge("Test1", 42);
            _entity2 = contexts.Context2.CreateEntity();
            _entity2.AddNameAge("Test2", 24);
            _multiReactiveSystem.Execute();

            contexts = new TestContexts();
            _multiTriggeredReactiveSystem = new MultiTriggeredMultiReactiveSystemSpy(contexts);
            _entity3 = contexts.Context1.CreateEntity();
            _entity3.AddNameAge("Test3", 32);
            _entity3.RemoveNameAge();
            _multiTriggeredReactiveSystem.Execute();
        }

        [Fact]
        public void ProcessesEntitiesFromDifferentContexts()
        {
            _multiReactiveSystem.Entities.Length.Should().Be(2);
            _multiReactiveSystem.Entities.Should().Contain(_entity1);
            _multiReactiveSystem.Entities.Should().Contain(_entity2);
            _entity1.NameAge.Age.Should().Be(52);
            _entity2.NameAge.Age.Should().Be(34);
        }

        [Fact]
        public void ExecutesOnce()
        {
            _multiReactiveSystem.DidExecute.Should().Be(1);
        }

        [Fact]
        public void MultiReactiveSystemToString()
        {
            _multiReactiveSystem.ToString().Should().Be("MultiReactiveSystem(MultiReactiveSystemSpy)");
        }

        [Fact]
        public void MultiTriggeredSystemExecutesOnce()
        {
            _multiTriggeredReactiveSystem.DidExecute.Should().Be(1);
        }

        [Fact]
        public void MultiTriggeredSystemMergesCollectedEntitiesAndRemovesDuplicates()
        {
            _multiTriggeredReactiveSystem.Entities.Length.Should().Be(1);
        }

        [Fact]
        public void MultiTriggeredSystemClearsMergedCollectedEntities()
        {
            _multiTriggeredReactiveSystem.Execute();
            _multiTriggeredReactiveSystem.DidExecute.Should().Be(1);
        }
    }
}
