using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanNestedTemplateRef {
    [XmlAttribute("key")]
    public required string TemplateKey { get; init; }
    [XmlAttribute("type")]
    public required string TemplateType { get; init; }
    [XmlAttribute("relativePath")]
    public string RelativePath { get; init; } = string.Empty;
    
    private ForemanTemplateDefinition ResolveEmbeddedTemplate(string sourcePath) {
        string resolvedPath = Path.Join(sourcePath, RelativePath);
        return ForemanTemplateDefinition.Load(resolvedPath);
    }

    public ForemanTemplateDefinition ResolveTemplate(string sourcePath) {
        return TemplateType switch {
            "embedded" => ResolveEmbeddedTemplate(sourcePath),
            _ => throw new NotImplementedException("Unsupported template type: " + TemplateType)
        };
    }
}