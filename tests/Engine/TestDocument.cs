using System.Xml;

namespace Foreman.Engine;

internal class TestDocument : XmlDocument {
    public override string BaseURI => "file:///my.file.xml";
    public static TestDocument FromData(string data) {
        TestDocument document = new();
        document.LoadXml(data);
        return document;
    }
}