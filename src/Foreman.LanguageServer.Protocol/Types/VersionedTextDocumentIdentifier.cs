namespace Foreman.LanguageServer.Protocol.Types;

public record VersionedTextDocumentIdentifier : TextDocumentIdentifier {
    public required long Version { get; init; }
}
