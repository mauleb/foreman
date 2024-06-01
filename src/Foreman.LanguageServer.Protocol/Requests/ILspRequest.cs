namespace Foreman.LanguageServer.Protocol.Requests;

public interface ILspRequest {
    public string Method { get; }
    public long Id { get; }
}
