using System.Xml;

using Foreman.Engine;

namespace Engine.ForemanJobTests;

public class ParseJobData {
    [Fact]
    public void Should_HandleEmptyJob() {
        string data = """
        <job handler="foreman:map">
            <definition />
            <pendingValues />
            <pendingVariables />
        </job>
        """;

        XmlDocument document = new();
        document.LoadXml(data);

        var job = NewForemanJob.ParseJobData(document);
        Assert.Empty(job.ContentErrors);
    }
}