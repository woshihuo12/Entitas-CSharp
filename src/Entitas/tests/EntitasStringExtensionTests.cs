using FluentAssertions;
using Xunit;

namespace Entitas.Tests
{
    public class EntitasStringExtensionTests
    {
        const string Word = "Word";

        [Fact]
        public void DoesNotAddSuffixToStringEndingWithSuffix()
        {
            (Word + EntitasStringExtension.CONTEXT_SUFFIX).AddContextSuffix().Should().Be(Word + EntitasStringExtension.CONTEXT_SUFFIX);
            (Word + EntitasStringExtension.ENTITY_SUFFIX).AddEntitySuffix().Should().Be(Word + EntitasStringExtension.ENTITY_SUFFIX);
            (Word + EntitasStringExtension.COMPONENT_SUFFIX).AddComponentSuffix().Should().Be(Word + EntitasStringExtension.COMPONENT_SUFFIX);
            (Word + EntitasStringExtension.SYSTEM_SUFFIX).AddSystemSuffix().Should().Be(Word + EntitasStringExtension.SYSTEM_SUFFIX);
            (Word + EntitasStringExtension.MATCHER_SUFFIX).AddMatcherSuffix().Should().Be(Word + EntitasStringExtension.MATCHER_SUFFIX);
            (Word + EntitasStringExtension.LISTENER_SUFFIX).AddListenerSuffix().Should().Be(Word + EntitasStringExtension.LISTENER_SUFFIX);
        }

        [Fact]
        public void AddsSuffixToStringEndingWithSuffix()
        {
            Word.AddContextSuffix().Should().Be(Word + EntitasStringExtension.CONTEXT_SUFFIX);
            Word.AddEntitySuffix().Should().Be(Word + EntitasStringExtension.ENTITY_SUFFIX);
            Word.AddComponentSuffix().Should().Be(Word + EntitasStringExtension.COMPONENT_SUFFIX);
            Word.AddSystemSuffix().Should().Be(Word + EntitasStringExtension.SYSTEM_SUFFIX);
            Word.AddMatcherSuffix().Should().Be(Word + EntitasStringExtension.MATCHER_SUFFIX);
            Word.AddListenerSuffix().Should().Be(Word + EntitasStringExtension.LISTENER_SUFFIX);
        }

        [Fact]
        public void DoesNotChangeStringWhenNotEndingWithSuffix()
        {
            Word.RemoveContextSuffix().Should().Be(Word);
            Word.RemoveEntitySuffix().Should().Be(Word);
            Word.RemoveComponentSuffix().Should().Be(Word);
            Word.RemoveSystemSuffix().Should().Be(Word);
            Word.RemoveMatcherSuffix().Should().Be(Word);
            Word.RemoveListenerSuffix().Should().Be(Word);
        }

        [Fact]
        public void RemovesSuffixWhenEndingWithSuffix()
        {
            (Word + EntitasStringExtension.CONTEXT_SUFFIX).RemoveContextSuffix().Should().Be(Word);
            (Word + EntitasStringExtension.ENTITY_SUFFIX).RemoveEntitySuffix().Should().Be(Word);
            (Word + EntitasStringExtension.COMPONENT_SUFFIX).RemoveComponentSuffix().Should().Be(Word);
            (Word + EntitasStringExtension.SYSTEM_SUFFIX).RemoveSystemSuffix().Should().Be(Word);
            (Word + EntitasStringExtension.MATCHER_SUFFIX).RemoveMatcherSuffix().Should().Be(Word);
            (Word + EntitasStringExtension.LISTENER_SUFFIX).RemoveListenerSuffix().Should().Be(Word);
        }

        [Fact]
        public void DoesNotHaveSuffix()
        {
            Word.HasContextSuffix().Should().BeFalse();
            Word.HasEntitySuffix().Should().BeFalse();
            Word.HasComponentSuffix().Should().BeFalse();
            Word.HasSystemSuffix().Should().BeFalse();
            Word.HasMatcherSuffix().Should().BeFalse();
            Word.HasListenerSuffix().Should().BeFalse();
        }

        [Fact]
        public void HasSuffix()
        {
            (Word + EntitasStringExtension.CONTEXT_SUFFIX).HasContextSuffix().Should().BeTrue();
            (Word + EntitasStringExtension.ENTITY_SUFFIX).HasEntitySuffix().Should().BeTrue();
            (Word + EntitasStringExtension.COMPONENT_SUFFIX).HasComponentSuffix().Should().BeTrue();
            (Word + EntitasStringExtension.SYSTEM_SUFFIX).HasSystemSuffix().Should().BeTrue();
            (Word + EntitasStringExtension.MATCHER_SUFFIX).HasMatcherSuffix().Should().BeTrue();
            (Word + EntitasStringExtension.LISTENER_SUFFIX).HasListenerSuffix().Should().BeTrue();
        }
    }
}
