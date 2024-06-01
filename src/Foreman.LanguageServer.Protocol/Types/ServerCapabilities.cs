namespace Foreman.LanguageServer.Protocol.Types;

public record ServerCapabilities {
    // TODO
    public TextDocumentSyncOptions? TextDocumentSync { get; init; }
    public SemanticTokensOptions? SemanticTokensProvider { get; init; }
}
