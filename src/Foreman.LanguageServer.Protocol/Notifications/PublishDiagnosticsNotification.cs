namespace Foreman.LanguageServer.Protocol.Notifications;

public record PublishDiagnosticsNotification : BaseNotification<PublishDiagnosticsParams> {}

public record PublishDiagnosticsParams {
    public required string Uri { get; init; }
    public required Types.Diagnostic[] Diagnostics { get; init; }

    public PublishDiagnosticsNotification Wrap() {
        return new() {
            Method = "textDocument/publishDiagnostics",
            Params = this
        };
    }
}