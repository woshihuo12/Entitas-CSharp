using FluentAssertions;
using Xunit;

namespace Entitas.Tests
{
    public class EntitasExceptionTests
    {
        [Fact]
        public void CreatesExceptionWithHintSeparatedByNewLine()
        {
            const string msg = "Message";
            const string hint = "Hint";
            var exception = new EntitasException(msg, hint);
            exception.Message.Should().Be(msg + "\n" + hint);
        }

        [Fact]
        public void IgnoresHintWhenNull()
        {
            const string msg = "Message";
            const string hint = null;
            var ex = new EntitasException(msg, hint);
            ex.Message.Should().Be(msg);
        }
    }
}
