using Foreman.CodeAnalysis.Semantics;
using Foreman.CodeAnalysis.Text;

namespace Foreman.LanguageServer;

public static class SemanticExtensions {
    public static Lsp.SemanticTokenDetails AsDetails(this SemanticToken token) => new() {
        Line = (uint)token.Line,
        StartCharacter = (uint)token.StartPosition,
        Length = (uint)(token.EndPosition - token.StartPosition + 1),
        TokenType = token.Kind.GetName(),
        TokenModifiers = []
    };

    public static IEnumerable<uint> Serialize(this IEnumerable<Lsp.SemanticTokenDetails> semanticTokenDetails) => semanticTokenDetails
        .SelectMany<Lsp.SemanticTokenDetails,uint>(token => [
            token.Line,
            token.StartCharacter,
            token.Length,
            SemanticTokenExtensions.GetSemanticTokenIndex(token.TokenType),
            0 // TODO: modifier support
        ]);

    public static IEnumerable<Lsp.SemanticTokenDetails> WithRelativeOffsets(this IEnumerable<Lsp.SemanticTokenDetails> tokens) {
        uint prevLine = 0;
        uint prevChar = 0;
        foreach (Lsp.SemanticTokenDetails details in tokens) {
            uint relLine = details.Line - prevLine;
            prevLine = details.Line;

            uint relChar = relLine > 0 
                ? details.StartCharacter 
                : details.StartCharacter - prevChar;
            prevChar = details.StartCharacter;

            yield return details with {
                Line = relLine,
                StartCharacter = relChar
            };
        }
    }

    public static Lsp.Range AsRange(this DocumentSpan span) => new() {
        Start = new() {
            Line = (ulong)span.StartLine,
            Character = (ulong)span.StartPosition
        },
        End = new() {
            Line = (ulong)span.EndLine,
            Character = (ulong)span.EndPosition + 1
        }
    };

}