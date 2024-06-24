using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobValueFragment {
    [XmlAttribute("value")]
    public string? Value { get; init; }
}
