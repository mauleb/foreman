using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Handlers.Notifications;

public class DidOpen : NotificationHandlerBase<DidOpenTextDocumentNotification>
{
    private readonly DocumentStore _documentStore;

    public DidOpen(DocumentStore documentStore)
    {
        _documentStore = documentStore;
    }


    public override async Task Handle(DidOpenTextDocumentNotification notification)
    {
        if (notification.Params == null) {
            return;
        }

        _documentStore.StoreDocument(
            uri: notification.Params.TextDocument.Uri,
            contents: notification.Params.TextDocument.Text
        );

        await _documentStore.PublishDiagnosticsAsync(
            uri: notification.Params.TextDocument.Uri
        );
    }

}