using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobVariableTarget {
    [XmlAttribute("evalPath")]
    public required string EvalPath { get; init; }
    [XmlAttribute("type")]
    public required string TargetType { get; init; }
    [XmlAttribute("id")]
    public required string Id { get; init; }
    [XmlAttribute("index")]
    public string? Index { get; init; }
    public bool Disabled { get; set; } = false;
}
