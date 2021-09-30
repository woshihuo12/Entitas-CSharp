using System;
using System.Threading;
using static Entitas.Tests.TestHelper;

namespace Entitas.Tests.Fixtures.Systems
{
    public class TestJobSystem : JobSystem<Entity>
    {
        public Exception Exception;

        public TestJobSystem(TestContext context, int threads) :
            base(context.GetGroup(Matcher<Entity>.AllOf(IndexA)), threads) { }

        protected override void Execute(Entity entity)
        {
            if (Exception != null)
                throw Exception;

            var nameAge = (NameAgeComponent)entity.GetComponent(IndexA);
            nameAge.Name += "-Processed";
            nameAge.Age = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
