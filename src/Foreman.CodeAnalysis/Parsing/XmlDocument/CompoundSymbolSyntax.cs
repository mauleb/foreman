using System.Collections.Immutable;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record CompoundSymbolSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.CompoundSymbol;
    public required ImmutableArray<SyntaxBase> Nodes { get; init; }
    public override IEnumerable<SyntaxBase?> Children => Nodes;
}

public partial class XmlDocumentParser {
    public static CompoundSymbolSyntax? ParseCompoundSymbol(XmlDocumentParsingContext context) {
        PartialSymbolSyntax? first = ParsePartialSymbol(context);
        if (first == null) {
            return null;
        }

        List<SyntaxBase> nodes = [first];
        while (context.Match(SyntaxTokenKind.Period)) {
            SingleSyntax delimiter = context.ConsumeSingle();
            nodes.Add(delimiter);

            PartialSymbolSyntax? next = ParsePartialSymbol(context);
            if (next == null) {
                context.ReportIncompleteCompoundSymbol(delimiter);
            } else {
                nodes.Add(next);
            }
        }

        return new() {
            Nodes = nodes.ToImmutableArray()
        };
    }
}
