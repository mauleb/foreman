namespace Foreman.LanguageServer.Protocol.Types;

public record TextDocumentContentChangeEvent {
    public Range? Range { get; init; }
    public ulong? RangeLength { get; init; }
    public required string Text { get; init; }
}
