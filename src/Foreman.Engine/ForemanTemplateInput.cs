using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanTemplateInput {
    [XmlAttribute("key")]
    public required string Key { get; init; }
    [XmlAttribute("allowedValues")]
    public string? AllowedValues { get; init; }
    public IEnumerable<string> EnumerateAllowedValues() {
        if (AllowedValues == null) {
            return [];
        }

        string[] items = AllowedValues.Split(",");
        return items;
    }
}
