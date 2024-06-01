using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record ContentSyntax : SequenceSyntax {
    public override SyntaxKind Kind => SyntaxKind.Content;
}

public partial class XmlDocumentParser {
    private static ContentSyntax? ParseContent(XmlDocumentParsingContext context, SyntaxTokenKind[] eoc) {
        List<SyntaxBase> contents = [];
        while (true) {
            if (context.MatchAny(eoc) || context.Tokens.Current().Kind == SyntaxTokenKind.EOF) {
                SequenceSyntax? sequence = context.BuildSequence(contents);
                return sequence != null
                    ? new() { Nodes = sequence.Nodes }
                    : null;
            } else if (context.MatchAny([SyntaxTokenKind.AtOpenBrace])) {
                InterpolatedSymbolSyntax? interpolated = ParseInterpolatedSymbol(context);
                if (interpolated == null) {
                    throw new Exception("Failed catastrophically to parse interpolated content");
                }
                contents.Add(interpolated);
            } else {
                contents.Add(context.ConsumeSingle());
            }
        }
    }
}