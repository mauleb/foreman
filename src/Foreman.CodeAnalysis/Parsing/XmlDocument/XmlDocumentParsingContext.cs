using System.Collections.Immutable;
using Foreman.CodeAnalysis.Semantics;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public class XmlDocumentParsingContext {
    private readonly List<Diagnostic> _diagnostics = [];

    public XmlDocumentParsingContext(MultiLineString mls) {
        Contents = mls;
        Tokens = Lexer.Lex(mls);
    }

    public MultiLineString Contents { get; init; }
    public SyntaxTokenBag Tokens { get; init; }
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public bool Match(SyntaxTokenKind kind, int offset = 0) => MatchAny([kind], offset: offset);

    public bool MatchAny(SyntaxTokenKind[] kinds, int offset = 0) {
        foreach (SyntaxTokenKind next in kinds) {
            if (Tokens.Peek(offset).Kind == next) {
                return true;
            }
        }

        return false;
    }

    public bool MatchSequence(SyntaxTokenKind[] kinds, int offset = 0) {
        for (int idx = 0; idx < kinds.Length; idx += 1) {
            if (Tokens.Peek(idx + offset).Kind != kinds[idx]) {
                return false;
            }
        }

        return true;
    }

    public SingleSyntax? CollectWhitespace() {
        SyntaxTokenKind[] whitespace = [SyntaxTokenKind.Whitespace, SyntaxTokenKind.NewLine];

        if (!MatchAny(whitespace)) {
            return null;
        }

        while (MatchAny(whitespace, offset: 1)) {
            Tokens.ShiftRight(1);
        }
        return ConsumeSingle();
    }

    public SyntaxToken[] ConsumeTokens() => Tokens.Consume().ToArray();
    public SingleSyntax ConsumeSingle(int shift = 0) => new() {
        Tokens = Tokens.Consume(shift).ToArray()
    };

    public SequenceSyntax? BuildSequence(IEnumerable<SyntaxBase> nodes) {
        ImmutableArray<SyntaxBase> nodeList = nodes.ToImmutableArray();
        if (nodeList.Length == 0) {
            return null;
        }

        return new() { Nodes = nodeList };
    }

    internal void ReportIncompleteCompoundSymbol(SingleSyntax delimiter) {
        string message = string.Format("Incomplete CompoundSymbol. Symbol fragments must be conjoined by '.' characters without ending in one.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = delimiter.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportIncompleteNamespacedSymbol(SingleSyntax delimiter) {
        string message = string.Format("Incomplete NamespacedSymbol. Symbol clauses must be conjoined by '/' characters without ending in one.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = delimiter.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportNonTerminatedInterpolation(InterpolatedSymbolSyntax interpolated) {
        string message = string.Format("Non-terminated interpolation.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = interpolated.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportMissingAttributeAssignment(AttributeSyntax attribute) {
        string message = string.Format("Missing attribute assignment.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = attribute.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportNonTerminatedString(AttributeSyntax attribute) {
        string message = string.Format("Non-terminated string.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = attribute.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportIncompleteXmlCloseTag(ClosingTagSyntax closeTag) {
        string message = string.Format("Incomplete XmlCloseTag. Expected the whitespace agnostic pattern </AAA>.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = closeTag.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportNonTerminatedComment(CommentSyntax comment)
    {
        string message = string.Format("Non-terminated comment.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = comment.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportMissingElementName(ElementSyntax element)
    {
        string message = string.Format("Element is missing name.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = element.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportNonTerminatedElementTag(SyntaxBase node)
    {
       string message = string.Format("Non-terminated element tag.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = node.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportInvalidElementName(SingleSyntax elementName)
    {
        string message = string.Format("Invalid element name.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = elementName.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportMissingAttributeSpacing(PartialSymbolSyntax attributeName)
    {
        string message = string.Format("Missing spacing between attributes.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = attributeName.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportNonTerminatedXmlDocument(SyntaxBase node) {
        string message = string.Format("Non-terminated xml document.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = node.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportInvalidElementValue(SingleSyntax invalidTokens)
    {
        string message = string.Format("Invalid element value");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = invalidTokens.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportInvalidToken(SingleSyntax node)
    {
        string message = string.Format("Invalid token");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = node.Span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportMissingNamespace(NamespacedSymbolSyntax namespacedSymbol)
    {
        string message = string.Format("Declared symbol does not have any namespaces declared.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = namespacedSymbol.Span
        };

        _diagnostics.Add(diagnostic);
    }
}