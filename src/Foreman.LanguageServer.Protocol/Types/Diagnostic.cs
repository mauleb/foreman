namespace Foreman.LanguageServer.Protocol.Types;

public record Diagnostic {
    public required Range Range { get; init; }
    public DiagnosticSeverity? Severity { get; init; }
    public string? Code { get; init; }
    // TODO: codeDescription
    public string? Source { get; init; }
    public required string Message { get; init; }
    // TODO: tags, related info, data
}
