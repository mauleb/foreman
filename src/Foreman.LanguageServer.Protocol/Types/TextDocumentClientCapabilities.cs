namespace Foreman.LanguageServer.Protocol.Types;

public record TextDocumentClientCapabilities {
    public SemanticTokensClientCapabilities? SemanticTokens { get; init; }
    public PublishDiagnosticsClientCapabilities? PublishDiagnostics { get; init; }
    // TODO
}
