using static Entitas.Tests.TestHelper;

namespace Entitas.Tests.Fixtures.Entities
{
    public class Test2Entity : Entity, IMyEntity
    {
        public NameAgeComponent NameAge => (NameAgeComponent)GetComponent(IndexA);
        public bool HasNameAge => HasComponent(IndexA);

        public void AddNameAge(string newName, int newAge)
        {
            AddComponent(IndexA, new NameAgeComponent {Name = newName, Age = newAge});
        }

        public void ReplaceNameAge(string newName, int newAge)
        {
            ReplaceComponent(IndexA, new NameAgeComponent {Name = newName, Age = newAge});
        }

        public void RemoveNameAge() => RemoveComponent(IndexA);
    }
}
