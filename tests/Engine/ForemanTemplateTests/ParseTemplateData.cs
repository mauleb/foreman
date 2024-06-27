using Foreman.Engine;

namespace Engine.ForemanTemplateTests;

public class ParseTemplateData {
    [Fact]
    public void Should_HandleEmptyState() {
        string data = """
        <template>
            <inputs />
            <nestedTemplates />
        </template>
        """;

        var document = TestDocument.FromData(data);
        var result = ForemanTemplateDefinition.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.Empty(result.Inputs);
    }

    [Fact]
    public void Should_HandleSimpleInputs() {
       string data = """
        <template>
            <inputs>
                <input key="hello" />
                <input key="goodbye" />
            </inputs>
            <nestedTemplates />
        </template>
        """;

        var document = TestDocument.FromData(data);
        var result = ForemanTemplateDefinition.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.Equal(2, result.Inputs.Length);

        string[] inputKeys = result.Inputs
            .Select(inp => inp.Key)
            .ToArray();

        Assert.Contains("hello", inputKeys);
        Assert.Contains("goodbye", inputKeys);
    }

    [Fact]
    public void Should_HandleInputWithAllowedValues() {
        string data = """
        <template>
            <inputs>
                <input key="hello" />
                <input key="constrained" allowedValues="a,b,c" />
            </inputs>
            <nestedTemplates />
        </template>
        """;

        var document = TestDocument.FromData(data);
        var result = ForemanTemplateDefinition.ParseTemplateData(document);

        Assert.NotNull(result);

        ForemanTemplateInput? constrained = result.Inputs
            .FirstOrDefault(inp => inp.Key == "constrained");

        Assert.NotNull(constrained);
        Assert.Contains("a", constrained.EnumerateAllowedValues());
        Assert.Contains("b", constrained.EnumerateAllowedValues());
        Assert.Contains("c", constrained.EnumerateAllowedValues());
    }
}