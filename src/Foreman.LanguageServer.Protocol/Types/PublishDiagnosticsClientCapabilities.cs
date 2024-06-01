namespace Foreman.LanguageServer.Protocol.Types;

public record PublishDiagnosticsClientCapabilities {
    public bool? RelatedInformation { get; init; }
    // TODO: tags
    public bool? VersionSuppport { get; init; }
    public bool? CodeDescriptionSupport { get; init; }
    public bool? DataSupport { get; init; }
}
