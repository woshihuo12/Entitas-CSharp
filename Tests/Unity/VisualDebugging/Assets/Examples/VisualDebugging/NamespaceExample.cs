using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

public class NamespaceExample : MonoBehaviour
{
    void Start()
    {
        var contexts = Contexts.sharedInstance;
        contexts.myNamespaceApp.CreateEntity().isGame1AppBooted = true;
        contexts.myNamespaceApp.CreateEntity().AddGame1AppName("My Game 1");

        contexts.myNamespaceApp.CreateEntity().isGame2AppBooted = true;
        contexts.myNamespaceApp.CreateEntity().AddGame1AppName("My Game 2");
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
