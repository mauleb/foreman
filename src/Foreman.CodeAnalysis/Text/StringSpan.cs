namespace Foreman.CodeAnalysis.Text;

public record StringSpan {
    public required int AbsolutePosition { get; init; }
    public required int Line { get; init; }
    public required int Start { get; init; }
    public required int End { get; init; }
    public int Length => End - Start + 1;
}