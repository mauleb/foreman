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

        var document = TestDocument.FromData(data);
        var job = ForemanJobDefinition.ParseJobData(document);
        
        Assert.Empty(job.PendingConditions);
        Assert.Empty(job.PendingValues);
        Assert.Empty(job.PendingVariables);
    }
}