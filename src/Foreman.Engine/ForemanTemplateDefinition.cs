using System.Xml;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Frozen;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
[XmlRoot("template")]
public record ForemanTemplateDefinition {
    public string? TemplatePath { get; init; }

    [XmlArray("inputs")]
    [XmlArrayItem("input", typeof(ForemanTemplateInput))]
    public required ForemanTemplateInput[] Inputs { get; init; }

    [XmlArray("jobs")]
    [XmlArrayItem("ref", typeof(ForemanTemplateRef))]
    public required ForemanTemplateRef[] Jobs { get; init; }

    private static readonly XmlSerializer s_serializer = new(typeof(ForemanTemplateDefinition));
    public static ForemanTemplateDefinition ParseTemplateData(XmlDocument document) {
        using XmlReader reader = new XmlNodeReader(document);
        var deserialized = s_serializer.Deserialize(reader);
        if (deserialized == null) {
            throw new InvalidCastException("Unable to deserialize document into a ForemanTemplateDefinition");
        }

        ForemanTemplateDefinition template = (ForemanTemplateDefinition)deserialized;
        return template with { 
            TemplatePath = new Uri(document.BaseURI).LocalPath
        };
    }

    public static ForemanTemplateDefinition ParseTemplateData(string filePath) {
        XmlDocument document = new();
        document.Load(filePath);
        return ParseTemplateData(document);
    }

    public static ForemanTemplateDefinition Load(string directory) {
        var template = ParseTemplateData(Path.Join(directory,"template.xml"));
        return template;
    }
}

public record NewForemanExecutionOptions {
    public required ForemanTemplateDefinition Template { get; init; }
    public Dictionary<string,string> Inputs { get; init; } = [];
}

public enum ForemanJobStatus {
    Pending,
    Ready
}

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

    private void Debug(string template, params object[] parts) {
        string message = string.Format(template, parts);
        string output = string.Format("[{0}] {1}", JobAlias, message);
        Console.WriteLine(output);
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

    internal async Task<string> InvokeAsync(ForemanExecutionContext context) {
        return JobAlias;
    }
}

public class ForemanExecutionContext {
    private bool _hasExecuted = false;
    private readonly NewForemanExecutionOptions _options;
    private readonly Dictionary<string,ForemanTemplateInput> _missingInputs = [];
    private readonly Dictionary<string,ForemanJobContext> _jobs = [];

    public ForemanExecutionContext(NewForemanExecutionOptions options) {
        _options = options;

        _missingInputs = _options.Template.Inputs
            .Where(input => _options.Inputs.ContainsKey(input.Key) == false)
            .Select(input => new KeyValuePair<string,ForemanTemplateInput>(input.Key, input))
            .ToDictionary();

        string? templateDir = Path.GetDirectoryName(_options.Template.TemplatePath);
        if (templateDir == null) {
            throw new InvalidDataException("Template has undeclared path. Abort.");
        }

        ForemanJobDefinition[] jobDefinitions = _options.Template.Jobs
            .Select(reference => Path.Join(templateDir, reference.RelativePath))
            .Select(ForemanJobDefinition.ParseJobData)
            .ToArray();
        if (jobDefinitions.Any(job => job.JobAlias == null)) {
            throw new InvalidDataException("One or more jobs have undeclared aliases. Abort.");
        }
        _jobs = jobDefinitions
            .Select(job => new KeyValuePair<string,ForemanJobContext>(job.JobAlias!,new(job)))
            .ToDictionary();

        foreach (var job in _jobs.Values) {
            foreach (var @namespace in job.GetVariableNamespaces()) {
                _jobs[@namespace].AddDependentJob(job.JobAlias);
            }
        }
    }

    public IEnumerable<ForemanTemplateInput> GetMissingInputs() {
        foreach (var kvp in _missingInputs) {
            yield return kvp.Value;
        }
    }

    public void SetTemplateInput(string key, string value) {
        _options.Inputs[key] = value;
        if (_missingInputs.ContainsKey(key)) {
            _missingInputs.Remove(key);
        }
    }

    private static readonly CancellationTokenSource s_invocationTokenSource = new();
    public async Task InvokeAsync() {
        if (_hasExecuted) {
            throw new InvalidOperationException("Attempted to re run a execution context. Create a new context using the same template and try again.");
        }

        if (_missingInputs.Count > 0) {
            throw new InvalidOperationException("Attempted to invoke with missing inputs. Abort.");
        }

        _hasExecuted = true;
        int jobsRemaining = _jobs.Count;
        List<Task<string>> runningJobs = [];

        foreach (var job in _jobs.Values) {
            job.ResolveVariables("inputs", _options.Inputs);
            if (job.Status == ForemanJobStatus.Ready) {
                jobsRemaining -= 1;
                Task<string> jobExecution = job.InvokeAsync(this);
                runningJobs.Add(jobExecution);
            }
        }

        while (jobsRemaining > 0 && runningJobs.Count > 0) {
            await Task.WhenAny(runningJobs);
            
            List<Task<string>> nextRound = [];
            foreach (var task in runningJobs) {
                if (task.IsCompleted) {
                    string alias = await task;
                    foreach (var dependentAlias in _jobs[alias].DependentJobs) {
                        _jobs[dependentAlias].ResolveVariables(
                            alias,
                            _jobs[alias].Outputs
                        );

                        if (_jobs[dependentAlias].Status == ForemanJobStatus.Ready) {
                            jobsRemaining -= 1;
                            Task<string> jobExecution = _jobs[dependentAlias].InvokeAsync(this);
                            nextRound.Add(jobExecution);
                        }
                    }
                } else {
                    nextRound.Add(task);
                }
            }

            runningJobs = nextRound;
        }
    }
}