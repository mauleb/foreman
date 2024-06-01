namespace Foreman.CodeAnalysis.Text;

public enum SyntaxTokenKind {
    EOF,
    OpenBracketForwardSlash,
    OpenBracketBang,
    OpenBracket,
    ForwardSlashCloseBracket,
    ForwardSlash,
    CloseBracket,
    Period,
    Comma,
    Equal,
    Quote,
    AtOpenBrace,
    SpecialChar,
    CloseBraceAt,
    HyphenHyphen,
    Hyphen,
    NewLine,
    Alpha,
    Whitespace,
    Numeric,
    Unknown,
    At

}

public record SyntaxToken {
    public required string DocumentId { get; init; }
    public required StringSpan Span { get; init; }
    public required SyntaxTokenKind Kind { get; init; }
}