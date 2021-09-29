using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class MultiKeyPrimaryEntityIndexTests
    {
        const string TestKey = "Test";

        readonly IContext<Entity> _context = new TestContext();
        readonly PrimaryEntityIndex<Entity, string> _index;

        public MultiKeyPrimaryEntityIndexTests() => _index = CreateIndex();

        [Fact]
        public void HasNoEntities()
        {
            _index.GetEntity("unknown").Should().BeNull();
        }

        [Fact]
        public void GetsEntityForKeys()
        {
            var entity = CreateNameAgeEntity();
            _index.GetEntity(TestKey + "1").Should().BeSameAs(entity);
            _index.GetEntity(TestKey + "2").Should().BeSameAs(entity);
        }

        [Fact]
        public void RetainsEntity()
        {
            var entity = CreateNameAgeEntity();
            entity.retainCount.Should().Be(3);
            if (entity.aerc is SafeAERC safeAerc)
                safeAerc.owners.Should().Contain(_index);
        }

        [Fact]
        public void ReleasesAndRemovesEntityWhenComponentGetsRemoved()
        {
            var entity = CreateNameAgeEntity();
            entity.RemoveComponent(IndexA);
            _index.GetEntity(TestKey + "1").Should().BeNull();
            _index.GetEntity(TestKey + "2").Should().BeNull();
            entity.retainCount.Should().Be(1);
            if (entity.aerc is SafeAERC safeAerc)
                safeAerc.owners.Should().NotContain(_index);
        }

        PrimaryEntityIndex<Entity, string> CreateIndex()
        {
            return new("TestIndex", _context.GetGroup(Matcher<Entity>.AllOf(IndexA)),
                (e, c) =>
                {
                    var nameAge = (NameAgeComponent)(c is NameAgeComponent ? c : e.GetComponent(IndexA));
                    return new[] {nameAge.Name + "1", nameAge.Name + "2"};
                });
        }

        Entity CreateNameAgeEntity()
        {
            var entity = _context.CreateEntity();
            entity.AddComponent(IndexA, new NameAgeComponent {Name = TestKey});
            return entity;
        }
    }
}
