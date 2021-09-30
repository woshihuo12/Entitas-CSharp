namespace Entitas.Tests.Fixtures.Entities
{
    public class TestEntity : Entity, IMyEntity
    {
        public NameAgeComponent NameAge { get; }
        public bool HasNameAge { get; }
        public void AddNameAge(string newName, int newAge) { }
        public void ReplaceNameAge(string newName, int newAge) { }
        public void RemoveNameAge() { }
    }
}
