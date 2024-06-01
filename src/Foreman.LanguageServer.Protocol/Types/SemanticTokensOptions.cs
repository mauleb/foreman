namespace Foreman.LanguageServer.Protocol.Types;

public record SemanticTokensOptions {
    public required SemanticTokensLegend Legend { get; init; }
    public required bool Range { get; init; }
    public required SemanticTokensFullOptions Full { get; init; }
}
