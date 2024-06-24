namespace Foreman.Engine;

public record ForemanExecutionOptions {
    public required ForemanTemplateDefinition Template { get; init; }
    public Dictionary<string,string> Inputs { get; init; } = [];
}
