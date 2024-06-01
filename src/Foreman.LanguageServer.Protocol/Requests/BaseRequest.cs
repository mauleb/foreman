namespace Foreman.LanguageServer.Protocol.Requests;

public record BaseRequest<TParams> : ILspRequest where TParams : class {
    public required string Method { get; init; }
    public required long Id { get; init; }
    public required TParams? Params { get; init; }
}
