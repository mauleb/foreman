using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
[XmlRoot("job")]
public record ForemanJobDefinition {
    private string? _jobAlias;
    public string? JobAlias {
        get => _jobAlias;
        init {
            if (value != null && value.ToLower() == "inputs") {
                throw new InvalidDataException("A job was constructed with an protected keyword as its alias: inputs");
            }
            _jobAlias = value;
        }
    }
    public string? JobPath { get; init; }
    public required XmlDocument Definition { get; init; }
    [XmlAttribute("handler")]
    public required string RelativeHandlerPath { get; init; }
    [XmlArray("pendingValues")]
    [XmlArrayItem("value", typeof(ForemanPendingJobValue))]
    public required ForemanPendingJobValue[] PendingValues { get; init; } = [];
    [XmlArray("pendingVariables")]
    [XmlArrayItem("variable", typeof(ForemanPendingJobVariable))]
    public required ForemanPendingJobVariable[] PendingVariables { get; init; } = [];
    [XmlArray("pendingConditions")]
    [XmlArrayItem("condition", typeof(ForemanPendingJobCondition))]
    public required ForemanPendingJobCondition[] PendingConditions { get; init; } = [];
    private static XmlDocument ParseJobDefinition(XmlNode definition) {
        XmlDocument payload = new();
        XmlNode job = payload.CreateNode(XmlNodeType.Element, "job", "");

        foreach (XmlAttribute attribute in definition.Attributes!) {
            var imported = payload.ImportNode(attribute.Clone(), true);
            job.Attributes?.Append((XmlAttribute)imported);
        }

        foreach (XmlNode child in definition.ChildNodes) {
            var imported = payload.ImportNode(child.Clone(), true);
            job.AppendChild(imported);
        }

        payload.AppendChild(job);
        return payload;
    }

    private static readonly XmlSerializer s_serializer = new(typeof(ForemanJobDefinition));
    public static ForemanJobDefinition ParseJobData(XmlDocument document) {
        using XmlReader reader = new XmlNodeReader(document);
        var deserialized = s_serializer.Deserialize(reader);
        if (deserialized == null) {
            throw new InvalidCastException("Unable to deserialize document into a ForemanJobDefiniton");
        }

        XmlNode? definition = document.SelectSingleNode("/job/definition");
        if (definition == null) {
            throw new InvalidCastException("ForemanJobDefinition is missing its definition. Abort.");
        }

        ForemanJobDefinition job = (ForemanJobDefinition)deserialized;
        string filePath = new Uri(document.BaseURI).LocalPath;
        return job with { 
            JobPath = filePath,
            JobAlias = Path.GetFileNameWithoutExtension(filePath),
            Definition = ParseJobDefinition(definition)
        };
    }

    public static ForemanJobDefinition ParseJobData(string filePath) {
        XmlDocument document = new();
        document.Load(filePath);
        return ParseJobData(document);
    }
}
