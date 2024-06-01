using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record PartialSymbolSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.PartialSymbol;
    public override IEnumerable<SyntaxBase?> Children => [];
    public required SyntaxToken[] Tokens { get; init; }
    private DocumentSpan? _span;
    public override DocumentSpan Span {
        get {
            if (_span == null) {
                _span = Tokens.GetDocumentSpan();
            }
            return _span;
        }
    }
}

public partial class XmlDocumentParser  {
    public static PartialSymbolSyntax? ParsePartialSymbol(XmlDocumentParsingContext context) {
        if (!context.Match(SyntaxTokenKind.Alpha)) {
            return null;
        }

        SyntaxTokenKind[] alphanumeric = [
            SyntaxTokenKind.Alpha,
            SyntaxTokenKind.Numeric
        ];

        Func<bool> stepForward = () => {
            switch (context.Tokens.Peek(1).Kind) {
                case SyntaxTokenKind.Alpha:
                case SyntaxTokenKind.Numeric:
                    context.Tokens.ShiftRight(1);
                    return true;
                case SyntaxTokenKind.Hyphen:
                    if (context.MatchAny(alphanumeric, offset: 2)) {
                        context.Tokens.ShiftRight(2);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        };
        while(stepForward());

        return new() {
            Tokens = context.ConsumeTokens()
        };
    }
}