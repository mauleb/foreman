namespace Foreman.CodeAnalysis.Binding;

public class VariableValueBag {
    private readonly Dictionary<VariableIdentifier, string> _values = [];

    public void SetVariable(VariableIdentifier pointer, string value) {
        _values[pointer] = value;
    }

    public void SetVariable(string @namespace, string key, string value)
        => SetVariable(new VariableIdentifier(@namespace, key), value);

    public string GetVariable(VariableIdentifier pointer) {
        if (_values.ContainsKey(pointer)) {
            return _values[pointer];
        }

        return string.Empty;
    }

    public string GetVariable(string @namespace, string key)
        => GetVariable(new VariableIdentifier(@namespace, key));
}