namespace Foreman.LanguageServer.Protocol.Types;

public record WorkspaceEditClientCapabilities {
    public bool? DocumentChanges { get; init; }
    public string[]? ResourceOperations { get; init; }
    public string? FailureHandling { get; init; }
    public bool? NormalizeLineEndings { get; init; }
    // TODO
}
