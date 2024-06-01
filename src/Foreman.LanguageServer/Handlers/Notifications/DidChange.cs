using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Handlers.Notifications;

public class DidChange : NotificationHandlerBase<DidChangeTextDocumentNotification>
{
    private readonly DocumentStore _documentStore;

    public DidChange(DocumentStore documentStore)
    {
        _documentStore = documentStore;

    }

    // private void HandleChange(string uri, TextDocumentContentChangeEvent changeEvent) {
    //     _logger.Write("CHANGED: " + uri);
    //     _logger.Write(changeEvent);

    //     DocumentNode? node = _documentStore.GetDocument(uri);
    //     if (node == null || changeEvent.Range == null) {
    //         return;
    //     }

    //     MultiLineSpan span = new() {
    //         StartLine = (int)changeEvent.Range.Start.Line,
    //         StartPosition = (int)changeEvent.Range.Start.Character,
    //         EndLine = (int)changeEvent.Range.End.Line,
    //         EndPosition = (int)changeEvent.Range.End.Character
    //     };

    //     SyntaxTokenBag tokenBag = node.TokenBag.ReplaceSpanWithContents(
    //         span: span,
    //         contents: changeEvent.Text
    //     );

    //     DocumentNode? rebuilt = _documentParser.Parse(tokenBag);
    //     if (rebuilt == null) {
    //         _logger.Write("FAILED TO REBUILD EXISTING: " + uri);
    //         return;
    //     }

    //     _documentStore.StoreDocument(uri, rebuilt);
    // }

    public override async Task Handle(DidChangeTextDocumentNotification notification)
    {
        if (notification.Params == null) {
            return;
        }

        TextDocumentContentChangeEvent @event = notification.Params.ContentChanges.Last();

        _documentStore.StoreDocument(
            uri: notification.Params.TextDocument.Uri,
            contents: @event.Text
        );

        await _documentStore.PublishDiagnosticsAsync(
            uri: notification.Params.TextDocument.Uri
        );
    }

}