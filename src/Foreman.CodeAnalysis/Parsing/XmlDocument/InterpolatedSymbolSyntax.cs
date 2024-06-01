using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record InterpolatedSymbolSyntax : SyntaxBase
{
    public override SyntaxKind Kind => SyntaxKind.InterpolatedSymbol;
    public override IEnumerable<SyntaxBase?> Children => [OpenBlock, Symbol, CloseBlock];
    public required SingleSyntax OpenBlock { get; init; }
    public required NamespacedSymbolSyntax? Symbol { get; init; }
    public required SingleSyntax? CloseBlock { get; init; }
}

public partial class XmlDocumentParser {
    public static InterpolatedSymbolSyntax? ParseInterpolatedSymbol(XmlDocumentParsingContext context) {
        if (!context.Match(SyntaxTokenKind.AtOpenBrace)) {
            return null;
        }

        SingleSyntax open = context.ConsumeSingle();
        NamespacedSymbolSyntax? symbol = ParseNamespacedSymbol(context);
        SingleSyntax? close = context.Match(SyntaxTokenKind.CloseBraceAt)
            ? context.ConsumeSingle()
            : null;

        InterpolatedSymbolSyntax interpolated = new() {
            OpenBlock = open,
            Symbol = symbol,
            CloseBlock = close
        };

        if (symbol == null || close == null) {
            context.ReportNonTerminatedInterpolation(interpolated);
        }

        return interpolated;
    }
}
