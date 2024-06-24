using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobValue {
    [XmlAttribute("evalPath")]
    public required string EvalPath { get; init; }
    [XmlAttribute("id")]
    public required string Id { get; init; }
    [XmlAttribute("type")]
    public required string ValueType { get; init; }
    [XmlAttribute("target")]
    public required string Target { get; init; }
    [XmlAttribute("attribute")]
    public string? AttributeName { get; init; }
    [XmlElement("fragment", typeof(ForemanPendingJobValueFragment))]
    public required ForemanPendingJobValueFragment[] Fragments { get; init; }
}
