using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Protocol.Requests;

public record InitializeRequest : BaseRequest<InitializeRequestParams> {}

public record InitializeRequestParams {
    public required int ProcessId { get; init; }
    public ClientInfo? ClientInfo { get; init; }
    public string? Locale { get; init; }
    public string? RootPath { get; init; }
    public string? DocumentUri { get; init; }
    public dynamic? InitializationOptions { get; init; }
    public ClientCapabilities? Capabilities { get; init; }
    public string? Trace { get; init; }
    public WorkspaceFolder[]? WorkspaceFolders { get; init; }
}