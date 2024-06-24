namespace Foreman.Engine;

public class ForemanExecutionContext {
    private bool _hasExecuted = false;
    private readonly ForemanExecutionOptions _options;
    private readonly Dictionary<string,ForemanTemplateInput> _missingInputs = [];
    private readonly Dictionary<string,ForemanJobContext> _jobs = [];

    private int _incompleteJobs;
    private List<Task<string>> _runningJobs = [];

    public ForemanExecutionContext(ForemanExecutionOptions options) {
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
        _incompleteJobs = _jobs.Count;

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

    private void InvokeJob(ForemanJobContext jobContext, List<Task<string>> jobs) {
        if (jobContext.Status != ForemanJobStatus.Ready) {
            throw new Exception("Attempted to invoke a job which is not Ready: " + jobContext.Status);
        }

        _incompleteJobs -= 1;
        Task<string> jobExecution = jobContext.InvokeAsync(this);
        jobs.Add(jobExecution);
        jobContext.Debug("was started");
    }

    public async Task InvokeAsync() {
        if (_hasExecuted) {
            throw new InvalidOperationException("Attempted to re run a execution context. Create a new context using the same template and try again.");
        }

        if (_missingInputs.Count > 0) {
            throw new InvalidOperationException("Attempted to invoke with missing inputs. Abort.");
        }

        _hasExecuted = true;

        foreach (var job in _jobs.Values) {
            job.ResolveVariables("inputs", _options.Inputs);
            if (job.Status == ForemanJobStatus.Ready) {
                InvokeJob(job, _runningJobs);
            }
        }

        while (_incompleteJobs > 0 && _runningJobs.Count > 0) {
            await Task.WhenAny(_runningJobs);
            
            List<Task<string>> nextRound = [];
            foreach (var task in _runningJobs) {
                if (task.IsCompleted) {
                    string alias = await task;

                    if (_jobs[alias].Status == ForemanJobStatus.Failed) {
                        _jobs[alias].Error("has failed");
                        continue;
                    }

                    if (_jobs[alias].Status != ForemanJobStatus.Complete) {
                        _jobs[alias].Error("INVALID TERMINAL STATUS: " + _jobs[alias].Status);
                        continue;
                    }

                    _jobs[alias].Debug("has completed");

                    foreach (var dependentAlias in _jobs[alias].DependentJobs) {
                        _jobs[dependentAlias].ResolveVariables(
                            alias,
                            _jobs[alias].Outputs
                        );

                        if (_jobs[dependentAlias].Status == ForemanJobStatus.Ready) {
                            InvokeJob(_jobs[dependentAlias], nextRound);
                        }
                    }
                } else {
                    nextRound.Add(task);
                }
            }

            _runningJobs = nextRound;
        }

        if (_incompleteJobs > 0) {
            Console.Error.WriteLine("Template did not run to completion.");
            foreach (var job in _jobs.Values) {
                switch (job.Status) {
                    case ForemanJobStatus.Pending:
                        job.Debug("pending variables");
                        break;
                    case ForemanJobStatus.Failed:
                        job.Debug("encountered an error");
                        break;
                    default:
                        job.Debug("INVALID STATUS: {0}", job.Status);
                        break;
                }
            }
        }
    }
}