using System.Collections.Immutable;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record ElementSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.Element;
    public override IEnumerable<SyntaxBase?> Children => [
        LeadingTrivia, OpenOpenTag, ElementName, 
        Attributes, CloseOpenTag, NestedElements, CloseTag];
    public required TriviaSyntax LeadingTrivia { get; init; }
    public SingleSyntax? OpenOpenTag { get; init; }
    public SingleSyntax? ElementName { get; init; }
    public SequenceSyntax? Attributes { get; init; }
    public SingleSyntax? CloseOpenTag { get; init; }
    public SequenceSyntax? NestedElements { get; init; }
    public ClosingTagSyntax? CloseTag { get; init; }

    internal readonly static SyntaxTokenKind[] ValidOpenTagTermination = [
        SyntaxTokenKind.CloseBracket,
        SyntaxTokenKind.ForwardSlashCloseBracket
    ];
}

public partial class XmlDocumentParser {
    public static ElementSyntax? ParseElement(XmlDocumentParsingContext context) {
        context.CollectWhitespace();
        TriviaSyntax leadingTrivia = ParseTrivia(context);
        ElementSyntax document = new() { LeadingTrivia = leadingTrivia }; 

        if (!context.Match(SyntaxTokenKind.OpenBracket)) {
            return leadingTrivia.Nodes.Length > 0 ? document : null;
        }

        document = document with { OpenOpenTag = context.ConsumeSingle() };
        context.CollectWhitespace();
        
        if (context.MatchAny(ElementSyntax.ValidOpenTagTermination)) {
            // <> or </>
            document = document with { CloseOpenTag = context.ConsumeSingle() };
            context.ReportMissingElementName(document);
            return document;
        }

        if (context.Match(SyntaxTokenKind.EOF)) {
            // <
            context.ReportNonTerminatedElementTag(document.OpenOpenTag);
            return document;
        }

        while (!context.MatchAny(AnyWhitespace, offset: 1) && !context.MatchAny(ElementSyntax.ValidOpenTagTermination, offset: 1)) {
            context.Tokens.ShiftRight(1);
        }
        document = document with { ElementName = context.ConsumeSingle() };
        if (document.ElementName.Tokens.Length != 1 || document.ElementName.Tokens[0].Kind != SyntaxTokenKind.Alpha) {
            // <alpha[BAD] 
            context.ReportInvalidElementName(document.ElementName);
        }

        SingleSyntax? whitespaceGap = context.CollectWhitespace();

        List<SyntaxBase> attributes = [];
        while (!context.MatchAny(ElementSyntax.ValidOpenTagTermination) && !context.Match(SyntaxTokenKind.EOF)) {
            AttributeSyntax? attribute = ParseAttribute(context);
            if (attribute == null) {
                break;
            }

            if (whitespaceGap == null || whitespaceGap.Tokens.Length == 0) {
                // <elem attr1=""attr2=""
                context.ReportMissingAttributeSpacing(attribute.AttributeName);
            }
            attributes.Add(attribute);

            whitespaceGap = context.CollectWhitespace();
        }
        document = document with { 
            Attributes = context.BuildSequence(attributes)
        };
        
        while (true) {
            if (context.Match(SyntaxTokenKind.EOF)) {
                ImmutableArray<SyntaxBase> fragment = document.Children
                    .Where(c => c != null)
                    .Cast<SyntaxBase>()
                    .ToImmutableArray();
                SequenceSyntax sequence = new() { Nodes = fragment };

                context.ReportNonTerminatedElementTag(sequence);
                // <elem attr=""
                return document;
            }

            if (context.Match(SyntaxTokenKind.ForwardSlashCloseBracket)) {
                // <elem attr="" />
                document = document with { CloseOpenTag = context.ConsumeSingle() };
                return document;
            }

            if (context.Match(SyntaxTokenKind.CloseBracket)) {
                break;
            }

            // <elem attr="" [BAD]
            SingleSyntax? node = context.ConsumeSingle();
            context.ReportInvalidToken(node);
        }

        // <elem attr="">
        document = document with { CloseOpenTag = context.ConsumeSingle() };

        List<SyntaxBase> elementChildren = [];
        while (true) {
            context.CollectWhitespace();
            ElementSyntax? innerDocument = ParseElement(context);
            if (innerDocument == null) {
                break;
            }
            elementChildren.Add(innerDocument);
        }

        if (context.Match(SyntaxTokenKind.EOF)) {
            // <elem attr="">[CHILDREN][EOF]
            document = document with {
                NestedElements = context.BuildSequence(elementChildren)
            };

            ImmutableArray<SyntaxBase> fragment = document.Children
                .Where(c => c != null)
                .Cast<SyntaxBase>()
                .ToImmutableArray();
            SequenceSyntax sequence = new() { Nodes = fragment };

            context.ReportNonTerminatedXmlDocument(sequence);
            return document;
        }

        if (context.Match(SyntaxTokenKind.OpenBracketForwardSlash)) {
            // <elem attr="">[CHILDREN]</elem>
            document = document with {
                NestedElements = context.BuildSequence(elementChildren),
                CloseTag = ParseClosingTag(context)
            };
            return document;
        }

        if (elementChildren.Count > 0) {
            // <elem attr="">???
            while (!context.MatchAny([SyntaxTokenKind.OpenBracketForwardSlash, SyntaxTokenKind.EOF], offset: 1)) {
                context.Tokens.ShiftRight(1);
            }
            SingleSyntax invalidTokens = context.ConsumeSingle();
            if (context.Match(SyntaxTokenKind.OpenBracketForwardSlash)) {
                document = document with {
                    CloseTag = ParseClosingTag(context)
                };
            }

            context.ReportInvalidElementValue(invalidTokens);
            return document;
        }

        // <elem>[VALUE]
        ContentSyntax? contents = ParseContent(context, [
            SyntaxTokenKind.OpenBracketForwardSlash,
            SyntaxTokenKind.EOF,
            SyntaxTokenKind.Unknown
        ]);

        if (contents != null) {
            elementChildren.Add(contents);
        }

        document = document with {
            NestedElements = context.BuildSequence(elementChildren),
            CloseTag = ParseClosingTag(context)
        };

        if (document.CloseTag == null) {
            context.ReportNonTerminatedXmlDocument(document);
        }

        return document;
    }
}