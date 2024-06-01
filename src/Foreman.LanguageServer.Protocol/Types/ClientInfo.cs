namespace Foreman.LanguageServer.Protocol.Types;

public record ClientInfo {
    public required string Name { get; init; }
    public string? Version { get; init; }
}
