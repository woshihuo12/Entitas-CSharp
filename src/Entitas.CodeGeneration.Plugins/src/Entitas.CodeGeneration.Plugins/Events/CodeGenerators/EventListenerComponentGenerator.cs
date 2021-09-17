using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class EventListenerComponentGenerator : AbstractGenerator {

        public override string name { get { return "Event (Listener Component)"; } }

        const string TEMPLATE =
            @"[Entitas.CodeGeneration.Attributes.DontGenerate(false)]
public sealed class ${EventListenerComponent} : Entitas.IComponent {
    public System.Collections.Generic.List<I${EventListener}> value;
}
";

        public override CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<ComponentData>()
                .Where(d => d.IsEvent())
                .SelectMany(generate)
                .ToArray();
        }

        CodeGenFile[] generate(ComponentData data) {
            return data.GetContextNames()
                .SelectMany(contextName => generate(contextName, data))
                .ToArray();
        }

        CodeGenFile[] generate(string contextName, ComponentData data) {
            var (ns, typeName) = contextName.ExtractNamespace();
            return data.GetEventData()
                .Select(eventData => new CodeGenFile(
                    contextName.RemoveDots() + Path.DirectorySeparatorChar +
                    "Components" + Path.DirectorySeparatorChar +
                    data.ComponentNameWithContext(typeName).AddComponentSuffix() + ".cs",
                    TEMPLATE.Replace(data, contextName, eventData).WrapInNamespace(ns),
                    GetType().FullName
                )).ToArray();
        }
    }
}
