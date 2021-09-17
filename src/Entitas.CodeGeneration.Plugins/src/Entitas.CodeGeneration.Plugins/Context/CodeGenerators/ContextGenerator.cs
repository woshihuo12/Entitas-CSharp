using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public class ContextGenerator : ICodeGenerator {

        public string name { get { return "Context"; } }
        public int priority { get { return 0; } }
        public bool runInDryMode { get { return true; } }

        const string TEMPLATE =
            @"public sealed partial class ${ShortContextType} : Entitas.Context<${EntityType}> {

    public ${ShortContextType}()
        : base(
            ${Lookup}.TotalComponents,
            0,
            new Entitas.ContextInfo(
                ""${FullContextName}"",
                ${Lookup}.componentNames,
                ${Lookup}.componentTypes
            ),
            (entity) =>

#if (ENTITAS_FAST_AND_UNSAFE)
                new Entitas.UnsafeAERC(),
#else
                new Entitas.SafeAERC(entity),
#endif
            () => new ${EntityType}()
        ) {
    }
}
";

        public CodeGenFile[] Generate(CodeGeneratorData[] data) {
            return data
                .OfType<ContextData>()
                .Select(generate)
                .ToArray();
        }

        CodeGenFile generate(ContextData data) {
            var contextName = data.GetContextName();
            return new CodeGenFile(
                data.GetFullContextName().RemoveDots() + Path.DirectorySeparatorChar +
                contextName.AddContextSuffix() + ".cs",
                TEMPLATE.Replace(data.GetFullContextName()).WrapInNamespace(data.GetContextNamespace()),
                GetType().FullName
            );
        }
    }
}
