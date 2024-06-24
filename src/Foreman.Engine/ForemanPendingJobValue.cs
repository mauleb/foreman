using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Foreman.Engine;

[Serializable]
public record ForemanPendingJobValue {
    [XmlAttribute("evalPath")]
    public required string EvalPath { get; init; }
    [XmlAttribute("id")]
    public required string Id { get; init; }
    [XmlAttribute("type")]
    public required string ValueType { get; init; }
    [XmlAttribute("target")]
    public required string Target { get; init; }
    [XmlAttribute("attribute")]
    public string? AttributeName { get; init; }
    
    private int _unresolvedFragments = 0;
    private ForemanPendingJobValueFragment[] _fragments = [];

    [XmlElement("fragment", typeof(ForemanPendingJobValueFragment))]
    public required ForemanPendingJobValueFragment[] Fragments {
        get => _fragments;
        init {
            _fragments = value;
            _unresolvedFragments = _fragments
                .Where(frag => frag.Value == null)
                .Count();
        }
    }

    public bool IsResolved => _unresolvedFragments == 0;
    public string ResolvedValue => _fragments
        .Select(frag => frag.Value ?? "")
        .Aggregate(new StringBuilder(), (sb, next) => sb.Append(next))
        .ToString();

    internal void ResolveFragment(int index, string value) {
        if (index < 0 || index >= _fragments.Length) {
            throw new InvalidOperationException("Out of bound resolution of a fragment.");
        }

        if (_fragments[index].Value == null) {
            _unresolvedFragments -= 1;
        }

        _fragments[index] = _fragments[index] with {
            Value = value
        };
    }
}
