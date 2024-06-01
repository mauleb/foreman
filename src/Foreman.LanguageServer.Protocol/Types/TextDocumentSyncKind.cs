namespace Foreman.LanguageServer.Protocol.Types;

public enum TextDocumentSyncKind : int {
    None = 0,
    Full = 1,
    Incremental = 2
}