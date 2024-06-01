using System.Collections.Immutable;

using Foreman.Core;

namespace Foreman.CodeAnalysis.Text;

public class SyntaxTokenBag : SlidingWindow<SyntaxToken, IEnumerable<SyntaxToken>> {
    private readonly SyntaxToken[] _tokens;
    public ImmutableArray<SyntaxToken> Tokens => _tokens.ToImmutableArray();

    public SyntaxTokenBag(SyntaxToken[] tokens) {
        _tokens = tokens;
    }

    public override SyntaxToken Get(int absolutePosition) {
        int index = Math.Min(absolutePosition, _tokens.Length - 1);
        return _tokens[index];
    }

    public override IEnumerable<SyntaxToken> Build(int rangeStart, int rangeEnd) {
        for (int i = rangeStart; i <= rangeEnd; i += 1) {
            yield return Get(i);
        }
    }

}