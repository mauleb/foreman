using System.Collections.Immutable;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record NamespacedSymbolSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.NamespacedSymbol;
    public ImmutableArray<SyntaxBase> Nodes { get; init; } = [];
    public override IEnumerable<SyntaxBase?> Children => Nodes;
}

public partial class XmlDocumentParser {
    public static NamespacedSymbolSyntax? ParseNamespacedSymbol(XmlDocumentParsingContext context) {
        NamespacedSymbolSyntax? namespacedSymbol = null;
                
        CompoundSymbolSyntax? first = ParseCompoundSymbol(context);
        if (first == null) {
            return namespacedSymbol;
        }

        namespacedSymbol = new();

        List<SyntaxBase> nodes = [first];
        while (context.Match(SyntaxTokenKind.ForwardSlash)) {
            SingleSyntax delimiter = context.ConsumeSingle();
            nodes.Add(delimiter);

            CompoundSymbolSyntax? next = ParseCompoundSymbol(context);
            if (next == null) {
                context.ReportIncompleteNamespacedSymbol(delimiter);
            } else {
                nodes.Add(next);
            }
        }

        namespacedSymbol = namespacedSymbol with {
            Nodes = nodes.ToImmutableArray()
        };

        if (nodes.Count == 1) {
            context.ReportMissingNamespace(namespacedSymbol);
        }

        return namespacedSymbol;
    }
}
