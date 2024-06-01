namespace Foreman.LanguageServer.Protocol.Types;

public record TextDocumentSyncOptions {
    public bool? OpenClose { get; init; }
    public TextDocumentSyncKind? Change { get; init; }
}
