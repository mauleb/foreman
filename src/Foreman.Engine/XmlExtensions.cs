using System.Xml;

namespace Foreman.Engine;

public static class XmlExtensions {
    public static IEnumerable<XmlNode?> AsEnumerable(this XmlNodeList nodeList) {
        for (int i = 0; i < nodeList.Count; i += 1) {
            yield return nodeList[i];
        }
    }

    public static bool GetAttributeValue(this XmlNode? node, string attributeName, out string value) {
        value = string.Empty;
        if (node == null || node.Attributes == null) {
            return false;
        }

        var attribute = node.Attributes.GetNamedItem(attributeName);
        if (attribute == null) {
            return false;
        }

        value = attribute.InnerText;
        return true;
    }
}
