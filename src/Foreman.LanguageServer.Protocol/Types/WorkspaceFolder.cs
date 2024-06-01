namespace Foreman.LanguageServer.Protocol.Types;

public record WorkspaceFolder {
    public required string Uri { get; init; }
    public required string Name { get; init; }
}