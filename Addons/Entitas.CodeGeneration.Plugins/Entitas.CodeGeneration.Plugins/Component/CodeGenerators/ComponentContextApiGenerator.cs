using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class ComponentContextApiGenerator : AbstractGenerator {

        public override string name { get { return "Component (Context API)"; } }

        const string STANDARD_TEMPLATE =
            @"public partial class ${ShortContextType} {

    public ${EntityType} ${componentName}Entity { get { return GetGroup(${MatcherType}.${ComponentName}).GetSingleEntity(); } }
    public ${ComponentType} ${validComponentName} { get { return ${componentName}Entity.${componentName}; } }
    public bool has${ComponentName} { get { return ${componentName}Entity != null; } }

    public ${EntityType} Set${ComponentName}(${newMethodParameters}) {
        if (has${ComponentName}) {
            throw new Entitas.EntitasException(""Could not set ${ComponentName}!\n"" + this + "" already has an entity with ${ComponentType}!"",
                ""You should check if the context already has a ${componentName}Entity before setting it or use context.Replace${ComponentName}()."");
        }
        var entity = CreateEntity();
        entity.Add${ComponentName}(${newMethodArgs});
        return entity;
    }

    public void Replace${ComponentName}(${newMethodParameters}) {
        var entity = ${componentName}Entity;
        if (entity == null) {
            entity = Set${ComponentName}(${newMethodArgs});
        } else {
            entity.Replace${ComponentName}(${newMethodArgs});
        }
    }

    public void Remove${ComponentName}() {
        ${componentName}Entity.Destroy();
    }
}
";

        const string FLAG_TEMPLATE =
            @"public partial class ${ShortContextType} {

    public ${EntityType} ${componentName}Entity { get { return GetGroup(${MatcherType}.${ComponentName}).GetSingleEntity(); } }

    public bool ${prefixedComponentName} {
        get { return ${componentName}Entity != null; }
        set {
            var entity = ${componentName}Entity;
            if (value != (entity != null)) {
                if (value) {
                    CreateEntity().${prefixedComponentName} = true;
                } else {
                    entity.Destroy();
                }
            }
        }
    }
}
";

        public override CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<ComponentData>()
                .Where(d => d.ShouldGenerateMethods())
                .Where(d => d.IsUnique())
                .SelectMany(generate)
                .ToArray();
        }

        CodeGenFile[] generate(ComponentData data) {
            return data.GetContextNames()
                .Select(contextName => generate(contextName, data))
                .ToArray();
        }

        CodeGenFile generate(string contextName, ComponentData data) {
            var template = data.GetMemberData().Length == 0
                ? FLAG_TEMPLATE
                : STANDARD_TEMPLATE;

            var (ns, typeName) = contextName.ExtractNamespace();
            return new CodeGenFile(
                contextName.RemoveDots() + Path.DirectorySeparatorChar +
                "Components" + Path.DirectorySeparatorChar +
                data.ComponentNameWithContext(typeName).AddComponentSuffix() + ".cs",
                template.Replace(data, contextName).WrapInNamespace(ns),
                GetType().FullName
            );
        }
    }
}
