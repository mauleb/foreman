namespace Foreman.LanguageServer.Protocol.Requests;

public record AnonymousRequest
{
    public required string Method { get; init; }

    public long? Id { get; init; }
}
