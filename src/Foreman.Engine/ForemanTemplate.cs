using System.Xml;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Frozen;

namespace Foreman.Engine;

public record NewForemanJob {
    public ImmutableArray<string> ContentErrors { get; init; } = ImmutableArray<string>.Empty;

    public static NewForemanJob ParseJobData(XmlDocument document) {
        return new();
    }
}

public record NewForemanInputValue {
    public string? Value { get; init; }
    public string[] AllowedValues { get; init; } = [];
}

public record NewForemanTemplate {
    public FrozenDictionary<string,NewForemanInputValue> Inputs { get; init; } = FrozenDictionary<string, NewForemanInputValue>.Empty;
    public ImmutableArray<string> ContentErrors { get; init; } = ImmutableArray<string>.Empty;

    public static NewForemanTemplate ParseTemplateData(string filePath) {
        XmlDocument document = new();
        document.Load(filePath);
        return ParseTemplateData(document);
    }
    public static NewForemanTemplate ParseTemplateData(XmlDocument document) {
        List<string> parsingErrors = [];
        Dictionary<string, NewForemanInputValue> documentInputs = [];

        foreach (var element in document.SelectNodes("/template/inputs/input")?.AsEnumerable() ?? []) {
            if (!element.GetAttributeValue("key", out string key)) {
                parsingErrors.Add("Input is missing key");
                continue;
            }

            element.GetAttributeValue("allowedValues", out string allowedValueList);
            string[] allowedValues = allowedValueList
                .Split(",")
                .Where(value => !string.IsNullOrEmpty(value))
                .ToArray();
            
            documentInputs[key] = new() {
                Value = null,
                AllowedValues = allowedValues
            };
        }

        return new() {
            Inputs = documentInputs.ToFrozenDictionary(),
            ContentErrors = parsingErrors.ToImmutableArray()
        };
    }

    public static NewForemanTemplate Load(string directory) {
        var template = ParseTemplateData(Path.Join(directory,"template.xml"));
        return template;
    }

    public NewForemanTemplate WithInputValue(string key, string value) {
        if (!ContentErrors.IsEmpty) {
            throw new InvalidOperationException("Unable to manipulate an invalid template");
        }

        Dictionary<string, NewForemanInputValue> inputs = new(Inputs);

        if (inputs[key].AllowedValues.Length > 0 && !inputs[key].AllowedValues.Contains(value)) {
            throw new ArgumentException(
                string.Format("Input value does not match constraints {0}: {1}",
                    string.Join(',',inputs[key].AllowedValues),
                    value
                ),
                key
            );
        }

        inputs[key] = inputs[key] with { Value = value };
        return new() {
            Inputs = inputs.ToFrozenDictionary()
        };
    }
}

public class ForemanTemplate {
    private ForemanTemplateInput[] _inputs = [];
    private ForemanTemplateReference[] _nestedTemplates;
    private ForemanJob[] _jobs;
    private Dictionary<string,string> _inputValues = [];
    public ImmutableArray<ForemanTemplateInput> Inputs
        => _inputs.ToImmutableArray();

    public ForemanExecutionContext BuildExecutionContext() {
        return new(this);
    }

    public void SetInput(ForemanTemplateInput input, string value) {
        if (input.AllowedValues != null && !input.AllowedValues.Contains(value)) {
            throw new ArgumentException("Provided value is invalid", input.Name);
        }
        
        _inputValues[input.Name] = value;
    }

    public async Task Invoke(ForemanExecutionContext? context = null) {
        if (context == null) {
            context = BuildExecutionContext();
        }
        context.LoadJobs(_jobs);
        context.LoadInputs(_inputValues);
        await context.Run();
    }

    public static ForemanTemplate ParseTemplateData(XmlDocument document) {
        ForemanTemplate template = new();
        return template;
    }

    public static ForemanTemplate Load(string directoryPath) {
        ForemanTemplate template = new();

        XmlDocument document = new();
        string templatePath = Path.Join(directoryPath, "template.xml");
        document.Load(templatePath);

        XmlNodeList? inputList = document.SelectNodes("/template/inputs/input");
        if (inputList != null) {
            template._inputs = inputList
                .AsEnumerable()
                .Where(node => node != null)
                .Select(node => ForemanTemplateInput.Load(node!))
                .ToArray();
        }

        XmlNodeList? templateList = document.SelectNodes("/template/nestedTemplates/ref");
        if (templateList != null) {
            template._nestedTemplates = templateList
                .AsEnumerable()
                .Where(node => node != null)
                .Select(node => ForemanTemplateReference.Load(node!))
                .ToArray();
        }

        template._jobs = Directory
            .EnumerateFiles(directoryPath, "jobs/*.xml", SearchOption.AllDirectories)
            .Select(path => new ForemanJob(path))
            .ToArray();

        return template;
    }
}

public class ForemanExecutionContext {
    private Task _scheduler = Task.CompletedTask;
    private readonly object _lock = new();
    private readonly ForemanTemplate _template;
    private Exception? _runtimeException = null;

    public delegate void VariableSet(string key, string value);
    public event VariableSet OnVariableSet = delegate{};

