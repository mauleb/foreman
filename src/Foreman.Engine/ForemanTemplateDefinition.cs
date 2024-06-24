using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
[XmlRoot("template")]
public record ForemanTemplateDefinition {
    public string? TemplatePath { get; init; }

    [XmlArray("inputs")]
    [XmlArrayItem("input", typeof(ForemanTemplateInput))]
    public required ForemanTemplateInput[] Inputs { get; init; }

    [XmlArray("jobs")]
    [XmlArrayItem("ref", typeof(ForemanTemplateRef))]
    public required ForemanTemplateRef[] Jobs { get; init; }

    private static readonly XmlSerializer s_serializer = new(typeof(ForemanTemplateDefinition));
    public static ForemanTemplateDefinition ParseTemplateData(XmlDocument document) {
        using XmlReader reader = new XmlNodeReader(document);
        var deserialized = s_serializer.Deserialize(reader);
        if (deserialized == null) {
            throw new InvalidCastException("Unable to deserialize document into a ForemanTemplateDefinition");
        }

        ForemanTemplateDefinition template = (ForemanTemplateDefinition)deserialized;
        return template with { 
            TemplatePath = new Uri(document.BaseURI).LocalPath
        };
    }

    public static ForemanTemplateDefinition ParseTemplateData(string filePath) {
        XmlDocument document = new();
        document.Load(filePath);
        return ParseTemplateData(document);
    }

    public static ForemanTemplateDefinition Load(string directory) {
        var template = ParseTemplateData(Path.Join(directory,"template.xml"));
        return template;
    }
}
