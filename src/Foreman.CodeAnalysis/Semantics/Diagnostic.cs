using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Semantics;

public record Diagnostic {
    public required string Message { get; init; }
    public required DocumentSpan Span { get; init; }
}