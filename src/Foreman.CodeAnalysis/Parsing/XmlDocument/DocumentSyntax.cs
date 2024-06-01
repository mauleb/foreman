using System.Collections.Immutable;

using Foreman.CodeAnalysis.Text;
using Foreman.Core;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record DocumentSyntax : SyntaxBase
{
    public override SyntaxKind Kind => SyntaxKind.Document;
    public override IEnumerable<SyntaxBase?> Children => [..Root.Children, TrailingTrivia];
    public required ElementSyntax Root { get; init; }
    public required TriviaSyntax TrailingTrivia { get; init; }
}

public partial class XmlDocumentParser {
    public static DocumentSyntax? ParseDocument(XmlDocumentParsingContext context) {
        ElementSyntax? element = ParseElement(context);
        if (element == null) {
            return null;
        }

        TriviaSyntax trivia = ParseTrivia(context);

        return new() {
            Root = element,
            TrailingTrivia = trivia
        };
    }
}
