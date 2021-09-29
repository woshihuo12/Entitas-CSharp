using FluentAssertions;
using Xunit;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests
{
    [Collection("Non-Parallel")]
    public class MatcherTests
    {
        readonly Entity _entityA;
        readonly Entity _entityB;
        readonly Entity _entityC;
        readonly Entity _entityAb;
        readonly Entity _entityAbc;
        readonly IAllOfMatcher<Entity> _allOfAb;
        readonly IAnyOfMatcher<Entity> _anyOfAb;
        readonly ICompoundMatcher<Entity> _allOfNoneOf;
        readonly ICompoundMatcher<Entity> _anyOfNoneOf;
        readonly ICompoundMatcher<Entity> _allOfAnyOf;

        public MatcherTests()
        {
            _entityA = CreateEntity();
            _entityA.AddComponent(IndexA, new ComponentA());

            _entityB = CreateEntity();
            _entityB.AddComponent(IndexB, new ComponentB());

            _entityC = CreateEntity();
            _entityC.AddComponent(IndexC, new ComponentC());

            _entityAb = CreateEntity();
            _entityAb.AddComponent(IndexA, new ComponentA());
            _entityAb.AddComponent(IndexB, new ComponentB());

            _entityAbc = CreateEntity();
            _entityAbc.AddComponent(IndexA, new ComponentA());
            _entityAbc.AddComponent(IndexB, new ComponentB());
            _entityAbc.AddComponent(IndexC, new ComponentC());

            var indexesAb = new[] {IndexA, IndexB};
            _allOfAb = Matcher<Entity>.AllOf(indexesAb);
            _anyOfAb = Matcher<Entity>.AnyOf(indexesAb);

            _allOfNoneOf = Matcher<Entity>.AllOf(indexesAb).NoneOf(IndexC, IndexD);
            _anyOfNoneOf = Matcher<Entity>.AnyOf(indexesAb).NoneOf(IndexC, IndexD);
            _allOfAnyOf = Matcher<Entity>.AllOf(indexesAb).AnyOf(IndexC, IndexD);
        }

        [Fact]
        public void AllOfHasAllIndexes()
        {
            var indexesAb = new[] {IndexA, IndexB};
            _allOfAb.indices.Should().BeEquivalentTo(indexesAb);
            _allOfAb.allOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AnyOfHasAllIndexes()
        {
            var indexesAb = new[] {IndexA, IndexB};
            _anyOfAb.indices.Should().BeEquivalentTo(indexesAb);
            _anyOfAb.anyOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AllOfNoneOfHasAllIndexes()
        {
            _allOfNoneOf.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC, IndexD});
            _allOfNoneOf.allOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            _allOfNoneOf.noneOfIndices.Should().BeEquivalentTo(new[] {IndexC, IndexD});
        }

        [Fact]
        public void AnyOfNoneOfHasAllIndexes()
        {
            _anyOfNoneOf.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC, IndexD});
            _anyOfNoneOf.anyOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            _anyOfNoneOf.noneOfIndices.Should().BeEquivalentTo(new[] {IndexC, IndexD});
        }

        [Fact]
        public void AllOfAnyOfHasAllIndexes()
        {
            _allOfAnyOf.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC, IndexD});
            _allOfAnyOf.allOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            _allOfAnyOf.anyOfIndices.Should().BeEquivalentTo(new[] {IndexC, IndexD});
        }

        [Fact]
        public void AllOfHasAllIndexesWithoutDuplicates()
        {
            var matcher = Matcher<Entity>.AllOf(IndexA, IndexA, IndexB, IndexB);
            var indexesAb = new[] {IndexA, IndexB};
            matcher.indices.Should().BeEquivalentTo(indexesAb);
            matcher.allOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AnyOfHasAllIndexesWithoutDuplicates()
        {
            var matcher = Matcher<Entity>.AnyOf(IndexA, IndexA, IndexB, IndexB);
            var indexesAb = new[] {IndexA, IndexB};
            matcher.indices.Should().BeEquivalentTo(indexesAb);
            matcher.anyOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AllOfNoneOfHasAllIndexesWithoutDuplicates()
        {
            var matcher = Matcher<Entity>
                .AllOf(IndexA, IndexA, IndexB)
                .NoneOf(IndexB, IndexC, IndexC);

            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB, IndexC});
        }

        [Fact]
        public void AnyOfNoneOfHasAllIndexesWithoutDuplicates()
        {
            var matcher = Matcher<Entity>
                .AnyOf(IndexA, IndexA, IndexB)
                .NoneOf(IndexB, IndexC, IndexC);

            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB, IndexC});
        }

        [Fact]
        public void AllOfAnyOfHasAllIndexesWithoutDuplicates()
        {
            var matcher = Matcher<Entity>
                .AllOf(IndexA, IndexA, IndexB)
                .AnyOf(IndexB, IndexC, IndexC);

            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB, IndexC});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexB, IndexC});
        }

        [Fact]
        public void AllOfCachesIndexes()
        {
            _allOfAb.indices.Should().BeSameAs(_allOfAb.indices);
        }

        [Fact]
        public void AnyOfCachesIndexes()
        {
            _anyOfAb.indices.Should().BeSameAs(_anyOfAb.indices);
        }

        [Fact]
        public void AllOfNoneOfCachesIndexes()
        {
            _allOfNoneOf.indices.Should().BeSameAs(_allOfNoneOf.indices);
        }

        [Fact]
        public void AnyOfNoneOfCachesIndexes()
        {
            _anyOfNoneOf.indices.Should().BeSameAs(_anyOfNoneOf.indices);
        }

        [Fact]
        public void AllOfAnyOfCachesIndexes()
        {
            _allOfAnyOf.indices.Should().BeSameAs(_allOfAnyOf.indices);
        }

        [Fact]
        public void AllOfDoesNotMatchEntity()
        {
            _allOfAb.Matches(_entityA).Should().BeFalse();
        }

        [Fact]
        public void AnyOfDoesNotMatchEntity()
        {
            _anyOfAb.Matches(_entityC).Should().BeFalse();
        }

        [Fact]
        public void AllOfNoneOfDoesNotMatchEntity()
        {
            _allOfNoneOf.Matches(_entityAbc).Should().BeFalse();
        }

        [Fact]
        public void AnyOfNoneOfDoesNotMatchEntity()
        {
            _anyOfNoneOf.Matches(_entityAbc).Should().BeFalse();
        }

        [Fact]
        public void AllOfAnyOfDoesNotMatchEntity()
        {
            _allOfAnyOf.Matches(_entityAb).Should().BeFalse();
        }

        [Fact]
        public void AllOfMatchesEntity()
        {
            _allOfAb.Matches(_entityAb).Should().BeTrue();
            _allOfAb.Matches(_entityAbc).Should().BeTrue();
        }

        [Fact]
        public void AnyOfMatchesEntity()
        {
            _anyOfAb.Matches(_entityA).Should().BeTrue();
            _anyOfAb.Matches(_entityB).Should().BeTrue();
            _anyOfAb.Matches(_entityAbc).Should().BeTrue();
        }

        [Fact]
        public void AllOfNoneOfMatchesEntity()
        {
            _allOfNoneOf.Matches(_entityAb).Should().BeTrue();
        }

        [Fact]
        public void AnyOfNoneOfMatchesEntity()
        {
            _anyOfNoneOf.Matches(_entityA).Should().BeTrue();
            _anyOfNoneOf.Matches(_entityB).Should().BeTrue();
        }

        [Fact]
        public void AllOfAnyOfMatchesEntity()
        {
            _allOfAnyOf.Matches(_entityAbc).Should().BeTrue();
        }

        [Fact]
        public void AllOfMergesMatchers()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var matcherC = Matcher<Entity>.AllOf(IndexC);
            var merged = Matcher<Entity>.AllOf(matcherA, matcherB, matcherC);
            var indexes = new[] {IndexA, IndexB, IndexC};
            merged.indices.Should().BeEquivalentTo(indexes);
            merged.allOfIndices.Should().BeEquivalentTo(indexes);
        }

        [Fact]
        public void AnyOfMergesMatchers()
        {
            var matcherA = Matcher<Entity>.AnyOf(IndexA);
            var matcherB = Matcher<Entity>.AnyOf(IndexB);
            var matcherC = Matcher<Entity>.AnyOf(IndexC);
            var merged = Matcher<Entity>.AnyOf(matcherA, matcherB, matcherC);
            var indexes = new[] {IndexA, IndexB, IndexC};
            merged.indices.Should().BeEquivalentTo(indexes);
            merged.anyOfIndices.Should().BeEquivalentTo(indexes);
        }

        [Fact]
        public void AllOfMergesMatchersWithoutDuplicates()
        {
            var matcherA1 = Matcher<Entity>.AllOf(IndexA);
            var matcherA2 = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var merged = Matcher<Entity>.AllOf(matcherA1, matcherA2, matcherB);
            var indexesAb = new[] {IndexA, IndexB};
            merged.indices.Should().BeEquivalentTo(indexesAb);
            merged.allOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AnyOfMergesMatchersWithoutDuplicates()
        {
            var matcherA1 = Matcher<Entity>.AnyOf(IndexA);
            var matcherA2 = Matcher<Entity>.AnyOf(IndexA);
            var matcherB = Matcher<Entity>.AnyOf(IndexB);
            var merged = Matcher<Entity>.AnyOf(matcherA1, matcherA2, matcherB);
            var indexesAb = new[] {IndexA, IndexB};
            merged.indices.Should().BeEquivalentTo(indexesAb);
            merged.anyOfIndices.Should().BeEquivalentTo(indexesAb);
        }

        [Fact]
        public void AllOfThrowsWhenMergingAllOfMatcherWithMoreThanOneIndex()
        {
            FluentActions.Invoking(() => Matcher<Entity>.AllOf(Matcher<Entity>.AllOf(IndexA, IndexB)))
                .Should().Throw<MatcherException>();
        }

        [Fact]
        public void AnyOfThrowsWhenMergingMatcherWithMoreThanOneIndex()
        {
            FluentActions.Invoking(() => Matcher<Entity>.AnyOf(Matcher<Entity>.AnyOf(IndexA, IndexB)))
                .Should().Throw<MatcherException>();
        }

        [Fact]
        public void AllOfToString()
        {
            _allOfAb.ToString().Should().Be($"AllOf({IndexA}, {IndexB})");
        }

        [Fact]
        public void AnyOfToString()
        {
            _anyOfAb.ToString().Should().Be($"AnyOf({IndexA}, {IndexB})");
        }

        [Fact]
        public void AllOfNoneOfToString()
        {
            _allOfNoneOf.ToString().Should().Be($"AllOf({IndexA}, {IndexB}).NoneOf({IndexC}, {IndexD})");
        }

        [Fact]
        public void AnyOfNoneOfToString()
        {
            _anyOfNoneOf.ToString().Should().Be($"AnyOf({IndexA}, {IndexB}).NoneOf({IndexC}, {IndexD})");
        }

        [Fact]
        public void AllOfAnyOfToString()
        {
            _allOfAnyOf.ToString().Should().Be($"AllOf({IndexA}, {IndexB}).AnyOf({IndexC}, {IndexD})");
        }

        [Fact]
        public void AllOfUsesComponentNamesWhenSet()
        {
            var matcher = (Matcher<Entity>)_allOfAb;
            matcher.componentNames = new[] {"one", "two", "three"};
            matcher.ToString().Should().Be("AllOf(two, three)");
        }

        [Fact]
        public void AllOfNoneOfUsesComponentNamesWhenSet()
        {
            var matcher = (Matcher<Entity>)_allOfNoneOf;
            matcher.componentNames = new[] {"one", "two", "three", "four", "five"};
            matcher.ToString().Should().Be("AllOf(two, three).NoneOf(four, five)");
        }

        [Fact]
        public void AllOfUsesComponentNamesWhenMergedMatcherToString()
        {
            var matcherA = (Matcher<Entity>)Matcher<Entity>.AllOf(IndexA);
            var matcherB = (Matcher<Entity>)Matcher<Entity>.AllOf(IndexB);
            var matcherC = (Matcher<Entity>)Matcher<Entity>.AllOf(IndexC);
            matcherB.componentNames = new[] {"A", "B", "C", "D"};
            var mergedMatcher = Matcher<Entity>.AllOf(matcherA, matcherB, matcherC);
            mergedMatcher.ToString().Should().Be("AllOf(B, C, D)");
        }

        [Fact]
        public void AllOfNoneOfMutatesMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcher = matcherA.NoneOf(IndexB);
            matcher.Should().BeSameAs(matcherA);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void AllOfNoneOfMutatesMergedMatcher()
        {
            var matcherA1 = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var merged = Matcher<Entity>.AllOf(matcherA1);
            var matcher = merged.NoneOf(matcherB);
            matcher.Should().BeSameAs(merged);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void AnyOfNoneOfMutatesMatcher()
        {
            var matcherA = Matcher<Entity>.AnyOf(IndexA);
            var matcher = matcherA.NoneOf(IndexB);
            matcher.Should().BeSameAs(matcherA);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void AnyOfNoneOfMutatesMergedMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var merged = Matcher<Entity>.AnyOf(matcherA);
            var matcher = merged.NoneOf(matcherB);
            matcher.Should().BeSameAs(merged);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.noneOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void AllOfAnyOfMutatesMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcher = matcherA.AnyOf(IndexB);
            matcher.Should().BeSameAs(matcherA);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void AllOfAnyOfMutatesMergedMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var merged = Matcher<Entity>.AllOf(matcherA);
            var matcher = merged.AnyOf(matcherB);
            matcher.Should().BeSameAs(merged);
            matcher.indices.Should().BeEquivalentTo(new[] {IndexA, IndexB});
            matcher.allOfIndices.Should().BeEquivalentTo(new[] {IndexA});
            matcher.anyOfIndices.Should().BeEquivalentTo(new[] {IndexB});
        }

        [Fact]
        public void UpdatesCacheWhenCallingAnyOf()
        {
            var matcher = Matcher<Entity>.AllOf(IndexA);
            var cache = matcher.indices;
            matcher.AnyOf(IndexB);
            matcher.indices.Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesCacheWhenCallingNoneOf()
        {
            var matcher = Matcher<Entity>.AllOf(IndexA);
            var cache = matcher.indices;
            matcher.NoneOf(IndexB);
            matcher.indices.Should().NotBeSameAs(cache);
        }

        [Fact]
        public void UpdatesHashWhenChangedWithAnyOf()
        {
            var matcher = Matcher<Entity>.AllOf(IndexA, IndexB);
            var hash = matcher.GetHashCode();
            matcher.AnyOf(42).GetHashCode().Should().NotBe(hash);
        }

        [Fact]
        public void UpdatesHashWhenChangedWithNoneOf()
        {
            var matcher = Matcher<Entity>.AllOf(IndexA, IndexB);
            var hash = matcher.GetHashCode();
            matcher.NoneOf(42).GetHashCode().Should().NotBe(hash);
        }

        [Fact]
        public void EqualsEqualAllOfMatcher()
        {
            var matcher1 = Matcher<Entity>.AllOf(IndexA, IndexB);
            var matcher2 = Matcher<Entity>.AllOf(IndexA, IndexB);
            matcher1.Should().NotBeSameAs(matcher2);
            matcher1.Equals(matcher2).Should().BeTrue();
            matcher1.GetHashCode().Should().Be(matcher2.GetHashCode());
        }

        [Fact]
        public void EqualsEqualAllOfMatcherWithDifferentIndexOrder()
        {
            var matcherAb = Matcher<Entity>.AllOf(IndexA, IndexB);
            var matcherBa = Matcher<Entity>.AllOf(IndexB, IndexA);
            matcherAb.Equals(matcherBa).Should().BeTrue();
            matcherAb.GetHashCode().Should().Be(matcherBa.GetHashCode());
        }

        [Fact]
        public void EqualsMergedMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AllOf(IndexB);
            var matcherBa = Matcher<Entity>.AllOf(IndexB, IndexA);
            var merged = Matcher<Entity>.AllOf(matcherA, matcherB);
            merged.Equals(matcherBa).Should().BeTrue();
            merged.GetHashCode().Should().Be(matcherBa.GetHashCode());
        }

        [Fact]
        public void DoesNotEqualDifferentAllOfMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherAb = Matcher<Entity>.AllOf(IndexA, IndexB);
            matcherA.Equals(matcherAb).Should().BeFalse();
            matcherA.GetHashCode().Should().NotBe(matcherAb.GetHashCode());
        }

        [Fact]
        public void AllOfDoesNotEqualAnyOfWithSameIndexes()
        {
            var allOf = Matcher<Entity>.AllOf(IndexA);
            var anyOf = Matcher<Entity>.AnyOf(IndexA);
            allOf.Equals(anyOf).Should().BeFalse();
            allOf.GetHashCode().Should().NotBe(anyOf.GetHashCode());
        }

        [Fact]
        public void DoesNotEqualDifferentTypeMatchersWithSameIndexes()
        {
            var m1 = Matcher<Entity>.AllOf(IndexA);
            var m2 = Matcher<Entity>.AllOf(IndexB);
            var m3 = Matcher<Entity>.AllOf(m1, m2);
            var m4 = Matcher<Entity>.AnyOf(m1, m2);
            m3.Equals(m4).Should().BeFalse();
            m3.GetHashCode().Should().NotBe(m4.GetHashCode());
        }

        [Fact]
        public void EqualsCompoundMatcher()
        {
            var matcherA = Matcher<Entity>.AllOf(IndexA);
            var matcherB = Matcher<Entity>.AnyOf(IndexB);
            var matcherC = Matcher<Entity>.AnyOf(IndexC);
            var matcherD = Matcher<Entity>.AnyOf(IndexD);
            var merged1 = Matcher<Entity>.AllOf(matcherA, matcherB).AnyOf(matcherC, matcherD);
            var merged2 = Matcher<Entity>.AllOf(matcherA, matcherB).AnyOf(matcherC, matcherD);
            merged1.Equals(merged2).Should().BeTrue();
            merged1.GetHashCode().Should().Be(merged2.GetHashCode());
        }
    }
}
