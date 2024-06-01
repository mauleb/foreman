using Foreman.CodeAnalysis.Binding;
using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Binding.BinderTests;

public class BindNamespacedSymbol {
    [Fact]
    public void Should_BindBasicSymbols() {
        MultiLineString text = new("very/cool");
        XmlDocumentParsingContext parsingContext = new(text);
        NamespacedSymbolSyntax? syntax = XmlDocumentParser.ParseNamespacedSymbol(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundSymbol>(boundNode);

        BoundSymbol symbol = (BoundSymbol)boundNode;
        Assert.Equal("very", symbol.Namespace.Data);
        Assert.Equal("cool", symbol.Key.Data);
        Assert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Should_BindLongSymbols() {
        MultiLineString text = new("a/b/c/d/e/f/g");
        XmlDocumentParsingContext parsingContext = new(text);
        NamespacedSymbolSyntax? syntax = XmlDocumentParser.ParseNamespacedSymbol(parsingContext);

        Assert.NotNull(syntax);

        BindingContext context = new(text);
        BoundNodeBase? boundNode = Binder.Bind(context, syntax);
        Assert.NotNull(boundNode);
        Assert.IsType<BoundSymbol>(boundNode);

        BoundSymbol symbol = (BoundSymbol)boundNode;
        Assert.Equal("a/b/c/d/e/f", symbol.Namespace.Data);
        Assert.Equal("g", symbol.Key.Data);
        Assert.Empty(context.Diagnostics);
    }
}