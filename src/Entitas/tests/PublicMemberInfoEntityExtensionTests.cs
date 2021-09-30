using Entitas.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    public class PublicMemberInfoEntityExtensionTests
    {
        readonly IContext<Entity> _context = new TestContext();
        readonly Entity _entity;
        readonly Entity _target;
        readonly NameAgeComponent _nameAge;

        public PublicMemberInfoEntityExtensionTests()
        {
            _entity = _context.CreateEntity();
            _target = _context.CreateEntity();
            _nameAge = new NameAgeComponent {Name = "Test", Age = 42};
        }

        [Fact]
        public void DoesNotChangeEntityIfOriginalDoesNotHaveAnyComponents()
        {
            _entity.CopyTo(_target);
            _entity.creationIndex.Should().Be(0);
            _target.creationIndex.Should().Be(1);
            _target.GetComponents().Length.Should().Be(0);
        }

        [Fact]
        public void AddsCopiesOfAllComponentsToTargetEntity()
        {
            var componentA = new ComponentA();
            _entity.AddComponent(IndexA, componentA);
            _entity.AddComponent(IndexB, _nameAge);
            _entity.CopyTo(_target);
            _target.GetComponents().Length.Should().Be(2);
            _target.HasComponent(IndexA).Should().BeTrue();
            _target.HasComponent(IndexB).Should().BeTrue();
            _target.GetComponent(IndexA).Should().NotBeSameAs(componentA);
            _target.GetComponent(IndexB).Should().NotBeSameAs(_nameAge);
            var clonedComponent = (NameAgeComponent)_target.GetComponent(IndexB);
            clonedComponent.Name.Should().Be(_nameAge.Name);
            clonedComponent.Age.Should().Be(_nameAge.Age);
        }

        [Fact]
        public void ThrowsWhenTargetAlreadyHasComponentAtIndex()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            _entity.AddComponent(IndexB, _nameAge);
            _target.AddComponent(IndexB, new NameAgeComponent());
            _entity.Invoking(entity => entity.CopyTo(_target))
                .Should().Throw<EntityAlreadyHasComponentException>();
        }

        [Fact]
        public void ReplacesExistingComponentsWhenOverwriteIsSet()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            _entity.AddComponent(IndexB, _nameAge);
            var component = new NameAgeComponent();
            _target.AddComponent(IndexB, component);
            _entity.CopyTo(_target, true);
            var clonedComponent = (NameAgeComponent)_target.GetComponent(IndexB);
            clonedComponent.Should().NotBeSameAs(_nameAge);
            clonedComponent.Should().NotBeSameAs(component);
            clonedComponent.Name.Should().Be(_nameAge.Name);
            clonedComponent.Age.Should().Be(_nameAge.Age);
        }

        [Fact]
        public void OnlyAddsCopiesOfSpecifiedComponentsToTargetEntity()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            _entity.AddComponent(IndexB, new ComponentB());
            _entity.AddComponent(IndexC, new ComponentC());
            _entity.CopyTo(_target, false, IndexB, IndexC);
            _target.GetComponents().Length.Should().Be(2);
            _target.HasComponent(IndexB).Should().BeTrue();
            _target.HasComponent(IndexC).Should().BeTrue();
        }

        [Fact]
        public void UsesComponentPool()
        {
            _entity.AddComponent(IndexA, new ComponentA());
            var component = new ComponentA();
            _target.GetComponentPool(IndexA).Push(component);
            _entity.CopyTo(_target);
            _target.GetComponent(IndexA).Should().BeSameAs(component);
        }
    }
}
