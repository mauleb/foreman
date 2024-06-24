using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobCondition {
    [XmlAttribute("evalPath")]
    public required string EvalPath { get; init; }
    [XmlAttribute("id")]
    public required string Id { get; init; }
    [XmlAttribute("operator")]
    public required string Operator { get; init; }
    [XmlAttribute("operand")]
    public required string Operand { get; init; }
    [XmlAttribute("definitionPath")]
    public required string DefinitionPath { get; init; }
}
