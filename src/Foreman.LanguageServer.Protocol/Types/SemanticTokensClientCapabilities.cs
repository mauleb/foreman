namespace Foreman.LanguageServer.Protocol.Types;

public record SemanticTokensClientCapabilities {
    public bool? OverlappingTokenSupport { get; init; }
    public bool? MultilineTokenSupport { get; init; }
    // TODO
}
