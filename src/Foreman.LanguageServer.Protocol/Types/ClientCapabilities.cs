namespace Foreman.LanguageServer.Protocol.Types;

public record ClientCapabilities {
    public TextDocumentClientCapabilities? TextDocument { get; init; }
    public WorkspaceClientCapabilities? Workspace { get; init; }
    // TODO
}
