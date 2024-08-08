using System.Management.Automation;
using System.Text.Json;
using System.Xml;

namespace Foreman.Engine;

public class ForemanJobContext {
    private readonly List<string> _dependentJobs = [];
    private readonly ForemanJobDefinition _job;
    private Dictionary<string,ForemanPendingJobValue> _pendingValues = [];
    private readonly Dictionary<VariableIdentifier,ForemanPendingJobVariable> _pendingVariables = [];
    private Dictionary<string, ForemanPendingJobCondition> _pendingConditions = [];
    public string JobAlias => _job.JobAlias!;
    private ForemanJobStatus? _status = null;
    public ForemanJobStatus Status => _status ?? (
        _pendingVariables.Keys.Count + _pendingConditions.Keys.Count > 0 
            ? ForemanJobStatus.Pending 
            : ForemanJobStatus.Ready
        );
    public string[] DependentJobs => _dependentJobs.ToArray();
    public Dictionary<string,string> Outputs { get; private set; } = [];

    public ForemanJobContext(ForemanJobDefinition job) {
        _job = job;

        _pendingValues = _job.PendingValues
            .Select(val => new KeyValuePair<string,ForemanPendingJobValue>(
                val.Id,
                val
            ))
            .ToDictionary();

        _pendingVariables = _job.PendingVariables
            .Select(var => new KeyValuePair<VariableIdentifier,ForemanPendingJobVariable>(
                VariableIdentifier.Parse(var.VariableKey),
                var
            ))
            .ToDictionary();

        _pendingConditions = _job.PendingConditions
            .Select(cond => new KeyValuePair<string,ForemanPendingJobCondition>(
                cond.Id,
                cond
            ))
            .ToDictionary();
    }

    public IEnumerable<string> GetVariableNamespaces() {
        return _job.PendingVariables
            .Select(var => VariableIdentifier.Parse(var.VariableKey).Namespace)
            .Where(@namespace => @namespace != "inputs")
            .Distinct();
    }

    public void AddDependentJob(string alias) {
        _dependentJobs.Add(alias);
    }

    private void EvaluateValue(string id, string? index, string resolvedValue) {
        ForemanPendingJobValue pendingValue = _pendingValues[id];
        int intIndex = int.Parse(index ?? "-1");

        pendingValue.ResolveFragment(intIndex, resolvedValue);

        if (pendingValue.IsResolved && pendingValue.ValueType == "condition") {
            EvaluateConditional(pendingValue.Target, pendingValue.ResolvedValue);
        }
    }

    private void EvaluateConditional(string id, string resolvedValue) {
        ForemanPendingJobCondition condition = _pendingConditions[id];
        _pendingConditions.Remove(id);

        bool didResolve = condition.Operator switch {
            "is" => condition.Operand == resolvedValue,
            "isNot" => condition.Operand != resolvedValue,
            _ => throw new InvalidOperationException("Unsupported conditional operator: " + condition.Operator)
        };

        Debug("condition at path {0} resolved to {1}", condition.DefinitionPath, didResolve);
        if (didResolve) {
            return;
        }

        string failedEvalPath = string.Format("{0}{1}/", condition.EvalPath, condition.Id);
        
        _pendingValues = _pendingValues
            .Where(kvp => kvp.Value.EvalPath.StartsWith(failedEvalPath) == false)
            .ToDictionary();

        _pendingConditions = _pendingConditions
            .Where(kvp => kvp.Value.EvalPath.StartsWith(failedEvalPath) == false)
            .ToDictionary();

        // NOTE: variable evaluation is what triggers condition evaluations, so clean up needs to be deferred
        foreach (var pendingVariable in _pendingVariables) {
            foreach (var target in pendingVariable.Value.Targets) {
                if (target.EvalPath.StartsWith(failedEvalPath)) {
                    target.Disabled = true;
                }
            }
        }
    }

    private void EvaluateVariableTargets(VariableIdentifier identifier, string value) {
        foreach (var target in _pendingVariables[identifier].Targets) {
            if (target.Disabled) {
                continue;
            }
            
            switch (target.TargetType) {
                case "condition":
                    EvaluateConditional(target.Id, value);
                    break;
                case "value":
                    EvaluateValue(target.Id, target.Index, value);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected variable target type: " + target.TargetType);
            }
        }

        _pendingVariables.Remove(identifier);
    }

