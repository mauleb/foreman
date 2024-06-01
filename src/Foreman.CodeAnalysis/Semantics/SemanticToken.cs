namespace Foreman.CodeAnalysis.Semantics;

public enum SemanticTokenKind : uint {
    Comment,
    Element,
    Variable,
    Attribute,
    String,
    InterpolatedBlock
}

public record SemanticToken {
    public required SemanticTokenKind Kind { get; init; }
    public required int Line { get; init; }
    public required int StartPosition { get; init; }
    public required int EndPosition { get; init; } 
}

public static class SemanticTokenExtensions {
    private static readonly Dictionary<string,uint> _indexes = TokenKinds()
        .Select((kind, index) => new KeyValuePair<string,uint>(kind.GetName(), (uint)index))
        .ToDictionary();

    public static string GetName(this SemanticTokenKind kind) => kind switch {
        SemanticTokenKind.Comment => "comment",
        SemanticTokenKind.Element => "element",
        SemanticTokenKind.Variable => "variableValue",
        SemanticTokenKind.Attribute => "attribute",
        SemanticTokenKind.String => "string",
        SemanticTokenKind.InterpolatedBlock => "variableInterpolation",
        _ => throw new Exception("Undeclared semantic token name")
    };

    public static uint GetSemanticTokenIndex(string token) {
        if (_indexes.ContainsKey(token)) {
            return _indexes[token];
        }

        throw new ArgumentException(nameof(token), "Unrecognizable token name / index: " + token);
    }

    public static SemanticTokenKind[] TokenKinds() => [
        SemanticTokenKind.Comment,
        SemanticTokenKind.Element,
        SemanticTokenKind.Variable,
        SemanticTokenKind.Attribute,
        SemanticTokenKind.String,
        SemanticTokenKind.InterpolatedBlock
    ];
}