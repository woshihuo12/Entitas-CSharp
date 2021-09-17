using System;

namespace Entitas.CodeGeneration.Plugins {

    public class ComponentNamespaceComponentDataProvider : IComponentDataProvider {

        public void Provide(Type type, ComponentData data) {
            data.SetNamespace(type.Namespace);
        }
    }

    public static class ComponentNamespaceComponentDataExtension {

        public const string COMPONENT_NAMESPACE = "Component.Namespace";

        public static string GetNamespace(this ComponentData data) {
            return (string)data[COMPONENT_NAMESPACE];
        }

        public static void SetNamespace(this ComponentData data, string ns) {
            data[COMPONENT_NAMESPACE] = ns;
        }
    }
}
