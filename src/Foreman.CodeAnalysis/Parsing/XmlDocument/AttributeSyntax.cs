using System.Collections.Immutable;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record AttributeSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.Attribute;
    public override IEnumerable<SyntaxBase?> Children => [AttributeName,Equal,OpenBlock,Contents,CloseBlock];
    public required PartialSymbolSyntax AttributeName { get; init; }
    public SingleSyntax? Equal { get; init; }
    public SingleSyntax? OpenBlock { get; init; }
    public ContentSyntax? Contents { get; init; }
    public SingleSyntax? CloseBlock { get; init; }
}

public partial class XmlDocumentParser {
    public static AttributeSyntax? ParseAttribute(XmlDocumentParsingContext context) {
        PartialSymbolSyntax? attributeName = ParsePartialSymbol(context);
        if (attributeName == null) {
            return null;
        }

        // myAttr
        AttributeSyntax attribute = new() { AttributeName = attributeName };

        if (!context.Match(SyntaxTokenKind.Equal)) {
            context.ReportMissingAttributeAssignment(attribute);
            return attribute;
        }
        // myAttr=
        attribute = attribute with { Equal = context.ConsumeSingle() };

        if (!context.Match(SyntaxTokenKind.Quote)) {
            // myAttr=BAD
            context.ReportMissingAttributeAssignment(attribute);
            return attribute;
        }
        // myAttr="
        attribute = attribute with { OpenBlock = context.ConsumeSingle() };

        if (context.Match(SyntaxTokenKind.Quote)) {
            // myAttr=""
            attribute = attribute with { CloseBlock = context.ConsumeSingle() };
            return attribute;
        }

        if (context.MatchAny([SyntaxTokenKind.EOF, SyntaxTokenKind.NewLine, SyntaxTokenKind.Unknown])) {
            // myAttr="BAD
            context.ReportNonTerminatedString(attribute);
            return attribute;
        }

        ContentSyntax? contents = ParseContent(context, [
            SyntaxTokenKind.Quote,
            SyntaxTokenKind.EOF,
            SyntaxTokenKind.NewLine,
            SyntaxTokenKind.Unknown,
            SyntaxTokenKind.ForwardSlashCloseBracket,
            SyntaxTokenKind.CloseBracket
        ]);

        attribute = attribute with {
            Contents = contents,
            CloseBlock = context.Match(SyntaxTokenKind.Quote)
                ? context.ConsumeSingle()
                : null
        };

        if (attribute.CloseBlock == null) {
            context.ReportNonTerminatedString(attribute);
        }

        return attribute;
    }
}
