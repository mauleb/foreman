using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Text;

public class LexerTests {
    [Theory]
    [MemberData(nameof(AlphaTokens))]
    [MemberData(nameof(NumericTokens))]
    [MemberData(nameof(WhitespaceTokens))]
    [MemberData(nameof(ConcatenatedTokens))]
    [MemberData(nameof(StaticTokens))]
    [MemberData(nameof(ExtendingGlobbingPairs))]
    public void Lex_Should_LexToken(string text, SyntaxTokenKind kind) {
        MultiLineString mls = new(text);

        SyntaxTokenBag bag = Lexer.Lex(mls);

        Assert.Equal(2, bag.Tokens.Length);
        Assert.Equal(kind, bag.Tokens[0].Kind);
        Assert.Equal(SyntaxTokenKind.EOF, bag.Tokens[1].Kind);
    }

    [Theory]
    [MemberData(nameof(ImmutableTokenPairs))]
    [MemberData(nameof(DistinctGlobbingPairs))]
    public void Lex_Should_LexPairs(string text, SyntaxTokenKind[] kinds) {
        MultiLineString mls = new(text);

        SyntaxTokenBag bag = Lexer.Lex(mls);

        Assert.Equal(kinds.Length + 1, bag.Tokens.Length);
        
        for (int i = 0; i < kinds.Length; i += 1) {
            Assert.Equal(kinds[i], bag.Tokens[i].Kind);
        }

        Assert.Equal(SyntaxTokenKind.EOF, bag.Tokens[kinds.Length].Kind);
    }

    public static IEnumerable<object[]> ImmutableTokenPairs() {
        object[][] first = [
            ..AlphaTokens(), 
            ..NumericTokens(), 
            ..WhitespaceTokens(), 
            ..ConcatenatedTokens()
        ];

        object[][] second = [..ConcatenatedTokens(), ..StaticTokens()];

        foreach (var left in first) {
            foreach (var right in second) {
                string text = (string)left[0] + (string)right[0];

                yield return [
                    text,
                    new SyntaxTokenKind[] {
                        (SyntaxTokenKind)left[1], 
                        (SyntaxTokenKind)right[1]
                    }
                ];
            }
        }
    }

    public static IEnumerable<object[]> DistinctGlobbingPairs() {
        object[][][] groups = [
            AlphaTokens().ToArray(),
            NumericTokens().ToArray(),
            WhitespaceTokens().ToArray()
        ];

        for (int i = 0; i < groups.Length; i += 1) {
            object[][] second = [
                ..groups[(i + 1) % groups.Length],
                ..groups[(i + 2) % groups.Length]
            ];

            foreach (var left in groups[i]) {
                foreach (var right in second) {
                    string text = (string)left[0] + (string)right[0];

                    yield return [
                        text,
                        new SyntaxTokenKind[] {
                            (SyntaxTokenKind)left[1], 
                            (SyntaxTokenKind)right[1]
                        }
                    ];
                }
            }
        }
    }

    public static IEnumerable<object[]> ExtendingGlobbingPairs() {
        object[][][] groups = [
            AlphaTokens().ToArray(),
            NumericTokens().ToArray(),
            WhitespaceTokens().ToArray()
        ];

        foreach (var group in groups) {
            foreach (var left in group) {
                foreach (var right in group) {
                    string text = (string)left[0] + (string)right[0];
                    if ((SyntaxTokenKind)left[1] != (SyntaxTokenKind)right[1]) {
                        throw new Exception("Woops");
                    }

                    yield return [text, left[1]];
                }
            }
        }
    }

    public static IEnumerable<object[]> AlphaTokens() {
        yield return ["a", SyntaxTokenKind.Alpha];
        yield return ["aaaa", SyntaxTokenKind.Alpha];
        yield return ["aAaAa", SyntaxTokenKind.Alpha];
        yield return ["abcdefghijklmnopqrstuvwxyz", SyntaxTokenKind.Alpha];
    }

    public static IEnumerable<object[]> NumericTokens() {
        yield return ["0", SyntaxTokenKind.Numeric];
        yield return ["00000", SyntaxTokenKind.Numeric];
        yield return ["0123456789", SyntaxTokenKind.Numeric];
    }

    public static IEnumerable<object[]> WhitespaceTokens() {
        yield return [" ", SyntaxTokenKind.Whitespace];
        yield return ["      ", SyntaxTokenKind.Whitespace];
        yield return ["\t \t ", SyntaxTokenKind.Whitespace];
    }

    public static IEnumerable<object[]> ConcatenatedTokens() {
        yield return ["</", SyntaxTokenKind.OpenBracketForwardSlash];
        yield return ["<!", SyntaxTokenKind.OpenBracketBang];
        yield return ["/>", SyntaxTokenKind.ForwardSlashCloseBracket];
        yield return ["@{", SyntaxTokenKind.AtOpenBrace];
        yield return ["}@", SyntaxTokenKind.CloseBraceAt];
        yield return ["--", SyntaxTokenKind.HyphenHyphen];
        yield return ["\r\n", SyntaxTokenKind.NewLine];
    }

    public static IEnumerable<object[]> StaticTokens() {
        yield return ["<", SyntaxTokenKind.OpenBracket];
        yield return ["/", SyntaxTokenKind.ForwardSlash];
        yield return [">", SyntaxTokenKind.CloseBracket];
        yield return [".", SyntaxTokenKind.Period];
        yield return [",", SyntaxTokenKind.Comma];
        yield return ["=", SyntaxTokenKind.Equal];
        yield return ["\"", SyntaxTokenKind.Quote];
        yield return ["-", SyntaxTokenKind.Hyphen];
        yield return ["@", SyntaxTokenKind.At];
        yield return ["}", SyntaxTokenKind.SpecialChar];
        yield return ["{", SyntaxTokenKind.SpecialChar];
        yield return ["!", SyntaxTokenKind.SpecialChar];
        yield return ["#", SyntaxTokenKind.SpecialChar];
        yield return ["$", SyntaxTokenKind.SpecialChar];
        yield return ["%", SyntaxTokenKind.SpecialChar];
        yield return ["^", SyntaxTokenKind.SpecialChar];
        yield return ["&", SyntaxTokenKind.SpecialChar];
        yield return ["*", SyntaxTokenKind.SpecialChar];
        yield return ["(", SyntaxTokenKind.SpecialChar];
        yield return [")", SyntaxTokenKind.SpecialChar];
        yield return ["_", SyntaxTokenKind.SpecialChar];
        yield return ["+", SyntaxTokenKind.SpecialChar];
        yield return ["~", SyntaxTokenKind.SpecialChar];
        yield return ["`", SyntaxTokenKind.SpecialChar];
        yield return ["[", SyntaxTokenKind.SpecialChar];
        yield return ["]", SyntaxTokenKind.SpecialChar];
        yield return ["|", SyntaxTokenKind.SpecialChar];
        yield return [":", SyntaxTokenKind.SpecialChar];
        yield return [";", SyntaxTokenKind.SpecialChar];
        yield return ["?", SyntaxTokenKind.SpecialChar];
        yield return ["\\", SyntaxTokenKind.SpecialChar];
        yield return ["'", SyntaxTokenKind.SpecialChar];
        yield return ["\r", SyntaxTokenKind.NewLine];
        yield return ["\n", SyntaxTokenKind.NewLine];
    }
}