using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanTemplateRef {
    [XmlAttribute("relativePath")]
    public required string RelativePath { get; init; }
}
