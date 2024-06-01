using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Protocol.Notifications;

public record DidChangeTextDocumentNotification : BaseNotification<DidChangeTextDocumentParams> {}

public record DidChangeTextDocumentParams {
    public required VersionedTextDocumentIdentifier TextDocument { get; init; }
    public required TextDocumentContentChangeEvent[] ContentChanges { get; init; }
}