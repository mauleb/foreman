using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobVariable {
    [XmlAttribute("key")]
    public required string VariableKey { get; init; }
    [XmlElement("target", typeof(ForemanPendingJobVariableTarget))]
    public required ForemanPendingJobVariableTarget[] Targets { get; init; }
}
