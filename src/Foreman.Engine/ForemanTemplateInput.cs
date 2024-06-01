using System.Xml;

namespace Foreman.Engine;

public record ForemanTemplateInput {
    public required string Name { get; init; }
    public string[]? AllowedValues { get; init; }

    public static ForemanTemplateInput Load(XmlNode node) {
        var key = node.Attributes?.GetNamedItem("key");
        var input = new ForemanTemplateInput() {
            Name = key?.InnerText ?? ""
        };

        var allowed = node.Attributes?.GetNamedItem("allowedValues");
        if (allowed != null) {
            input = input with {
                AllowedValues = allowed.InnerText.Split(",")
            };
        }

        return input;
    }
}
