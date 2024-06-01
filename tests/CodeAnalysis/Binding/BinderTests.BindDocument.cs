using Foreman.CodeAnalysis.Binding;
using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Binding.BinderTests;

public class BindDocument {
    [Fact]
    public void Should_ExposeVariableReferences() {
        MultiLineString text = new("<job a=\"@{cool/beans}@\"><wow a=\"@{lame/beans}@\"/></job>");
        XmlDocumentParsingContext parsingContext = new(text);
        DocumentSyntax? syntax = XmlDocumentParser.ParseDocument(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundForemanJob>(boundNode);

        BoundForemanJob job = (BoundForemanJob)boundNode;
        Assert.Equal(
            context.VariableReferences.Identifiers.Count(), 
            context.VariableReferences.Identifiers.Intersect(job.VariableReferences.Identifiers).Count()
        );
        Assert.Equal(
            context.VariableReferences.Spans.Count(), 
            context.VariableReferences.Spans.Intersect(job.VariableReferences.Spans).Count()
        );
        Assert.Equal(
            context.VariableReferences.Identifiers.Count(), 
            job.VariableReferences.Identifiers.Intersect(context.VariableReferences.Identifiers).Count()
        );
        Assert.Equal(
            context.VariableReferences.Spans.Count(), 
            job.VariableReferences.Spans.Intersect(context.VariableReferences.Spans).Count()
        );
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_FailWithAnonymousXmlWithEmbeddedSymbols() {
        MultiLineString text = new("<wow a=\"@{cool/beans}@\"><wow a=\"@{lame/beans}@\"/></wow>");
        XmlDocumentParsingContext parsingContext = new(text);
        DocumentSyntax? syntax = XmlDocumentParser.ParseDocument(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundDocument>(boundNode);
        Assert.NotEmpty(context.Diagnostics);
    }
}