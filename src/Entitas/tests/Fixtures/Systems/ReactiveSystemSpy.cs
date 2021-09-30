using System;
using System.Collections.Generic;

namespace Entitas.Tests.Fixtures.Systems
{
    public interface IReactiveSystemSpy
    {
        int DidInitialize { get; }
        int DidExecute { get; }
        int DidCleanup { get; }
        int DidTearDown { get; }
        Entity[] Entities { get; }
    }

    public class ReactiveSystemSpy : ReactiveSystem<Entity>, IReactiveSystemSpy, IInitializeSystem, ICleanupSystem, ITearDownSystem
    {
        public int DidInitialize => _didInitialize;
        public int DidExecute => _didExecute;
        public int DidCleanup => _didCleanup;
        public int DidTearDown => _didTearDown;
        public Entity[] Entities => _entities;

        public Action<List<Entity>> ExecuteAction;

        protected int _didInitialize;
        protected int _didExecute;
        protected int _didCleanup;
        protected int _didTearDown;
        protected Entity[] _entities;

        readonly Func<Entity, bool> _filter;

        public ReactiveSystemSpy(ICollector<Entity> collector) : base(collector) { }

        public ReactiveSystemSpy(ICollector<Entity> collector, Func<Entity, bool> filter) : this(collector) => _filter = filter;

        protected override ICollector<Entity> GetTrigger(IContext<Entity> context) => null;
        protected override bool Filter(Entity entity) => _filter == null || _filter(entity);
        public void Initialize() => _didInitialize += 1;

        protected override void Execute(List<Entity> entities)
        {
            _didExecute += 1;
            _entities = entities?.ToArray();
            ExecuteAction?.Invoke(entities);
        }

        public void Cleanup() => _didCleanup += 1;
        public void TearDown() => _didTearDown += 1;
    }
}
