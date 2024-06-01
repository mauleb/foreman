namespace Foreman.LanguageServer.Protocol.Types;

public record Position {
    public required ulong Line { get; init; }
    public required ulong Character { get; init; }
}