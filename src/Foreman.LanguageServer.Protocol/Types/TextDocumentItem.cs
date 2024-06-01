namespace Foreman.LanguageServer.Protocol.Types;

public record TextDocumentItem {
    public required string Uri { get; init; }
    public required string LanguageId { get; init; }
    public required long Version { get; init; }
    public required string Text { get; init; }
}
