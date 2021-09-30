using System;
using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Systems;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class JobSystemTests
    {
        readonly TestContext _context = new();

        [Fact]
        public void ProcessesEntity()
        {
            var system = new TestJobSystem(_context, 2);
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new NameAgeComponent {Name = "e", Age = -1});
            system.Execute();
            ((NameAgeComponent)entity.GetComponent(IndexA)).Name.Should().Be("e-Processed");
        }

        [Fact]
        public void ProcessesAllEntitiesWhenCountIsDividableByNumThreads()
        {
            var system = new TestJobSystem(_context, 2);
            CreateEntities(4);

            system.Execute();

            var entities = _context.GetEntities();
            entities.Length.Should().Be(4);
            for (var i = 0; i < entities.Length; i++)
            {
                var nameAge = ((NameAgeComponent)entities[i].GetComponent(IndexA));
                nameAge.Name.Should().Be("e" + i + "-Processed");
                nameAge.Age.Should().BeGreaterOrEqualTo(0);
            }
        }

        [Fact]
        public void ProcessesAllEntitiesWhenCountIsNotDividableByNumThreads()
        {
            var system = new TestJobSystem(_context, 4);
            CreateEntities(103);

            system.Execute();

            var entities = _context.GetEntities();
            entities.Length.Should().Be(103);
            for (var i = 0; i < entities.Length; i++)
            {
                var nameAge = ((NameAgeComponent)entities[i].GetComponent(IndexA));
                nameAge.Name.Should().Be("e" + i + "-Processed");
                nameAge.Age.Should().BeGreaterOrEqualTo(0);
            }
        }

        [Fact]
        public void ThrowsWhenThreadThrows()
        {
            var system = new TestJobSystem(_context, 2);
            system.Exception = new Exception("Test Exception");
            CreateEntities(10);
            system.Invoking(s => s.Execute()).Should().Throw<Exception>();
        }

        [Fact]
        public void RecoversFromException()
        {
            var system = new TestJobSystem(_context, 2)
            {
                Exception = new Exception("Test Exception")
            };
            CreateEntities(10);

            var didThrow = 0;
            try
            {
                system.Execute();
            }
            catch (Exception e)
            {
                didThrow += 1;
            }

            didThrow.Should().Be(1);
            system.Exception = null;
            system.Invoking(s => s.Exception).Should().NotThrow();
        }

        void CreateEntities(int count)
        {
            for (var i = 0; i < count; i++)
                _context.CreateEntity().AddComponent(IndexA, new NameAgeComponent {Name = "e" + i, Age = -1});
        }
    }
}
