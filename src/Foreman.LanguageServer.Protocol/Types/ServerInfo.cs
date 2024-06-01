namespace Foreman.LanguageServer.Protocol.Types;

public record ServerInfo {
    public required string Name { get; init; }
    public required string Version { get; init; }
}