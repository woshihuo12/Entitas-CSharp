using System.Collections.Generic;

namespace Entitas.Tests
{
    public static class TestHelper
    {
        public const int IndexA = 1;
        public const int IndexB = 2;
        public const int IndexC = 3;
        public const int IndexD = 4;
        public const int TotalComponents = 5;

        public static Entity CreateEntity(int totalComponents = TotalComponents)
        {
            var entity = new Entity();
            entity.Initialize(0, totalComponents, new Stack<IComponent>[totalComponents]);
            return entity;
        }

        public static Entity CreateEntityA()
        {
            var entity = CreateEntity();
            entity.AddComponent(IndexA, new ComponentA());
            return entity;
        }

        public static Entity CreateEntityAb()
        {
            var entity = CreateEntityA();
            entity.AddComponent(IndexB, new ComponentB());
            return entity;
        }
    }
}
