using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class ComponentEntityApiInterfaceGenerator : AbstractGenerator {

        public override string name { get { return "Component (Entity API Interface)"; } }

        const string STANDARD_TEMPLATE =
            @"public partial interface I${ComponentName}Entity {

    ${ComponentType} ${validComponentName} { get; }
    bool has${ComponentName} { get; }

    void Add${ComponentName}(${newMethodParameters});
    void Replace${ComponentName}(${newMethodParameters});
    void Remove${ComponentName}();
}
";

        const string FLAG_TEMPLATE =
            @"public partial interface I${ComponentName}Entity {
    bool ${prefixedComponentName} { get; set; }
}
";

        const string ENTITY_INTERFACE_TEMPLATE = "public partial class ${EntityType} : ${Namespace}I${ComponentName}Entity { }\n";

        public override CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<ComponentData>()
                .Where(d => d.ShouldGenerateMethods())
                .Where(d => d.GetContextNames().Length > 1)
                .SelectMany(generate)
                .ToArray();
        }

        CodeGenFile[] generate(ComponentData data) {
            return new[] { generateInterface(data) }
                .Concat(data.GetContextNames().Select(contextName => generateEntityInterface(contextName, data)))
                .ToArray();
        }

        CodeGenFile generateInterface(ComponentData data) {
            var template = data.GetMemberData().Length == 0
                ? FLAG_TEMPLATE
                : STANDARD_TEMPLATE;

            return new CodeGenFile(
                "Components" + Path.DirectorySeparatorChar +
                "Interfaces" + Path.DirectorySeparatorChar +
                "I" + data.ComponentName() + "Entity.cs",
                template.Replace(data, string.Empty).WrapInNamespace(data.GetNamespace()),
                GetType().FullName
            );
        }

        CodeGenFile generateEntityInterface(string contextName, ComponentData data) {
            var (ns, typeName) = contextName.ExtractNamespace();
            return new CodeGenFile(
                contextName.RemoveDots() + Path.DirectorySeparatorChar +
                "Components" + Path.DirectorySeparatorChar +
                data.ComponentNameWithContext(typeName).AddComponentSuffix() + ".cs",
                ENTITY_INTERFACE_TEMPLATE
                    .Replace("${Namespace}", string.IsNullOrEmpty(data.GetNamespace()) ? string.Empty : data.GetNamespace() + ".")
                    .Replace(data, contextName).WrapInNamespace(ns),
                GetType().FullName
            );
        }
    }
}
