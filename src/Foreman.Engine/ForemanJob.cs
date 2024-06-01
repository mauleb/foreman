using System.Collections;
using System.Collections.Immutable;
using System.Management.Automation;
using System.Text;
using System.Text.Json;
using System.Xml;

using Json.More;

namespace Foreman.Engine;

public enum ForemanJobStatus {
    Pending,
    Ready,
    Running,
    Completed,
    Failed
}

public class ForemanJob {
    private string _alias;
    private string _path;
    private string _handlerPath;
    private ForemanJobStatus _status;
    private XmlDocument _document;
    private Dictionary<string,string> _outputs;

    public ForemanJobStatus Status => _status;
    public string Alias => _alias;
    public ImmutableDictionary<string,string> Outputs => _outputs.ToImmutableDictionary();

    internal ForemanJob(string path) {
        _path = path;
        _alias = Path.GetFileNameWithoutExtension(path);
        _document = new();
        _outputs = [];

        _document.Load(path);
        var jobNode = _document.SelectSingleNode("/job");
        var handlerAttr = jobNode?.Attributes?.GetNamedItem("handler");
        if (handlerAttr == null) {
            throw new InvalidDataException("Job defined at path is malformed, no handler is defined: " + path);
        }
        _handlerPath = Path.Join(Path.GetDirectoryName(path), handlerAttr.InnerText);

        _status = MissingVariables.Length > 0
            ? ForemanJobStatus.Pending
            : ForemanJobStatus.Ready;
    }

    public string[] MissingVariables {
        get {
            var nodes = _document.SelectNodes("/job/pendingVariables/instance");
            return nodes == null
                ? []
                : nodes.AsEnumerable()
                    .Where(node => node != null)
                    .Select(node => node!.Attributes?.GetNamedItem("variable"))
                    .Where(node => node != null)
                    .Select(node => node!.InnerText)
                    .ToArray(); 
        }
    }

    private void HandleVariableTargetValue(XmlNode target, string value) {
        XmlNode? idNode = target.Attributes?.GetNamedItem("id");
        if (idNode == null) {
            throw new InvalidDataException("pendingVariable target[type=value] is missing attribute: id");
        }

        XmlNode? indexNode = target.Attributes?.GetNamedItem("index");
        if (indexNode == null) {
            throw new InvalidDataException("pendingVariable target[type=value] is missing attribute: index");
        }

        XmlNode? fragmentNode = _document.SelectSingleNode($"/job/pendingValues/instance[@id='{idNode.InnerText}']/fragment[{indexNode.InnerText}]");
        if (fragmentNode == null) {
            throw new InvalidDataException("Attempted to resolve a target[type=value] referencing an invalid location: " + idNode.InnerText + "/" + indexNode.InnerText);
        }

        XmlAttribute attr = _document.CreateAttribute("value");
        attr.Value = value;
        fragmentNode.Attributes?.Append(attr);
    }

    internal (ForemanJobStatus,ForemanJobStatus) SetVariable(string key, string value) {
        var initialStatus = Status;

        var instance = _document.SelectSingleNode($"/job/pendingVariables/instance[@variable=\"{key}\"]");
        if (instance == null) {
            return (initialStatus, initialStatus);
        }

        var targets = _document.SelectNodes($"/job/pendingVariables/instance[@variable=\"{key}\"]/target");
        if (targets != null) {
            foreach (XmlNode nextTarget in targets) {
                XmlNode? typeNode = nextTarget.Attributes?.GetNamedItem("type");
                if (typeNode == null) {
                    throw new InvalidDataException("pendingVariable instance is missing attribute 'type'");
                }

                switch (typeNode.InnerText) {
                    case "value":
                        HandleVariableTargetValue(nextTarget, value);
                        break;
                    default:
                        throw new InvalidDataException("pendingVariable instance has unknown type: " + typeNode.InnerText);
                }
            }
        }

        var collection = _document.SelectSingleNode("/job/pendingVariables");
        collection?.RemoveChild(instance);

        if (collection?.ChildNodes.Count == 0) {
            _status = ForemanJobStatus.Ready;
        }

        return (initialStatus, Status);
    }

    private XmlDocument GetPayload() {
        XmlNode? definition = _document.SelectSingleNode("/job/definition");
        if (definition == null) {
            throw new InvalidOperationException("Job defined at path is malformed, no definition is defined: " + _path);
        }

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

        XmlNodeList? pendingValues = _document.SelectNodes("/job/pendingValues/instance");
        if (pendingValues != null) {
            foreach (XmlNode instance in pendingValues) {
                StringBuilder builder = new();
                foreach (XmlNode fragment in instance.ChildNodes) {
                    XmlNode? fragmentValueNode = fragment.Attributes?.GetNamedItem("value");
                    if (fragmentValueNode == null) {
                        throw new InvalidDataException("Unable to build job payload: missing fragment value");
                    }
                    builder.Append(fragmentValueNode.InnerText);
                }
                
                XmlNode? pathNode = instance.Attributes?.GetNamedItem("path");
                if (pathNode == null) {
                    throw new InvalidDataException("Unable to build job payload: missing instance path");
                }

                XmlNode? propNode = instance.Attributes?.GetNamedItem("prop");
                if (propNode == null) {
                    throw new InvalidDataException("Unable to build job payload: missing instance prop");
                }

                XmlNode? declaredNode = payload.SelectSingleNode(pathNode.InnerText);
                if (declaredNode == null) {
                    throw new InvalidDataException($"Unable to build job payload: declared path ({pathNode.InnerText}) does not resolve to a node");
                }

                if (propNode.InnerText == "#text") {
                    // TODO: implement
                    throw new NotImplementedException("text replacement");
                } else {
                    XmlAttribute attr = payload.CreateAttribute(propNode.InnerText);
                    attr.Value = builder.ToString();
                    declaredNode.Attributes?.Append(attr);
                }
            }
        }

        return payload;
    }

    private Dictionary<string,string> ParseOutputs(PSObject obj) {        
        JsonDocument rawContents = obj.BaseObject.ToJsonDocument();
        var data = JsonSerializer.Deserialize<Dictionary<string,object>>(rawContents) ?? [];
        return data
            .Select(kvp => new KeyValuePair<string,string>(
                _alias + "/" + kvp.Key, 
                kvp.Value + ""
            ))
            .ToDictionary();
    }

    internal async Task Invoke(ForemanExecutionContext context) {
        _status = ForemanJobStatus.Running;

        try {
            XmlDocument payload = GetPayload();

            using PowerShell shell = PowerShell.Create();
            shell.AddScript(File.ReadAllText(_handlerPath));
            // TODO: not hardcoded
            shell.AddParameter("Configuration", payload.InnerXml);
            shell.AddParameter("Context", "{}");
            var result = await shell.InvokeAsync();

            _status = shell.Streams.Error.Any()
                ? ForemanJobStatus.Failed
                : ForemanJobStatus.Completed;
            _outputs = ParseOutputs(result.Last());
            context.LoadOutputs(Outputs);

            foreach (var line in shell.Streams.Information) {
                Console.WriteLine(string.Format("[{0}] {1}", line.TimeGenerated, line.MessageData));
            }
        } catch (Exception ex) {
            _status = ForemanJobStatus.Failed;
            context.SetException(ex);
        }
        
    }
}
