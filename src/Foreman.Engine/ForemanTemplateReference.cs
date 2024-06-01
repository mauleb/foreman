using System.Xml;

namespace Foreman.Engine;

public record ForemanTemplateReference {
    public required string RelativePath { get; init; }

    public static ForemanTemplateReference Load(XmlNode node) {
        var relPath = node.Attributes?.GetNamedItem("relativePath");
        return new() { RelativePath = relPath?.InnerText ?? "" };
    }
}
