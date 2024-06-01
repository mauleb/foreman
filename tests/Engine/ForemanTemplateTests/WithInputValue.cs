using System.Collections.Frozen;

using Foreman.Engine;

namespace Engine.ForemanTemplateTests;

public class WithInputValue {
    [Fact]
    public void Should_ReturnTemplateWithValueSet() {
        Dictionary<string, NewForemanInputValue> inputs = new() {
            { "myInput", new() }
        };
        
        NewForemanTemplate template = new() {
            Inputs = inputs.ToFrozenDictionary()
        };

        template = template.WithInputValue("myInput", "wow");

        Assert.Equal("wow", template.Inputs["myInput"].Value);
    }

    [Fact]
    public void Should_ReturnTemplateWithConstrainedValueSet() {
        Dictionary<string, NewForemanInputValue> inputs = new() {
            { "myInput", new() { AllowedValues = ["yes","no"] } }
        };
        
        NewForemanTemplate template = new() {
            Inputs = inputs.ToFrozenDictionary()
        };

        template = template.WithInputValue("myInput", "yes");

        Assert.Equal("yes", template.Inputs["myInput"].Value);
    }

    [Fact]
    public void Should_FailWhenTemplateHasErrors() {
        Dictionary<string, NewForemanInputValue> inputs = new() {
            { "myInput", new() }
        };
        
        NewForemanTemplate template = new() {
            Inputs = inputs.ToFrozenDictionary(),
            ContentErrors = ["hello"]
        };

        Assert.Throws<InvalidOperationException>(() => template = template.WithInputValue("myInput", "wow"));
    }

    [Fact]
    public void Should_FailWhenInputWithKeyDNE() {
        Dictionary<string, NewForemanInputValue> inputs = new() {
            { "myInput", new() }
        };
        
        NewForemanTemplate template = new() {
            Inputs = inputs.ToFrozenDictionary()
        };

        Assert.Throws<KeyNotFoundException>(() => template = template.WithInputValue("somethingElse", "wow"));
    }

    [Fact]
    public void Should_FailWhenValueNotAllowed() {
        Dictionary<string, NewForemanInputValue> inputs = new() {
            { "myInput", new() { AllowedValues = ["yes","no"] } }
        };
        
        NewForemanTemplate template = new() {
            Inputs = inputs.ToFrozenDictionary()
        };

        Assert.Throws<ArgumentException>(() => template = template.WithInputValue("myInput", "wow"));
    }
}