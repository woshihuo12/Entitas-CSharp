using System.IO;
using System.Linq;
using DesperateDevs.CodeGeneration;
using DesperateDevs.Utils;
using Entitas.CodeGeneration.Attributes;

namespace Entitas.CodeGeneration.Plugins {

    public class EventSystemGenerator : AbstractGenerator {

        public override string name { get { return "Event (System)"; } }

        const string ANY_TARGET_TEMPLATE =
            @"public sealed class ${Event}EventSystem : Entitas.ReactiveSystem<${FullEntityType}> {

    readonly Entitas.IGroup<${FullEntityType}> _listeners;
    readonly System.Collections.Generic.List<${FullEntityType}> _entityBuffer;
    readonly System.Collections.Generic.List<I${EventListener}> _listenerBuffer;

    public ${Event}EventSystem(Contexts contexts) : base(contexts.${fullContextName}) {
        _listeners = contexts.${contextName}.GetGroup(${FullMatcherType}.${EventListener});
        _entityBuffer = new System.Collections.Generic.List<${FullEntityType}>();
        _listenerBuffer = new System.Collections.Generic.List<I${EventListener}>();
    }

    protected override Entitas.ICollector<${FullEntityType}> GetTrigger(Entitas.IContext<${FullEntityType}> context) {
        return Entitas.CollectorContextExtension.CreateCollector(
            context, Entitas.TriggerOnEventMatcherExtension.${GroupEvent}(${FullMatcherType}.${ComponentName})
        );
    }

    protected override bool Filter(${FullEntityType} entity) {
        return ${filter};
    }

    protected override void Execute(System.Collections.Generic.List<${FullEntityType}> entities) {
        foreach (var e in entities) {
            ${cachedAccess}
            foreach (var listenerEntity in _listeners.GetEntities(_entityBuffer)) {
                _listenerBuffer.Clear();
                _listenerBuffer.AddRange(listenerEntity.${eventListener}.value);
                foreach (var listener in _listenerBuffer) {
                    listener.On${EventComponentName}${EventType}(e${methodArgs});
                }
            }
        }
    }
}
";

        const string SELF_TARGET_TEMPLATE =
            @"public sealed class ${Event}EventSystem : Entitas.ReactiveSystem<${FullEntityType}> {

    readonly System.Collections.Generic.List<I${EventListener}> _listenerBuffer;

    public ${Event}EventSystem(Contexts contexts) : base(contexts.${fullContextName}) {
        _listenerBuffer = new System.Collections.Generic.List<I${EventListener}>();
    }

    protected override Entitas.ICollector<${FullEntityType}> GetTrigger(Entitas.IContext<${FullEntityType}> context) {
        return Entitas.CollectorContextExtension.CreateCollector(
            context, Entitas.TriggerOnEventMatcherExtension.${GroupEvent}(${FullMatcherType}.${ComponentName})
        );
    }

    protected override bool Filter(${FullEntityType} entity) {
        return ${filter};
    }

    protected override void Execute(System.Collections.Generic.List<${FullEntityType}> entities) {
        foreach (var e in entities) {
            ${cachedAccess}
            _listenerBuffer.Clear();
            _listenerBuffer.AddRange(e.${eventListener}.value);
            foreach (var listener in _listenerBuffer) {
                listener.On${ComponentName}${EventType}(e${methodArgs});
            }
        }
    }
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
            return data.GetEventData()
                .Select(eventData => {
                    var methodArgs = data.GetEventMethodArgs(eventData, ", " + (data.GetMemberData().Length == 0
                                                                            ? data.PrefixedComponentName()
                                                                            : getMethodArgs(data.GetMemberData())));

                    var cachedAccess = data.GetMemberData().Length == 0
                        ? string.Empty
                        : "var component = e." + data.ComponentNameValidLowercaseFirst() + ";";

                    if (eventData.eventType == EventType.Removed) {
                        methodArgs = string.Empty;
                        cachedAccess = string.Empty;
                    }

                    var template = eventData.eventTarget == EventTarget.Self
                        ? SELF_TARGET_TEMPLATE
                        : ANY_TARGET_TEMPLATE;

                    var fileContent = template
                        .Replace("${GroupEvent}", eventData.eventType.ToString())
                        .Replace("${filter}", getFilter(data, contextName, eventData))
                        .Replace("${cachedAccess}", cachedAccess)
                        .Replace("${methodArgs}", methodArgs)
                        .Replace(data, contextName, eventData);

                    var (ns, typeName) = contextName.ExtractNamespace();
                    return new CodeGenFile(
                        contextName.RemoveDots() + Path.DirectorySeparatorChar +
                        "Components" + Path.DirectorySeparatorChar +
                        data.ComponentNameWithContext(typeName).AddComponentSuffix() + ".cs",
                        fileContent.WrapInNamespace(ns),
                        GetType().FullName
                    );
                }).ToArray();
        }

        string getFilter(ComponentData data, string contextName, EventData eventData) {
            var filter = string.Empty;
            if (data.GetMemberData().Length == 0) {
                switch (eventData.eventType) {
                    case EventType.Added:
                        filter = "entity." + data.PrefixedComponentName();
                        break;
                    case EventType.Removed:
                        filter = "!entity." + data.PrefixedComponentName();
                        break;
                }
            } else {
                switch (eventData.eventType) {
                    case EventType.Added:
                        filter = "entity.has" + data.ComponentName();
                        break;
                    case EventType.Removed:
                        filter = "!entity.has" + data.ComponentName();
                        break;
                }
            }

            if (eventData.eventTarget == EventTarget.Self) {
                filter += " && entity.has" + data.EventListener(contextName, eventData);
            }

            return filter;
        }

        string getMethodArgs(MemberData[] memberData) {
            return string.Join(", ", memberData
                .Select(info => "component." + info.name)
                .ToArray()
            );
        }
    }
}
