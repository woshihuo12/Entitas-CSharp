using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Systems;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class SystemsTests
    {
        readonly TestContext _context = new();
        readonly Systems _systems = new();

        [Fact]
        public void ReturnsSystemsWhenAddingSystem()
        {
            _systems.Add(new InitializeSystemSpy()).Should().BeSameAs(_systems);
        }

        [Fact]
        public void InitializesIInitializeSystem()
        {
            var system = new InitializeSystemSpy();
            _systems.Add(system);
            _systems.Initialize();
            system.DidInitialize.Should().Be(1);
        }

        [Fact]
        public void ExecutesIExecuteSystem()
        {
            var system = new ExecuteSystemSpy();
            _systems.Add(system);
            _systems.Execute();
            system.DidExecute.Should().Be(1);
        }

        [Fact]
        public void ExecutesReactiveSystem()
        {
            var system = new ReactiveSystemSpy(_context.CreateCollector(Matcher<Entity>.AllOf(IndexA)));
            _systems.Add(system);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systems.Execute();
            system.DidExecute.Should().Be(1);
        }

        [Fact]
        public void CleansUpICleanupSystem()
        {
            var system = new CleanupSystemSpy();
            _systems.Add(system);
            _systems.Cleanup();
            system.DidCleanup.Should().Be(1);
        }

        [Fact]
        public void TearsDownITearDownSystem()
        {
            var system = new TearDownSystemSpy();
            _systems.Add(system);
            _systems.TearDown();
            system.DidTearDown.Should().Be(1);
        }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        [Fact]
        public void RemovesIInitializeSystem()
        {
            var system = new InitializeSystemSpy();
            _systems.Add(system);
            _systems.Remove(system);
            _systems.Initialize();
            system.DidInitialize.Should().Be(0);
        }

        [Fact]
        public void RemovesIExecuteSystem()
        {
            var system = new ExecuteSystemSpy();
            _systems.Add(system);
            _systems.Remove(system);
            _systems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void RemovesReactiveSystem()
        {
            var system = new ReactiveSystemSpy(_context.CreateCollector(Matcher<Entity>.AllOf(IndexA)));
            _systems.Add(system);
            _systems.Remove(system);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void RemovesICleanupSystem()
        {
            var system = new CleanupSystemSpy();
            _systems.Add(system);
            _systems.Remove(system);
            _systems.Cleanup();
            system.DidCleanup.Should().Be(0);
        }

        [Fact]
        public void RemovesITearDownSystem()
        {
            var system = new TearDownSystemSpy();
            _systems.Add(system);
            _systems.Remove(system);
            _systems.TearDown();
            system.DidTearDown.Should().Be(0);
        }

        [Fact]
        public void InitializesExecutesCleansUpAndTearsDownReactiveSystemSpy()
        {
            var system = CreateReactiveSystem(new TestContext());
            _systems.Add(system);

            system.DidInitialize.Should().Be(0);
            _systems.Initialize();
            system.DidInitialize.Should().Be(1);

            system.DidExecute.Should().Be(0);
            _systems.Execute();
            system.DidExecute.Should().Be(1);

            system.DidCleanup.Should().Be(0);
            _systems.Cleanup();
            system.DidCleanup.Should().Be(1);

            system.DidTearDown.Should().Be(0);
            _systems.TearDown();
            system.DidTearDown.Should().Be(1);
        }

        [Fact]
        public void ExecutesReactiveSystemOnce()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);

            system.DidExecute.Should().Be(0);
            _systems.Execute();
            _systems.Execute();
            system.DidExecute.Should().Be(1);
        }

        [Fact]
        public void InitializesExecutesCleansUpAndTearsDownRecursively()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);

            var parentSystems = new Systems();
            parentSystems.Add(_systems);

            system.DidInitialize.Should().Be(0);
            parentSystems.Initialize();
            system.DidInitialize.Should().Be(1);

            system.DidExecute.Should().Be(0);
            parentSystems.Execute();
            parentSystems.Execute();
            system.DidExecute.Should().Be(1);

            system.DidCleanup.Should().Be(0);
            parentSystems.Cleanup();
            system.DidCleanup.Should().Be(1);

            system.DidTearDown.Should().Be(0);
            parentSystems.TearDown();
            system.DidTearDown.Should().Be(1);
        }

        [Fact]
        public void ClearsReactiveSystems()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            _systems.ClearReactiveSystems();
            _systems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void ClearsReactiveSystemsRecursively()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            var parentSystems = new Systems();
            parentSystems.Add(_systems);
            parentSystems.ClearReactiveSystems();
            parentSystems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void DeactivatesReactiveSystems()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            _systems.DeactivateReactiveSystems();
            _systems.Execute();
            system.DidExecute.Should().Be(0);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void DeactivatesReactiveSystemsRecursively()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            var parentSystems = new Systems();
            parentSystems.Add(_systems);
            parentSystems.DeactivateReactiveSystems();
            parentSystems.Execute();
            system.DidExecute.Should().Be(0);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            parentSystems.Execute();
            system.DidExecute.Should().Be(0);
        }

        [Fact]
        public void ActivatesReactiveSystems()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            _systems.DeactivateReactiveSystems();
            _systems.ActivateReactiveSystems();
            _systems.Execute();
            system.DidExecute.Should().Be(0);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systems.Execute();
            system.DidExecute.Should().Be(1);
        }

        [Fact]
        public void ActivatesReactiveSystemsRecursively()
        {
            var system = CreateReactiveSystem(_context);
            _systems.Add(system);
            var parentSystems = new Systems();
            parentSystems.Add(_systems);
            parentSystems.DeactivateReactiveSystems();
            parentSystems.ActivateReactiveSystems();
            parentSystems.Execute();
            system.DidExecute.Should().Be(0);
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _systems.Execute();
            system.DidExecute.Should().Be(1);
        }

        static ReactiveSystemSpy CreateReactiveSystem(TestContext context)
        {
            var system = new ReactiveSystemSpy(context.CreateCollector(Matcher<Entity>.AllOf(IndexA)));
            context.CreateEntity().AddComponent(IndexA, new ComponentA());
            return system;
        }
    }
}
