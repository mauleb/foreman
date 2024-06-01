// using System.Management.Automation;
// using System.Text.Json;

// using PowerShell shell = PowerShell.Create();
// shell.AddScript(File.ReadAllText(@"/Users/maule/workspace/lsp-thesequel/src/Foreman.Console/example.ps1"));
// var output = await shell.InvokeAsync();
// var result = output.Last();

// var dict = JsonSerializer.Deserialize<Dictionary<string,string>>(JsonSerializer.Serialize(result.BaseObject));
// foreach (var kvp in dict ?? []) {
//     Console.WriteLine("{0}={1}", kvp.Key, kvp.Value);
// }

// Console.WriteLine("Hello, World!");

using System.Xml;

using Foreman.Engine;

using Spectre.Console;
using Spectre.Console.Cli;

#if DEBUG
args = ["run","example/perEnv","--input","envCode=dev"];
#endif

var app = new CommandApp();
app.Configure(o => {
    o.AddCommand<RunCommand>("run")
        .WithDescription("Run a Forman template artifact.")
        .WithExample(["run","./path/to/template/folder"]);
});
app.Run(args);

public static class ForemanExtensions {
    public static SelectionPrompt<string> BuildPrompt(this ForemanTemplateInput input) {
        if (input.AllowedValues == null) {
            throw new InvalidOperationException("Unable to build a spectre prompt for a unconstrained input.");
        }

        return new SelectionPrompt<string>()
            .Title(input.Name + "::")
            .AddChoices(input.AllowedValues);
    }
}

public class RunCommandSettings : CommandSettings {
    [CommandArgument(0, "[TEMPLATE]")]
    public required string Template { get; set; }
    [CommandOption("-i|--input <VALUES>")]
    public required string[] Inputs { get; set; }
    public Dictionary<string,string> GetInputCollection() {
        Dictionary<string,string> result = [];
        foreach (string next in Inputs ?? []) {
            string[] parts = next.Split("=");
            result[parts[0]] = parts[1][0] == '"' && parts[1][-1] == '"'
                ? parts[1].Substring(1, parts[1].Length - 2)
                : parts[1];
        }
        return result;
    }
}

public class RunCommand : AsyncCommand<RunCommandSettings> {
    public override async Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings) {
        var cwd = Environment.CurrentDirectory;
        string templatePath = Path.Join(cwd, settings.Template);
        var template = ForemanTemplate.Load(templatePath);

        Dictionary<string,string> declaredInputs = settings.GetInputCollection();

        foreach (var input in template.Inputs) {
            if (declaredInputs.ContainsKey(input.Name)) {
                template.SetInput(input, declaredInputs[input.Name]);
            } else {
                string value = input.AllowedValues == null
                    ? AnsiConsole.Ask<string>(input.Name + ":: ")
                    : AnsiConsole.Prompt(input.BuildPrompt());
                template.SetInput(input, value);
            }
        }

        ForemanExecutionContext executionContext = template.BuildExecutionContext();
        executionContext.OnVariableSet += (key, value) => {
            AnsiConsole.WriteLine("DEFINED > " + key + "=" + value);
        };

        executionContext.OnJobUpdated += (alias, status) => {
            AnsiConsole.WriteLine($"[{alias}] -> {status}");
        };

        await template.Invoke(executionContext);

        return 0;
    }
}
