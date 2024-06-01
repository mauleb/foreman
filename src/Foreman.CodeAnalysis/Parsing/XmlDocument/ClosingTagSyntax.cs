using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record ClosingTagSyntax : SyntaxBase
{
    public override SyntaxKind Kind => SyntaxKind.ClosingTag;
    public override IEnumerable<SyntaxBase?> Children => [OpenTag,ElementName,CloseTag];
    public required SingleSyntax OpenTag { get; init; }
    public SingleSyntax? ElementName { get; init; }
    public SingleSyntax? CloseTag { get; init; }
}

public partial class XmlDocumentParser {
    public static ClosingTagSyntax? ParseClosingTag(XmlDocumentParsingContext context) {
        if (!context.Match(SyntaxTokenKind.OpenBracketForwardSlash)) {
            return null;
        }

        ClosingTagSyntax closeTag = new() {
            OpenTag = context.ConsumeSingle()
        };

        context.CollectWhitespace();
        if (!context.Match(SyntaxTokenKind.Alpha)) {
            context.ReportIncompleteXmlCloseTag(closeTag);
            return closeTag;
        }
        closeTag = closeTag with { ElementName = context.ConsumeSingle() };

        context.CollectWhitespace();
        if (!context.Match(SyntaxTokenKind.CloseBracket)) {
            context.ReportIncompleteXmlCloseTag(closeTag);
            return closeTag;
        }
        closeTag = closeTag with { CloseTag = context.ConsumeSingle() };
        return closeTag;
    }
}