    public delegate void JobUpdated(string alias, ForemanJobStatus status);
    public event JobUpdated OnJobUpdated = delegate{};

    private readonly Dictionary<string, ForemanJob> _jobs = [];
    private readonly Dictionary<string, Task> _pendingTasks = [];
    private readonly Dictionary<string, List<string>> _pendingVariables = [];
    private readonly Dictionary<ForemanJobStatus, HashSet<string>> _state = new() {
        { ForemanJobStatus.Pending, new() },
        { ForemanJobStatus.Ready, new() },
        { ForemanJobStatus.Running, new() },
        { ForemanJobStatus.Completed, new() },
        { ForemanJobStatus.Failed, new() }
    };

    internal ForemanExecutionContext(ForemanTemplate template) {
        _template = template;
    }

    internal void SetException(Exception ex) {
        _runtimeException = ex;
    }

    internal async Task Run() {
        HashSet<string> waitingOn = [
            .._state[ForemanJobStatus.Pending],
            .._state[ForemanJobStatus.Ready]
        ];

        OnJobUpdated += (alias, status) => {
            if (status == ForemanJobStatus.Completed || status == ForemanJobStatus.Failed) {
                waitingOn.Remove(alias);
            }
        };

        RunBatch();

        await Task.Run(async () => {
            while (_runtimeException == null && waitingOn.Count > 0) {
                await Task.Delay(250);
            }

            if (_runtimeException != null) {
                throw _runtimeException;
            }
        });
    }

    private void RunBatch() {
        lock(_lock) {
            int pendingCount = _state[ForemanJobStatus.Pending].Count;
            int readyCount = _state[ForemanJobStatus.Ready].Count;
            int runningCount = _state[ForemanJobStatus.Running].Count;

            if (pendingCount > 0 && (readyCount + runningCount) == 0) {
                _runtimeException = new InvalidProgramException("Encountered a deadlock");
                return;
            }

            for (int i = runningCount; i < 1 && i < readyCount; i += 1) {
                string next = _state[ForemanJobStatus.Ready].First();
                _state[ForemanJobStatus.Ready].Remove(next);

                OnJobUpdated(next, ForemanJobStatus.Running);
                _pendingTasks[next] = _jobs[next].Invoke(this);
                _pendingTasks[next].ContinueWith((_) => {
                    _pendingTasks.Remove(next);
                    _state[ForemanJobStatus.Running].Remove(next);
                    _state[_jobs[next].Status].Add(next);
                    OnJobUpdated(next, _jobs[next].Status);
                    _scheduler.ContinueWith((_) => {
                        RunBatch();
                    });
                });
                _state[ForemanJobStatus.Running].Add(next);
            }
        }
    }

    internal void LoadJobs(ForemanJob[] jobs) {
        foreach (var nextJob in jobs) {
            _jobs[nextJob.Alias] = nextJob;
            _state[nextJob.Status].Add(nextJob.Alias);
            
            foreach (var missingVar in nextJob.MissingVariables) {
                if (!_pendingVariables.ContainsKey(missingVar)) {
                    _pendingVariables[missingVar] = [];
                }

                _pendingVariables[missingVar].Add(nextJob.Alias);
            }
        }
    }

    private void HandleVariableSet(string key, string value) {
        OnVariableSet(key, value);

        List<(string,ForemanJobStatus,ForemanJobStatus)> pendingShifts = [];
        if (_pendingVariables.ContainsKey(key)) {
            foreach (var alias in _pendingVariables[key]) {
                try {
                    (var prevStatus, var newStatus) = _jobs[alias].SetVariable(key, value);
                    if (prevStatus != newStatus) {
                        pendingShifts.Add((alias,prevStatus,newStatus));
                    }
                } catch (Exception ex) {
                    _runtimeException = ex;
                    throw;
                }
            }
        }

        if (pendingShifts.Count > 0) {
            lock (_lock) {
                foreach ((var alias, var prev, var next) in pendingShifts) {
                    _state[prev].Remove(alias);
                    _state[next].Add(alias);
                    OnJobUpdated(alias, next);
                }
            }
        }
    }

    private void HandleVariableSet(KeyValuePair<string,string> kvp)
        => HandleVariableSet(kvp.Key, kvp.Value);

    internal void LoadOutputs(IDictionary<string,string> outputValues) {
        lock (_lock) {
            foreach (var kvp in outputValues) {
                HandleVariableSet(kvp);
            }
        }
    }

    internal void LoadInputs(Dictionary<string,string> inputValues) {
        var expectedInputs = _template.Inputs;

        List<string> missingInputs = [];
        foreach (var input in expectedInputs) {
            if (!inputValues.ContainsKey(input.Name)) {
                missingInputs.Add(input.Name);
            }
        }

        if (missingInputs.Count > 0) {
            throw new InvalidOperationException("Unable to invoke template. Missing required inputs: " + string.Join(", ", missingInputs));
        }

        lock (_lock) {
            foreach (var kvp in inputValues) {
                HandleVariableSet("@inputs/" + kvp.Key, kvp.Value);
            }
        }
    }
}