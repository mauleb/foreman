using Foreman.CodeAnalysis.Binding;
using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Binding.BinderTests;

public class BindElement {
    [Fact]
    public void Should_BindEmptyInline() {
        MultiLineString text = new("<wow      />");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow/>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindWithAttributes() {
        MultiLineString text = new("<wow\nmyAttr=\"cool\"\nanother=\"some-prefix-@{values/a}@\"/>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();
        variables.SetVariable("values","a","pulled");

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow myAttr=\"cool\" another=\"some-prefix-pulled\"/>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindElementsWithContent() {
        MultiLineString text = new("<wow>hello</wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow>hello</wow>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindElementsWithInterpolatedContent() {
        MultiLineString text = new("<wow>hello, @{inputs/who}@!</wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);
        variables.SetVariable("inputs","who","world");

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow>hello, world!</wow>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindWithCollapsedContent() {
        MultiLineString text = new("<wow></wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow/>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }
    
    [Fact]
    public void Should_BindWithNestedElements() {
        MultiLineString text = new("<wow><cool><a/></cool></wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);
        VariableValueBag variables = new();

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);

        BoundElement elem = (BoundElement)boundNode;
        Assert.Equal("wow", elem.OpenTag.Data);
        Assert.Equal("<wow><cool><a/></cool></wow>", elem.Serialize(variables));
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_DislikeDuplicateAttributes() {
        MultiLineString text = new("<wow a=\"a\" a=\"b\" />");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);
        Assert.NotEmpty(context.Diagnostics);
    }

    [Fact]
    public void Should_DislikeNonMatchingTags() {
        MultiLineString text = new("<a></b>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);
        Assert.NotEmpty(context.Diagnostics);
    }

    [Fact]
    public void Should_CaptureAttributeSymbols() {
        MultiLineString text = new("<wow attr=\"@{cool/beans}@\"/>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);
        Assert.Empty(context.Diagnostics);
        Assert.Contains(
            new VariableIdentifier("cool","beans"),
            context.VariableReferences.Identifiers
        );
    }

    [Fact]
    public void Should_CaptureValueContentSymbols() {
        MultiLineString text = new("<wow>hello @{cool/beans}@</wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);
        Assert.Empty(context.Diagnostics);
        Assert.Contains(
            new VariableIdentifier("cool","beans"),
            context.VariableReferences.Identifiers
        );
    }

    [Fact]
    public void Should_CaptureNestedSymbols() {
        MultiLineString text = new("<a><b x=\"@{value/one}@\" /><b x=\"@{value/two}@\" /><b><c x=\"@{value/three}@\" /></b></a>");
        XmlDocumentParsingContext parsingContext = new(text);
        ElementSyntax? syntax = XmlDocumentParser.ParseElement(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundElement>(boundNode);
        Assert.Empty(context.Diagnostics);

        string[] expectedKeys = ["one","two","three"];
        foreach (string key in expectedKeys) {
            Assert.Contains(
                new VariableIdentifier("value",key),
                context.VariableReferences.Identifiers
            );
        }
    }
}