


namespace Foreman.CodeAnalysis.Text;

public class Lexer {
    private readonly MultiLineString _mls;
    private readonly SlidingStringWindow _ssw;

    public static SyntaxTokenBag Lex(MultiLineString mls) {
        Lexer lexer = new(mls);
        SyntaxToken[] tokens = lexer.Lex().ToArray();
        return new(tokens);
    }

    private Lexer(MultiLineString mls) {
        _mls = mls;
        _ssw = new(mls);
    }

    private IEnumerable<SyntaxToken> Lex() {
        while (_ssw.Current() != '\0') {
            yield return LexNextToken();
        }

        int eofLine = _mls.LineCount - 1;
        int eofLineLength = _mls.GetLineLength(eofLine);

        yield return new() {
            DocumentId = _mls.DocumentId,
            Kind = SyntaxTokenKind.EOF,
            Span = new() {
                AbsolutePosition = _mls.GetAllLines().Length,
                Line = eofLine,
                Start = eofLineLength,
                End = eofLineLength
            }
        };
    }

    private SyntaxToken BuildToken(SyntaxTokenKind kind, int amount = 1) => new() {
        DocumentId = _mls.DocumentId,
        Kind = kind,
        Span = _ssw.Consume(amount - 1)
    };

    private SyntaxToken LexNextToken() {
        switch (_ssw.Current()) {
            case '<':
                if (_ssw.Peek(1) == '/') {
                    return BuildToken(SyntaxTokenKind.OpenBracketForwardSlash, amount: 2);
                }

                if (_ssw.Peek(1) == '!') {
                    return BuildToken(SyntaxTokenKind.OpenBracketBang, amount: 2);
                }

                return BuildToken(SyntaxTokenKind.OpenBracket);
            case '/':
                if (_ssw.Peek(1) == '>') {
                    return BuildToken(SyntaxTokenKind.ForwardSlashCloseBracket, amount: 2);
                }
                return BuildToken(SyntaxTokenKind.ForwardSlash);
            case '>':
                return BuildToken(SyntaxTokenKind.CloseBracket);
            case '.':
                return BuildToken(SyntaxTokenKind.Period);
            case ',':
                return BuildToken(SyntaxTokenKind.Comma);
            case '=':
                return BuildToken(SyntaxTokenKind.Equal);
            case '"':
                return BuildToken(SyntaxTokenKind.Quote);
            case '@':
                if (_ssw.Peek(1) == '{') {
                    return BuildToken(SyntaxTokenKind.AtOpenBrace, amount: 2);
                }

                return BuildToken(SyntaxTokenKind.At);
            case '}':
                if (_ssw.Peek(1) == '@') {
                    return BuildToken(SyntaxTokenKind.CloseBraceAt, amount: 2);
                }

                return BuildToken(SyntaxTokenKind.SpecialChar);
            case '-':
                if (_ssw.Peek(1) == '-') {
                    return BuildToken(SyntaxTokenKind.HyphenHyphen, amount: 2);
                }

                return BuildToken(SyntaxTokenKind.Hyphen);
            case '{':
            case '!':
            case '#':
            case '$':
            case '%':
            case '^':
            case '&':
            case '*':
            case '(':
            case ')':
            case '_':
            case '+':
            case '~':
            case '`':
            case '[':
            case ']':
            case '|':
            case ':':
            case ';':
            case '?':
            case '\\':
            case '\'':
                return BuildToken(SyntaxTokenKind.SpecialChar);
            case '\r':
                if (_ssw.Peek(1) == '\n') {
                    return BuildToken(SyntaxTokenKind.NewLine, amount: 2);
                }

                return BuildToken(SyntaxTokenKind.NewLine);
            case '\n':
                return BuildToken(SyntaxTokenKind.NewLine);
            default:
                return LexCharacterSet();
        }
    }

    private SyntaxToken LexCharacterSet() {
        char current = _ssw.Current();
        if (char.IsLetter(current))
        {
            return BuildCharacterSetToken(SyntaxTokenKind.Alpha, char.IsLetter);
        }

        if (IsWhiteSpace(current))
        {
            return BuildCharacterSetToken(SyntaxTokenKind.Whitespace, IsWhiteSpace);
        }

        if (char.IsNumber(current))
        {
            return BuildCharacterSetToken(SyntaxTokenKind.Numeric, char.IsNumber);
        }

        // TODO: diagnostic
        return BuildToken(SyntaxTokenKind.Unknown);
    }

    private bool IsWhiteSpace(char c) {
        switch (c) {
            case '\r':
            case '\n':
                return false;
            default:
                return char.IsWhiteSpace(c);
        }
    }

    private SyntaxToken BuildCharacterSetToken(SyntaxTokenKind kind, Func<char, bool> doInclude)
    {
        while (doInclude(_ssw.Peek(1))) {
            _ssw.ShiftRight(1);
        }

        return new() {
            DocumentId = _mls.DocumentId,
            Kind = kind,
            Span = _ssw.Consume()
        };
    }

}