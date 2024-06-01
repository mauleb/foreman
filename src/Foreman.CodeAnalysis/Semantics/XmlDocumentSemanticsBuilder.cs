using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Semantics;

public static class XmlDocumentSemanticsBuilder {
    public static IEnumerable<SemanticToken> GetSemanticTokens(MultiLineString mls, SyntaxBase? node) {
        if (node == null) {
            return [];
        }

        switch (node.Kind) {
            case SyntaxKind.Comment:
                return node.Span.Explode(mls, SemanticTokenKind.Comment);
            case SyntaxKind.ClosingTag:
                return node.Span.Explode(mls, SemanticTokenKind.Element);
            case SyntaxKind.NamespacedSymbol:
                return node.Span.Explode(mls, SemanticTokenKind.Variable);
            case SyntaxKind.InterpolatedSymbol:
                return GetInterpolatedSymbolTokens(mls, (InterpolatedSymbolSyntax)node);
            case SyntaxKind.Attribute:
                return GetXmlAttributeTokens(mls, (AttributeSyntax)node);
            case SyntaxKind.Element:
                return GetXmlElementTokens(mls, (ElementSyntax)node);
            default:
                break;
        }

        return node.Children
            .Where(child => child != null)
            .Cast<SyntaxBase>()
            .SelectMany(child => GetSemanticTokens(mls, child));
    }

    private static IEnumerable<SemanticToken> GetXmlElementTokens(MultiLineString mls, ElementSyntax node)
    {
        return [
            ..GetSemanticTokens(mls, node.LeadingTrivia),
            ..node.OpenOpenTag?.Span.Explode(mls, SemanticTokenKind.Element),
            ..node.ElementName?.Span.Explode(mls, SemanticTokenKind.Element),
            ..GetSemanticTokens(mls, node.Attributes),
            ..node.CloseOpenTag?.Span.Explode(mls, SemanticTokenKind.Element),
            ..GetSemanticTokens(mls, node.NestedElements),
            ..GetSemanticTokens(mls, node.CloseTag)
        ];
    }


    private static IEnumerable<SemanticToken> GetInterpolatedSymbolTokens(MultiLineString mls, InterpolatedSymbolSyntax node) {
        return [
            ..node.OpenBlock.Span.Explode(mls, SemanticTokenKind.InterpolatedBlock),
            ..GetSemanticTokens(mls, node.Symbol),
            ..node.CloseBlock?.Span.Explode(mls, SemanticTokenKind.InterpolatedBlock)
        ];
    }


    private static IEnumerable<SemanticToken> GetXmlAttributeTokens(MultiLineString mls, AttributeSyntax node) {
        DocumentSpan labelSpan = node.AttributeName.Span;
        foreach (var token in labelSpan.Explode(mls, SemanticTokenKind.Attribute)) {
            yield return token;
        }

        if (node.OpenBlock == null) {
            yield break;
        }

        DocumentSpan? increment = node.OpenBlock.Span;
        foreach (var child in node.Contents?.Nodes ?? []) {
            if (child.Kind == SyntaxKind.InterpolatedSymbol) {
                if (increment != null) {
                    foreach(var token in increment.Explode(mls, SemanticTokenKind.String)) {
                        yield return token;
                    }
                    increment = null;
                }

                foreach (var token in GetSemanticTokens(mls, child)) {
                    yield return token;
                }
            } else if (increment == null) {
                increment = child.Span;              
            } else {
                increment += child.Span;
            }
        }

        if (node.CloseBlock != null) {
            increment = increment != null
                ? increment + node.CloseBlock.Span
                : node.CloseBlock.Span;
        }

        foreach(var token in increment!.Explode(mls, SemanticTokenKind.String)) {
            yield return token;
        }
    }


    private static IEnumerable<SemanticToken> Explode(this DocumentSpan span, MultiLineString mls, SemanticTokenKind kind) {
        if (span.StartLine == span.EndLine) {
            yield return new() {
                Kind = kind,
                Line = span.StartLine,
                StartPosition = span.StartPosition,
                EndPosition = span.EndPosition
            };
            yield break;
        }

        for (int line = span.StartLine; line <= span.EndLine; line += 1) {
            int startPosition = line == span.StartLine
                ? span.StartPosition
                : 0;
            int endPosition = line == span.EndLine
                ? span.EndPosition
                : mls.GetLineLength(line) - 1;

            yield return new() {
                Kind = kind,
                Line = line,
                StartPosition = startPosition,
                EndPosition = endPosition
            };
        }
    }

}