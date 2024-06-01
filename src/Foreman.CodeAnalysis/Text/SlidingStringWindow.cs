
using Foreman.Core;

namespace Foreman.CodeAnalysis.Text;

public class SlidingStringWindow : SlidingWindow<char, StringSpan> {
    private readonly MultiLineString _mls;

    public SlidingStringWindow(MultiLineString mls) {
        _mls = mls;
    }

    public override StringSpan Build(int rangeStart, int rangeEnd) {
        for (int line = 0, delta = 0; line < _mls.LineCount; line += 1) {
            int lineLength = _mls.GetLineLength(line);
            if (delta + lineLength > rangeStart) {
                if (delta + lineLength < rangeEnd) {
                    throw new InvalidCastException("Unable to cast multi line span as StringSpan");
                }

                return new() {
                    AbsolutePosition = rangeStart,
                    Line = line,
                    Start = rangeStart - delta,
                    End = rangeEnd - delta
                };
            }

            delta += lineLength;
        }

        throw new InvalidOperationException("Failed catastrophically to construct a StringSpan");
    }

    public override char Get(int absolutePosition) {
        try {
            return _mls.CharAt(absolutePosition);
        } catch (IndexOutOfRangeException) {
            return '\0';
        }
    }
}
