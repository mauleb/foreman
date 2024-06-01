using System.Collections.Immutable;

using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record TriviaSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.Trivia;
    public override IEnumerable<SyntaxBase?> Children => Nodes;
    public required ImmutableArray<SyntaxBase> Nodes { get; init; }
}

public partial class XmlDocumentParser {
    public static TriviaSyntax ParseTrivia(XmlDocumentParsingContext context) {
        List<SyntaxBase> nodes = [];

        bool doContinue = true;
        while (doContinue && !context.Match(SyntaxTokenKind.EOF)) {
            SyntaxBase? next;
            
            if (context.Match(SyntaxTokenKind.OpenBracketBang)) {
                next = ParseComment(context);
            } else {
                next = context.CollectWhitespace();
            }

            if (next == null) {
                doContinue = false;
            } else {
                nodes.Add(next);
            }
        }

        return new() { Nodes = nodes.ToImmutableArray() };
    }
}