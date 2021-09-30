namespace Entitas.Tests.Fixtures.Entities
{
    public interface INameAgeEntity
    {
        NameAgeComponent NameAge { get; }
        bool HasNameAge { get; }
        void AddNameAge(string newName, int newAge);
        void ReplaceNameAge(string newName, int newAge);
        void RemoveNameAge();
    }
}
