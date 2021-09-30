using System;
using Entitas.Blueprints;
using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class EntitasErrorMessagesTests
    {
        readonly ITestOutputHelper _output;
        readonly TestContext _context;
        readonly Entity _entity;

        public EntitasErrorMessagesTests(ITestOutputHelper output)
        {
            _output = output;
            var componentNames = new[] {"Health", "Position", "View"};
            var contextInfo = new ContextInfo("Test Context", componentNames, null);
            _context = new TestContext(componentNames.Length, 42, contextInfo);
            _entity = _context.CreateEntity();
        }

        [Fact]
        public void AddComponent()
        {
            _entity.Destroy();
            PrintErrorMessage(() => _entity.AddComponent(IndexA, new ComponentA()));
        }

        [Fact]
        public void RemoveComponent()
        {
            _entity.Destroy();
            PrintErrorMessage(() => _entity.RemoveComponent(IndexA));
        }

        [Fact]
        public void ReplaceComponent()
        {
            _entity.Destroy();
            PrintErrorMessage(() => _entity.ReplaceComponent(IndexA, new ComponentA()));
        }

        [Fact]
        public void AddComponentTwice()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            PrintErrorMessage(() => _entity.AddComponent(IndexA, new ComponentA()));
        }

        [Fact]
        public void RemoveComponentThatDoesNotExist()
        {
            PrintErrorMessage(() => _entity.RemoveComponent(IndexA));
        }

        [Fact]
        public void GetComponentThatDoesNotExist()
        {
            PrintErrorMessage(() => _entity.GetComponent(IndexA));
        }

        [Fact]
        public void RetainEntityTwice()
        {
            var owner = new object();
            _entity.Retain(owner);
            PrintErrorMessage(() => _entity.Retain(owner));
        }

        [Fact]
        public void ReleaseEntityWithUnknownOwner()
        {
            PrintErrorMessage(() => _entity.Release(new object()));
        }

        [Fact]
        public void GetSingleEntityWhenMultipleExist()
        {
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            var matcher = (Matcher<Entity>)Matcher<Entity>.AllOf(IndexA);
            matcher.componentNames = _context.contextInfo.componentNames;
            var group = _context.GetGroup(matcher);
            PrintErrorMessage(() => group.GetSingleEntity());
        }

        [Fact]
        public void UnbalancedGroups()
        {
            var groupA = new Group<Entity>(Matcher<Entity>.AllOf(IndexA));
            var groupB = new Group<Entity>(Matcher<Entity>.AllOf(IndexB));
            PrintErrorMessage(() => new Collector<Entity>(
                new[] {groupA, groupB},
                new[] {GroupEvent.Added}
            ));
        }

        [Fact]
        public void UnbalancedContextInfoComponentNamesCount()
        {
            var componentNames = new[] {"Health", "Position", "View"};
            var contextInfo = new ContextInfo("Test Context", componentNames, null);
            PrintErrorMessage(() => new TestContext(1, 0, contextInfo));
        }

        [Fact]
        public void DestroyRetainedEntities()
        {
            var entity = _context.CreateEntity();
            entity.Retain(this);
            entity.Retain(new object());

            entity = _context.CreateEntity();
            entity.Retain(this);
            entity.Retain(new object());

            PrintErrorMessage(() => _context.DestroyAllEntities());
        }

        [Fact]
        public void ReleaseEntityBeforeDestroy()
        {
            PrintErrorMessage(() => _entity.Release(_context));
        }

        [Fact]
        public void UnknownEntityIndex()
        {
            PrintErrorMessage(() => _context.GetEntityIndex("unknown"));
        }

        [Fact]
        public void DuplicateEntityIndex()
        {
            var groupA = _context.GetGroup((Matcher<Entity>)Matcher<Entity>.AllOf(IndexA));
            var index = new PrimaryEntityIndex<Entity, string>("TestIndex", groupA, delegate { return string.Empty; });
            _context.AddEntityIndex(index);
            PrintErrorMessage(() => _context.AddEntityIndex(index));
        }

        [Fact]
        public void GetSingleEntityWhenMoreThanOneExist()
        {
            PrintErrorMessage(() => new IEntity[2].SingleEntity());
        }

        [Fact]
        public void BlueprintTypeDoesNotImplementIComponent()
        {
            var componentBlueprint = new ComponentBlueprint {fullTypeName = "string"};
            PrintErrorMessage(() => componentBlueprint.CreateComponent(_entity));
        }

        [Fact]
        public void BlueprintTypeDoesNotExist()
        {
            var componentBlueprint = new ComponentBlueprint {fullTypeName = "Unknown"};
            PrintErrorMessage(() => componentBlueprint.CreateComponent(_entity));
        }

        [Fact]
        public void PrimaryIndexMultipleEntitiesForKey()
        {
            new PrimaryEntityIndex<Entity, string>(
                "TestIndex",
                _context.GetGroup((Matcher<Entity>)Matcher<Entity>.AllOf(IndexA)),
                delegate { return "key"; }
            );
            _context.CreateEntity().AddComponent(IndexA, new ComponentA());
            PrintErrorMessage(() => _context.CreateEntity().AddComponent(IndexA, new ComponentA()));
        }

        void PrintErrorMessage(Action action)
        {
            var didFail = false;
            try
            {
                action();
            }
            catch (Exception exception)
            {
                didFail = true;
                _output.WriteLine("================================================================================");
                _output.WriteLine("Exception preview for: " + exception.GetType());
                _output.WriteLine("--------------------------------------------------------------------------------");
                _output.WriteLine(exception.Message);
                _output.WriteLine("================================================================================");
            }

            didFail.Should().BeTrue();
        }
    }
}
