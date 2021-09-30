namespace Entitas.Tests.Fixtures.Entities
{
    public class Test2Entity : Entity, IMyEntity
    {
        public NameAgeComponent NameAge { get; }
        public bool HasNameAge { get; }
        public void AddNameAge(string newName, int newAge) { }
        public void ReplaceNameAge(string newName, int newAge) { }
        public void RemoveNameAge() { }
    }
}
