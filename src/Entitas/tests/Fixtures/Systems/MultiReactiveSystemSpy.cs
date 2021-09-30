using System;
using System.Collections.Generic;
using Entitas;
using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Entities;
using static Entitas.Tests.TestHelper;

public class MultiReactiveSystemSpy : MultiReactiveSystem<IMyEntity, TestContexts>
{
    public int DidExecute { get; protected set; }
    public IEntity[] Entities { get; protected set; }

    public Action<List<IMyEntity>> ExecuteAction;

    public MultiReactiveSystemSpy(TestContexts contexts) : base(contexts) { }

    protected override ICollector[] GetTrigger(TestContexts contexts) => new ICollector[]
    {
        contexts.Context1.CreateCollector(Matcher<IMyEntity>.AllOf(IndexA)),
        contexts.Context2.CreateCollector(Matcher<IMyEntity>.AllOf(IndexA))
    };

    protected override bool Filter(IMyEntity entity) => true;

    protected override void Execute(List<IMyEntity> entities)
    {
        DidExecute += 1;
        Entities = entities?.ToArray();
        ExecuteAction?.Invoke(entities);
    }
}
