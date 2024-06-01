namespace Foreman.LanguageServer.Protocol.Types;

public record WorkspaceClientCapabilities {
    public bool? ApplyEdit { get; init; }
    public WorkspaceEditClientCapabilities? WorkspaceEdit { get; init; }
    // TODO
}
