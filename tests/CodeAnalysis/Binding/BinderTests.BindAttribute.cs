using Foreman.CodeAnalysis.Binding;
using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Binding.BinderTests;

public class BindAttribute {
    [Fact]
    public void Should_BindEmptyAttributes() {
        MultiLineString text = new("attr=\"\"");
        XmlDocumentParsingContext parsingContext = new(text);
        AttributeSyntax? syntax = XmlDocumentParser.ParseAttribute(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundAttribute>(boundNode);

        BoundAttribute attr = (BoundAttribute)boundNode;
        Assert.Equal("attr", attr.Key.Data);
        Assert.Equal("attr=\"\"", attr.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindStaticAttributes() {
        MultiLineString text = new("attr=\"myValue\"");
        XmlDocumentParsingContext parsingContext = new(text);
        AttributeSyntax? syntax = XmlDocumentParser.ParseAttribute(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundAttribute>(boundNode);

        BoundAttribute attr = (BoundAttribute)boundNode;
        Assert.Equal("attr", attr.Key.Data);
        Assert.Equal("attr=\"myValue\"", attr.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Theory]
    [InlineData("@{cool/beans}@", "")]
    [InlineData("wow-@{cool/beans}@", "wow-")]
    [InlineData("@{cool/beans}@-wow-@{great/news}@", "-wow-")]
    [InlineData("@{a/b/c/d/e/f/g}@", "")]
    public void Should_BindDynamicValuesWithEmptyStrings(string value, string serializedValue) {
        string contents = string.Format("attr=\"{0}\"", value);
        MultiLineString text = new(contents);
        XmlDocumentParsingContext parsingContext = new(text);
        AttributeSyntax? syntax = XmlDocumentParser.ParseAttribute(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundAttribute>(boundNode);

        BoundAttribute attr = (BoundAttribute)boundNode;
        Assert.Equal("attr", attr.Key.Data);
        Assert.Equal(serializedValue, attr.Value.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Theory]
    [InlineData("@{value/a}@", "a")]
    [InlineData("wow-@{value/a}@", "wow-a")]
    [InlineData("@{value/a}@-wow-@{value/a}@", "a-wow-a")]
    [InlineData("@{value/a}@-wow-@{value/b}@", "a-wow-")]
    public void Should_BindDynamicValues(string value, string serializedValue) {
        string contents = string.Format("attr=\"{0}\"", value);
        MultiLineString text = new(contents);
        XmlDocumentParsingContext parsingContext = new(text);
        AttributeSyntax? syntax = XmlDocumentParser.ParseAttribute(parsingContext);
        VariableValueBag variables = new();
        variables.SetVariable("value","a","a");

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundAttribute>(boundNode);

        BoundAttribute attr = (BoundAttribute)boundNode;
        Assert.Equal("attr", attr.Key.Data);
        Assert.Equal(serializedValue, attr.Value.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_CaptureVariables() {
        MultiLineString text = new("attr=\"@{cool/beans}@\"");
        XmlDocumentParsingContext parsingContext = new(text);
        AttributeSyntax? syntax = XmlDocumentParser.ParseAttribute(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundAttribute>(boundNode);
        Assert.Empty(context.Diagnostics);
        Assert.Contains(
            new VariableIdentifier("cool","beans"),
            context.VariableReferences.Identifiers
        );
    }
}