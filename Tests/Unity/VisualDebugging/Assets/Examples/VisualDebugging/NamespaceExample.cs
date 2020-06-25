using System.Collections.Generic;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

public class NamespaceExample : MonoBehaviour
{
    Feature _systems;

    void Start()
    {
        var contexts = Contexts.sharedInstance;
        _systems = new Feature();
        _systems.Add(new My.Namespace.PositionSystem(contexts));
        _systems.Add(new YourNamespace.PositionSystem(contexts));

        _systems.Initialize();
    }

    void Update()
    {
        _systems.Execute();
    }
}

namespace My.Namespace
{
    public class PositionSystem : ReactiveSystem<AppEntity>, IInitializeSystem
    {
        Contexts _contexts;

        public PositionSystem(Contexts contexts) : base(contexts.myNamespaceApp) => _contexts = contexts;
        protected override ICollector<AppEntity> GetTrigger(IContext<AppEntity> context) => context.CreateCollector(AppMatcher.Game1Position);
        protected override bool Filter(AppEntity entity) => entity.hasGame1Position;
        public void Initialize() => _contexts.myNamespaceApp.CreateEntity().AddGame1Position(Vector3Int.zero);

        protected override void Execute(List<AppEntity> entities)
        {
            foreach (var e in entities)
            {
                var i = _contexts.myNamespaceApp.GetEntityWithGame1Position(e.game1Position.Value);
                UnityEngine.Debug.Log("My position: " + (i.game1Position.Value));
            }
        }
    }
}

namespace YourNamespace
{
    public class PositionSystem : ReactiveSystem<AppEntity>, IInitializeSystem
    {
        readonly Contexts _contexts;

        public PositionSystem(Contexts contexts) : base(contexts.yourNamespaceApp) => _contexts = contexts;
        protected override ICollector<AppEntity> GetTrigger(IContext<AppEntity> context) => context.CreateCollector(AppMatcher.Game1Position);
        protected override bool Filter(AppEntity entity) => entity.hasGame1Position;
        public void Initialize() => _contexts.yourNamespaceApp.CreateEntity().AddGame1Position(Vector3Int.zero);

        protected override void Execute(List<AppEntity> entities)
        {
            foreach (var e in entities)
            {
                var i = _contexts.yourNamespaceApp.GetEntityWithGame1Position(e.game1Position.Value);
                UnityEngine.Debug.Log("Your position: " + (i.game1Position.Value));
            }
        }
    }
}

namespace Game1
{
    [My.Namespace.App, YourNamespace.App]
    public class AppBooted : IComponent
    {
    }

    [My.Namespace.App, YourNamespace.App, Unique]
    public class AppNameComponent : IComponent
    {
        public string Value;
    }

    [My.Namespace.App, YourNamespace.App, Event(EventTarget.Self)]
    public class PositionComponent : IComponent
    {
        [PrimaryEntityIndex]
        public Vector3Int Value;
    }
}

namespace Game2
{
    [My.Namespace.App, YourNamespace.App, Unique]
    public class AppBooted : IComponent
    {
    }

    [My.Namespace.App, YourNamespace.App]
    public class AppNameComponent : IComponent
    {
        public string Value;
    }

    [My.Namespace.App, YourNamespace.App, Event(EventTarget.Self)]
    public class PositionComponent : IComponent
    {
        [PrimaryEntityIndex]
        public Vector3Int Value;
    }
}
