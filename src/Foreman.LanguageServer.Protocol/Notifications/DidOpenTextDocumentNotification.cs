using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Protocol.Notifications;

public record DidOpenTextDocumentNotification : BaseNotification<DidOpenTextDocumentParams> {}

public record DidOpenTextDocumentParams {
    public required TextDocumentItem TextDocument { get; init; }
}