using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Semantics;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Semantics;

public static partial class CustomAssert {
    public static void AssertValues(this SemanticToken token, int expectedLine, int expectedStart, int expectedEnd, SemanticTokenKind expectedKind) {
        Assert.Equal(expectedLine, token.Line);
        Assert.Equal(expectedStart, token.StartPosition);
        Assert.Equal(expectedEnd, token.EndPosition);
        Assert.Equal(expectedKind, token.Kind);
    }
}

public class XmlDocumentSemanticsBuilderTests {
    [Fact]
    public void GetSemanticTokens_ShouldHandle_SingleLineComment() {
        MultiLineString mls = new("<!-- wow -->");
        XmlDocumentParsingContext context = new(mls);
        CommentSyntax comment = XmlDocumentParser.ParseComment(context)!;

        SemanticToken[] tokens = XmlDocumentSemanticsBuilder
            .GetSemanticTokens(mls, comment)
            .ToArray();

        Assert.Single(tokens);
        tokens[0].AssertValues(0,0,11,SemanticTokenKind.Comment);
    }

    [Fact]
    public void GetSemanticTokens_ShouldHandle_NamespacedSymbol() {
        MultiLineString mls = new("wow/very.good");
        XmlDocumentParsingContext context = new(mls);
        NamespacedSymbolSyntax symbol = XmlDocumentParser.ParseNamespacedSymbol(context)!;

        SemanticToken[] tokens = XmlDocumentSemanticsBuilder
            .GetSemanticTokens(mls, symbol)
            .ToArray();

        Assert.Single(tokens);
        tokens[0].AssertValues(0,0,12,SemanticTokenKind.Variable);
    }
}