using Entitas.CodeGeneration.Plugins;
using NSpec;
using Shouldly;

class describe_ContextDataProvider : nspec {

    void when_providing() {

        it["creates data for each context name"] = () => {
            var names = "Entitas.CodeGeneration.Plugins.Contexts = Input, GameState";
            var provider = new ContextDataProvider();
            provider.Configure(new TestPreferences(names));

            var data = (ContextData[])provider.GetData();

            data.Length.ShouldBe(2);
            data[0].GetContextName().ShouldBe("Input");
            data[1].GetContextName().ShouldBe("GameState");
        };

        it["extracts namespace for each context name"] = () => {
            var names = "Entitas.CodeGeneration.Plugins.Contexts = My.Namespace.Input, YourNamespace.GameState";
            var provider = new ContextDataProvider();
            provider.Configure(new TestPreferences(names));

            var data = (ContextData[])provider.GetData();

            data.Length.ShouldBe(2);
            data[0].GetContextName().ShouldBe("Input");
            data[0].GetContextNamespace().ShouldBe("My.Namespace");

            data[1].GetContextName().ShouldBe("GameState");
            data[1].GetContextNamespace().ShouldBe("YourNamespace");
        };
    }
}
