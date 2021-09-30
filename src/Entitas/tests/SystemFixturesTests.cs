using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Systems;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class SystemFixturesTests
    {
        [Fact]
        public void InitializesInitializeSystemSpy()
        {
            var system = new InitializeSystemSpy();
            system.DidInitialize.Should().Be(0);
            system.Initialize();
            system.DidInitialize.Should().Be(1);
        }

        [Fact]
        public void ExecutesExecuteSystemSpy()
        {
            var system = new ExecuteSystemSpy();
            system.DidExecute.Should().Be(0);
            system.Execute();
            system.DidExecute.Should().Be(1);
        }

        [Fact]
        public void CleansUpCleanupSystemSpy()
        {
            var system = new CleanupSystemSpy();
            system.DidCleanup.Should().Be(0);
            system.Cleanup();
            system.DidCleanup.Should().Be(1);
        }

        [Fact]
        public void TearsDownTearDownSystemSpy()
        {
            var system = new TearDownSystemSpy();
            system.DidTearDown.Should().Be(0);
            system.TearDown();
            system.DidTearDown.Should().Be(1);
        }

        [Fact]
        public void InitializesExecutesCleansUpAndTearsDownReactiveSystemSpy()
        {
            var system = CreateReactiveSystem(new TestContext());

            system.DidInitialize.Should().Be(0);
            system.Initialize();
            system.DidInitialize.Should().Be(1);

            system.DidExecute.Should().Be(0);
            system.Execute();
            system.DidExecute.Should().Be(1);

            system.DidCleanup.Should().Be(0);
            system.Cleanup();
            system.DidCleanup.Should().Be(1);

            system.DidTearDown.Should().Be(0);
            system.TearDown();
            system.DidTearDown.Should().Be(1);
        }

        [Fact]
        public void ExecutesReactiveSystemSpy()
        {
            var system = CreateReactiveSystem(new TestContext());
            system.Execute();
            system.Entities.Length.Should().Be(1);
        }

        static ReactiveSystemSpy CreateReactiveSystem(TestContext context)
        {
            var system = new ReactiveSystemSpy(context.CreateCollector(Matcher<Entity>.AllOf(IndexA)));
            context.CreateEntity().AddComponent(IndexA, new ComponentA());
            return system;
        }
    }
}
