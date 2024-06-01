namespace Foreman.LanguageServer.Protocol.Types;

public record SemanticTokensFullOptions {
    public required bool Delta { get; init; }
}
