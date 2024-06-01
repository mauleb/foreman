namespace Foreman.LanguageServer.Protocol.Types;

public record SemanticTokenDetails {
    public required uint Line { get; init; }
    public required uint StartCharacter { get; init; }
    public required uint Length { get; init; }
    public required string TokenType { get; init; }
    public required string[] TokenModifiers { get; init; }
}