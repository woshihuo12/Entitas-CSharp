using System;
using System.Collections.Generic;
using Entitas;
using Entitas.Tests.Fixtures;
using Entitas.Tests.Fixtures.Entities;
using static Entitas.Tests.TestHelper;

public class MultiTriggeredMultiReactiveSystemSpy : MultiReactiveSystem<IMyEntity, TestContexts>
{
    public int DidExecute { get; protected set; }
    public IEntity[] Entities { get; protected set; }

    public Action<List<IMyEntity>> ExecuteAction;

    public MultiTriggeredMultiReactiveSystemSpy(TestContexts contexts) : base(contexts) { }

    protected override ICollector[] GetTrigger(TestContexts contexts) => new ICollector[]
    {
        contexts.Context1.CreateCollector(Matcher<IMyEntity>.AllOf(IndexA).Added()),
        contexts.Context2.CreateCollector(Matcher<IMyEntity>.AllOf(IndexA).Removed())
    };

    protected override bool Filter(IMyEntity entity) => true;

    protected override void Execute(List<IMyEntity> entities)
    {
        DidExecute += 1;
        Entities = entities?.ToArray();
        ExecuteAction?.Invoke(entities);
    }
}
