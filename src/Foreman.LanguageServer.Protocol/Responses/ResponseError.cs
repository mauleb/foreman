namespace Foreman.LanguageServer.Protocol.Responses;

public record ResponseError {
    public required int Code { get; init; }
    public required string Message { get; init; }
    public dynamic? Data { get; init; }
}