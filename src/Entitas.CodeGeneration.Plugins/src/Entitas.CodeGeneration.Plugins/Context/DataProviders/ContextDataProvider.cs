using System.Collections.Generic;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Serialization;

namespace Entitas.CodeGeneration.Plugins {

    public class ContextDataProvider : IDataProvider, IConfigurable {

        public string name { get { return "Context"; } }
        public int priority { get { return 0; } }
        public bool runInDryMode { get { return true; } }

        public Dictionary<string, string> defaultProperties { get { return _contextNamesConfig.defaultProperties; } }

        readonly ContextNamesConfig _contextNamesConfig = new ContextNamesConfig();

        public void Configure(Preferences preferences) {
            _contextNamesConfig.Configure(preferences);
        }

        public CodeGeneratorData[] GetData() {
            return _contextNamesConfig.contextNames
                .Select(contextName =>
                {
                    var data = new ContextData();
                    var (ns, typeName) = contextName.ExtractNamespace();
                    data.SetFullContextName(contextName);
                    data.SetContextName(typeName);
                    data.SetContextNamespace(ns);
                    return data;
                }).ToArray();
        }
    }

    public static class ContextDataExtension {

        public const string CONTEXT_FULL_NAME = "Context.FullName";
        public const string CONTEXT_NAME = "Context.Name";
        public const string CONTEXT_NAMESPACE = "Context.Namespace";

        public static string GetFullContextName(this ContextData data) {
            return (string)data[CONTEXT_FULL_NAME];
        }

        public static void SetFullContextName(this ContextData data, string contextName) {
            data[CONTEXT_FULL_NAME] = contextName;
        }

        public static string GetContextName(this ContextData data) {
            return (string)data[CONTEXT_NAME];
        }

        public static void SetContextName(this ContextData data, string contextName) {
            data[CONTEXT_NAME] = contextName;
        }

        public static string GetContextNamespace(this ContextData data) {
            return (string)data[CONTEXT_NAMESPACE];
        }

        public static void SetContextNamespace(this ContextData data, string contextNamespace) {
            data[CONTEXT_NAMESPACE] = contextNamespace;
        }
    }
}
