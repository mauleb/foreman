using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Parsing;

public abstract class ParserTests {
    protected void HandlePositiveCases<TNode>(string text, Func<XmlDocumentParsingContext,TNode?> parser) where TNode : SyntaxBase {
        MultiLineString mls = new(text);
        XmlDocumentParsingContext context = new(mls);

        var result = parser(context);

        CustomAssert.All(
            result != null,
            context.Diagnostics.IsEmpty,
            context.Tokens.Current().Kind == SyntaxTokenKind.EOF
        );
    }

    protected void HandleNegativeCases<TNode>(string text, Func<XmlDocumentParsingContext,TNode?> parser) where TNode : SyntaxBase {
        MultiLineString mls = new(text);
        XmlDocumentParsingContext context = new(mls);

        var result = parser(context);

        CustomAssert.Any(
            result == null,
            context.Diagnostics.Any(),
            context.Tokens.Current().Kind != SyntaxTokenKind.EOF
        );
    }
}

public class XmlDocumentParserTests {
    public class ParseElement : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ["<hello />"],
            ["<   hello/>"],
            ["<hello who=\"world\"/>"],
            ..ParseAttribute.PositiveCases()
                .Select(test => "<element " + test[0] + "/>")
                .Select<string,object[]>(text => [text]),
            ..ParseAttribute.PositiveCases()
                .Select(test => "<element " + test[0] + " " + test[0] + "/>")
                .Select<string,object[]>(text => [text]),
            ["<hello></hello>"],
            ["<hello><wow/></hello>"],
            ["<hello><!--cool--><wow/><!--wow--></hello>"],
            ["<hello><hello><wow/></hello></hello>"],
            ["<hello><hello><wow/></hello><hello><wow/></hello><hello><wow/></hello></hello>"],
            ["<hello>value</hello>"],
            ["<hello>value@{var/cool}@nice</hello>"],
            ["<hello>@{var/cool}@</hello>"],
            ["<!--cool--><hello>@{var/cool}@</hello>"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["<"],
            ["</>"],
            ["<asd123/>"],
            ..ParseAttribute.PositiveCases()
                .Select(test => "<element " + test[0] + test[0] + "/>")
                .Select<string,object[]>(text => [text]),
            ..ParseAttribute.NegativeCases()
                .Select(test => "<element " + test[0] + "/>")
                .Select<string,object[]>(text => [text]),
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseElement);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseElement);
    }
    public class ParseTrivia : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ..ParseComment.PositiveCases(),
            ..ParseComment
                .PositiveCases()
                .SelectMany(outer => ParseComment
                    .PositiveCases()
                    .Select(inner => outer[0] + "" + inner[0]))
                .Select<string,object[]>(text => [text]),
            ..ParseComment
                .PositiveCases()
                .SelectMany(outer => ParseComment
                    .PositiveCases()
                    .Select(inner => "\n" + outer[0] + "\n\n\n" + outer[0] + inner[0]))
                .Select<string,object[]>(text => [text])
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["<!"],
            ["<!--"],
            ["<!----"]
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseTrivia);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseTrivia);
    }
    public class ParseComment : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ["<!-- hello -->"],
            ["<!-- ~`!@#$%^&*()_+-={}|[]\\:\";'<>?,./ -->"],
            ["<!-- @{}@<//><!-- -->"],
            ["<!--\n\n\nwow\ncool\r\r\n-->"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["<!"],
            ["<!--"],
            ["<!----"],
            ..PositiveCases()
                .Select(test => "  " + test[0])
                .Select<string,object[]>(text => [text]),
            ..PositiveCases()
                .Select(test => test[0] + "  ")
                .Select<string,object[]>(text => [text])
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseComment);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseComment);
    }
    public class ParseClosingTag : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ["</hello>"],
            ["</   hello  >"],
            ["</\n\nhello\n\n>"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["</"],
            ["</>"],
            ["</asd"],
            ["< /hello>"],
            ["</hello13>"],
            ["</hello-world>"],
            ["</hello.world>"],
            ["</hello/world>"]
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseClosingTag);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseClosingTag);
    }
    public class ParseAttribute : ParserTests {
        private static readonly IEnumerable<string> ValidAttributeNames = [
            "myAttr",
            "my-attr",
            "MYATTR",
            "myAttr23"
        ];

        private static IEnumerable<string> ValidContents => [
            "hello",
            "an english sentence.",
            "123123",
            "~`!@#$%^&*()_-+={}[]\\|",
            "</<!--}@",
            "@{var/value}@",
            "shared-@{azure/env.code}@",
            "@{var/wow}@.@{var/cool}@ and a bottle of @{var/beverage}@",
            ..ParseInterpolatedSymbol.PositiveCases()
                .Select(test => (string)test[0])
        ];

        public static IEnumerable<object[]> PositiveCases() => ValidContents
            .SelectMany(contents => ValidAttributeNames.Select(atr => string.Format("{0}=\"{1}\"", atr, contents)))
            .Select<string,object[]>(text => [text]);

        public static IEnumerable<object[]> NegativeCases() => [
            ["myAttr"],
            ["myAttr="],
            ["myAttr=\""],
            ["myAttr=\"@{\""],
            ["myAttr=\"@{}@\""],
            ["myAttr =\"@{hello}@\""],
            ["myAttr= \"@{hello}@\""],
            ["myAttr\n=\"@{hello}@\""],
            ["myAttr=\n\"@{hello}@\""],
            ["myAttr=\"he\nllo\""],
            ..ValidContents
                .Select(contents => "123=\"" + contents + "\"")
                .Select<string,object[]>(text => [text]),
            ..ValidContents
                .Select(contents => "compound.value=\"" + contents + "\"")
                .Select<string,object[]>(text => [text]),
            ..ValidContents
                .Select(contents => "namespaced/value=\"" + contents + "\"")
                .Select<string,object[]>(text => [text])
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseAttribute);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseAttribute);
    }
    public class ParseInterpolatedSymbol : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => ParseNamespacedSymbol
            .PositiveCases()
            .Select(test => "@{" + test[0] + "}@")
            .Select<string, object[]>(text => [text]);

        public static IEnumerable<object[]> NegativeCases() => [
            ..ParseNamespacedSymbol.PositiveCases(),
            ["@{"],
            ["@{asdad"],
            ["@{}@"]
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseInterpolatedSymbol);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseInterpolatedSymbol);
    }
    public class ParseNamespacedSymbol : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ..ParseCompoundSymbol
                .PositiveCases()
                .Select(test => "example/" + test[0])
                .Select<string,object[]>(text => [text]),
            ["hello/world"],
            ["azure/env.code"],
            ["a/b/c/d/e/f/g"],
            ["hello/world"],
            ["who.me/name"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["wow"],
            ["@wow"],
            ["-wow"],
            ["wow-"],
            [".wow"],
            ["wow."],
            ["/wow"],
            ["wow/"],
            ["  wow"],
            ["wow  "]
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseNamespacedSymbol);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseNamespacedSymbol);
    }
    public class ParseCompoundSymbol : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ..ParsePartialSymbol.PositiveCases(),
            ["a.b.c.d"],
            ["a1.b-2.c"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["123"],
            ["  xx"],
            [".asd"],
            ["asd."],
            ["hello/world"],
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParseCompoundSymbol);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParseCompoundSymbol);
    }
    public class ParsePartialSymbol : ParserTests {
        public static IEnumerable<object[]> PositiveCases() => [
            ["hello"],
            ["a1"],
            ["a1a1a1a"],
            ["a-a"],
            ["a-1"],
            ["a-1-a-2-aaa-a-a"]
        ];

        public static IEnumerable<object[]> NegativeCases() => [
            ["hello-"],
            ["hello:"],
            ["hello/world"],
            ["hello.world"],
            ["123"],
            ["-x"],
            ["/asd"],
            ["  wow"]
        ];

        [Theory]
        [MemberData(nameof(PositiveCases))]
        public void Should_FullyParseText(string text)
            => HandlePositiveCases(text, XmlDocumentParser.ParsePartialSymbol);

        [Theory]
        [MemberData(nameof(NegativeCases))]
        public void Should_FailToFullyConsume(string text)
            => HandleNegativeCases(text, XmlDocumentParser.ParsePartialSymbol);
    }
}