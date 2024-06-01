using System.Xml;

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

        XmlDocument document = new();
        document.LoadXml(data);

        var result = NewForemanTemplate.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.Empty(result.Inputs);
        Assert.Empty(result.ContentErrors);
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

        XmlDocument document = new();
        document.LoadXml(data);

        var result = NewForemanTemplate.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.Empty(result.ContentErrors);
        Assert.Equal(2, result.Inputs.Keys.Length);

        Assert.Contains("hello", result.Inputs.Keys);
        Assert.Contains("goodbye", result.Inputs.Keys);
        foreach (var kvp in result.Inputs) {
            Assert.Null(kvp.Value.Value);
            Assert.Empty(kvp.Value.AllowedValues);
        }
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

        XmlDocument document = new();
        document.LoadXml(data);

        var result = NewForemanTemplate.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.Empty(result.ContentErrors);

        Assert.Contains("constrained", result.Inputs.Keys);
        Assert.Contains("a", result.Inputs["constrained"].AllowedValues);
        Assert.Contains("b", result.Inputs["constrained"].AllowedValues);
        Assert.Contains("c", result.Inputs["constrained"].AllowedValues);
    }

    [Fact]
    public void Should_DeclareErrorsForInvalidInputs() {
        string data = """
        <template>
            <inputs>
                <input />
                <input />
            </inputs>
            <nestedTemplates />
        </template>
        """;

        XmlDocument document = new();
        document.LoadXml(data);

        var result = NewForemanTemplate.ParseTemplateData(document);

        Assert.NotNull(result);
        Assert.NotEmpty(result.ContentErrors);
        Assert.Equal(2, result.ContentErrors.Length);
    }
}