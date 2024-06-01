namespace Foreman.LanguageServer.Protocol.Responses;

public interface ILspResponse {
    public long Id { get; }
    public object? Result { get; }
    public ResponseError? Error { get; }
}

public record BaseResponse<TResult> : ILspResponse where TResult : class, ILspResult {
    public required long Id { get; init; }
    public required TResult? Result { get; init; }
    public ResponseError? Error { get; init; }
    object? ILspResponse.Result => Result;
}

public interface ILspResult {
    internal abstract ILspResponse AsResponse(long id);
}