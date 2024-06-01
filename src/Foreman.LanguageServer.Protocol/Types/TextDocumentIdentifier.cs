namespace Foreman.LanguageServer.Protocol.Types;

public record TextDocumentIdentifier {
    public required string Uri { get; init; }
}
