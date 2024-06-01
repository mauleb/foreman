using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record SingleSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.Single;
    public required SyntaxToken[] Tokens { get; init; }
    public override IEnumerable<SyntaxBase?> Children => [];
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
