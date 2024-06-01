namespace Foreman.LanguageServer.Protocol.Types;

public record Range {
    public required Position Start { get; init; }
    public required Position End { get; init; }
}
