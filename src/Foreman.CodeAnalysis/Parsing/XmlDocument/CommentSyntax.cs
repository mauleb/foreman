using System.Collections.Immutable;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record CommentSyntax : SyntaxBase
{
    public override SyntaxKind Kind => SyntaxKind.Comment;
    public override IEnumerable<SyntaxBase?> Children => [OpenComment,Contents,CloseComment];
    public required SingleSyntax OpenComment { get; init; }
    public SequenceSyntax? Contents { get; init; }
    public SingleSyntax? CloseComment { get; init; }

    internal static SyntaxTokenKind[] OpenCommentSequence = [
        SyntaxTokenKind.OpenBracketBang,
        SyntaxTokenKind.HyphenHyphen
    ];

    internal static SyntaxTokenKind[] CloseCommentSequence = [
        SyntaxTokenKind.HyphenHyphen,
        SyntaxTokenKind.CloseBracket
    ];
}

public partial class XmlDocumentParser {
    public static CommentSyntax? ParseComment(XmlDocumentParsingContext context) {
        if (!context.MatchSequence(CommentSyntax.OpenCommentSequence)) {
            return null;
        }

        SingleSyntax open = context.ConsumeSingle(shift: 1);
        CommentSyntax comment = new() { OpenComment = open };        

        List<SyntaxBase> contents = [];
        while (!context.Match(SyntaxTokenKind.EOF) && !context.MatchSequence(CommentSyntax.CloseCommentSequence)) {
            contents.Add(context.ConsumeSingle());
        }

        comment = comment with { 
            Contents = context.BuildSequence(contents)
        };

        if (context.Match(SyntaxTokenKind.EOF)) {
            context.ReportNonTerminatedComment(comment);
            return comment;
        }

        comment = comment with {
            CloseComment = context.ConsumeSingle(shift: 1)
        };
        return comment;
    }
}
