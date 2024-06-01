namespace Foreman.LanguageServer.Protocol.Types;

public record SemanticTokensLegend {
    public required string[] TokenTypes { get; init; }
    public required string[] TokenModifiers { get; init; }
}