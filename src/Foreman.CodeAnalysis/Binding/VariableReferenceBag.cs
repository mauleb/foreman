using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public class VariableReferenceBag {
    private readonly Dictionary<VariableIdentifier, List<DocumentSpan>> _references;

    public VariableReferenceBag() {
        _references = [];
    }

    private VariableReferenceBag(Dictionary<VariableIdentifier, List<DocumentSpan>> references) : this() {
        foreach (var kvp in references) {
            _references[kvp.Key] = new(kvp.Value);
        }
    }

    public VariableReferenceBag Clone() => new(_references);

    public void SetVariableReference(VariableIdentifier pointer, DocumentSpan span) {
        if (!_references.ContainsKey(pointer)) {
            _references[pointer] = [];
        }

        _references[pointer].Add(span);
    }

    public IEnumerable<VariableIdentifier> Identifiers => _references.Keys;
    public IEnumerable<DocumentSpan> Spans => _references.Values
        .SelectMany(list => list);
}