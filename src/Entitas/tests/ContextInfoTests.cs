using FluentAssertions;
using Xunit;

namespace Entitas.Tests
{
    public class ContextInfoTests
    {
        [Fact]
        public void SetsFieldsWithConstructorValues()
        {
            var contextName = "Test Context";
            var componentNames = new[] {"ComponentA", "ComponentB"};
            var componentTypes = new[] {typeof(ComponentA), typeof(ComponentB)};

            var info = new ContextInfo(contextName, componentNames, componentTypes);

            info.name.Should().Be(contextName);
            info.componentNames.Should().BeSameAs(componentNames);
            info.componentTypes.Should().BeSameAs(componentTypes);
        }
    }
}
