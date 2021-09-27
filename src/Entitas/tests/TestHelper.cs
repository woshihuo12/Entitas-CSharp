using System.Collections.Generic;

namespace Entitas.Tests
{
    public static class TestHelper
    {
        public const int IndexA = 1;
        public const int IndexB = 2;
        public const int TotalComponents = 3;

        public static readonly int[] IndexesA = {IndexA};
        public static readonly int[] IndicesAb = {IndexA, IndexB};

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
