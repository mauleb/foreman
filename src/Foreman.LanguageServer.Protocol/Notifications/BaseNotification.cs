namespace Foreman.LanguageServer.Protocol.Notifications;

public interface ILspNotification {}

public abstract record BaseNotification<TParams> : ILspNotification where TParams : class {
    public required string Method { get; init; }
    public required TParams? Params { get; init; }
}

public record EmptyParams {}