    internal void Debug(string template, params object[] parts) {
        string message = string.Format(template, parts);
        string output = string.Format("[{0}] {1}", JobAlias, message);
        Console.WriteLine(output);
    }

    internal void Error(string template, params object[] parts) {
        string message = string.Format(template, parts);
        string output = string.Format("[{0}] {1}", JobAlias, message);
        Console.Error.WriteLine(output);
    }

    public void ResolveVariables(string @namespace, Dictionary<string,string> values) {
        foreach (var kvp in values) {
            VariableIdentifier id = new() {
                Namespace = @namespace,
                Key = kvp.Key
            };

            if (_pendingVariables.ContainsKey(id)) {
                Debug("{0} ~> {1}", id.ToString(), kvp.Value);
                EvaluateVariableTargets(id, kvp.Value);
                _pendingVariables.Remove(id);
            }
        }
    }

    private async Task InvokeAdhocAsync(ForemanExecutionContext context, XmlDocument jobDefinition) {
        if (string.IsNullOrEmpty(_job.RelativeHandlerPath)) {
            Console.WriteLine("Undeclared handlerPath");
            _status = ForemanJobStatus.Failed;
            return;
        }

        try {
            string jobDirectory = Path.GetDirectoryName(_job.JobPath)!;
            string resolvedHandlerPath = Path.Join(jobDirectory, _job.RelativeHandlerPath);
            using PowerShell shell = PowerShell.Create();
            shell.AddScript(File.ReadAllText(resolvedHandlerPath));
            shell.AddParameter("Configuration",jobDefinition.OuterXml);
            shell.AddParameter("Context","{}"); // TODO: real context
            var output = await shell.InvokeAsync();
            var result = output.Last();

            string serialized = JsonSerializer.Serialize(result.BaseObject);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string,object>>(serialized)!;
            Outputs = deserialized
                .Select(kvp => new KeyValuePair<string,string>(kvp.Key, kvp.Value + ""))
                .ToDictionary();
            _status = ForemanJobStatus.Complete;
        } catch (Exception ex) {
            Console.WriteLine(ex);
            _status = ForemanJobStatus.Failed;
        }

        return;
    }

    private async Task InvokeTemplateAsync(ForemanExecutionContext context, XmlDocument jobDefinition) {
        Dictionary<string,string> inputs = [];
        foreach (XmlNode inputNode in jobDefinition.TrySelectNodes("/job/input")) {
            string?[] attrs = inputNode.TrySelectAttributes("key","value");
            if (attrs.Any(x => x is null)) {
                throw new Exception("Encountered incomplete nested template");
            }
            inputs.TryAdd(attrs[0]!,attrs[1]!);
        }

        ForemanExecutionOptions nestedOptions = new() {
            Template = context.ResolveNestedTemplate(_job.JobExecutionKey),
            Inputs = inputs
        };

        ForemanExecutionContext nestedContext = new(nestedOptions);
        if (nestedContext.GetMissingInputs().Any()) {
            throw new Exception("Encountered nested template with missing inputs");
        }

        await nestedContext.InvokeAsync();
        // TODO: template output support
        Outputs = [];
        _status = ForemanJobStatus.Complete;
    }

    internal async Task<string> InvokeAsync(ForemanExecutionContext context) {
        XmlDocument job = _job.Definition;

        foreach (var pendingValue in _pendingValues.Values) {
            if (pendingValue.ValueType == "condition") {
                continue;
            }

            XmlNode? node = job.SelectSingleNode(pendingValue.Target);
            if (node == null) {
                throw new Exception("Attempted to resolve a definition value at a path that does not exist");
            }

            if (pendingValue.AttributeName != null) {
                XmlAttribute attr = job.CreateAttribute(pendingValue.AttributeName);
                attr.Value = pendingValue.ResolvedValue;
                node.Attributes?.Append(attr);
            } else {
                throw new NotImplementedException("Not attribute value injection");
            }
        }

        _status = ForemanJobStatus.Running;

        Task execution = _job.JobExecutionType switch {
            "adhoc" => InvokeAdhocAsync(context, job),
            "template" => InvokeTemplateAsync(context, job),
            _ => throw new ArgumentException("Unknown job execution type: " + _job.JobExecutionType)
        };

        await execution;
        return JobAlias;
    }
}
