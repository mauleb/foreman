namespace Foreman.Engine;

public class ForemanExecutionContext {
    private bool _hasExecuted = false;
    private readonly ForemanExecutionOptions _options;
    private readonly Dictionary<string,ForemanTemplateInput> _missingInputs = [];
    private readonly Dictionary<string,ForemanJobContext> _jobs = [];

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