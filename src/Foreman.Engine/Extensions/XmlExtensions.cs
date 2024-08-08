using System.Xml;

namespace Foreman.Engine;

internal static class XmlExtensions {
    internal static IEnumerable<XmlNode> TrySelectNodes(this XmlDocument document, string xpath) {
        XmlNodeList? result = document.SelectNodes(xpath);
        if (result == null) { yield break; }

        foreach (XmlNode node in result) {
            yield return node;
        }
    }

    internal static string?[] TrySelectAttributes(this XmlNode node, params string[] attributeNames) {
        string?[] outputs = new string?[attributeNames.Length];

        XmlAttributeCollection? attributeCollection = node.Attributes;
        if (attributeCollection == null) {
            return outputs;
        }

        for (int idx = 0; idx < attributeNames.Length; idx += 1) {
            string name = attributeNames[idx];
            XmlAttribute? attribute = attributeCollection[name];
            outputs[idx] = attribute?.Value;
        }

        return outputs;
    }
}