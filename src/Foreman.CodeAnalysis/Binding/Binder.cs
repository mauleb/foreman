using System.Collections.Immutable;

using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

// TODO: not static?
public static class Binder {
    public static BoundNodeBase? Bind(BindingContext context, SyntaxBase syntax) => syntax.Kind switch {
        SyntaxKind.Document => BindDocument(context, (DocumentSyntax)syntax),
        SyntaxKind.Element => BindElement(context, (ElementSyntax)syntax),
        SyntaxKind.Attribute => BindAttribute(context, (AttributeSyntax)syntax),
        SyntaxKind.NamespacedSymbol => BindNamespacedSymbol(context, (NamespacedSymbolSyntax)syntax),
        _ => null
    };

    private static BoundDocument BindDocument(BindingContext context, DocumentSyntax syntax) {
        BoundElement? root = BindElement(context, syntax.Root);
        VariableReferenceBag referenceBag = context.VariableReferences;
        
        switch (root?.OpenTag.Data.ToLower()) {
            // TODO: other well known elements, e.g. template
            case "job":
                return new BoundForemanJob() {
                    VariableReferences = referenceBag,
                    Root = root
                };
            default:
                foreach (var span in referenceBag.Spans) {
                    context.ReportUnexpectedSymbol(span);
                }

                return new() {
                    VariableReferences = referenceBag,
                    Root = root
                };
        }
    }


    private static BoundElement? BindElement(BindingContext context, ElementSyntax syntax) {
        if (syntax.ElementName == null) { return null; }
        if (syntax.CloseOpenTag == null) { return null; }

        BoundSpan<string> openTag = context.BindString(syntax.ElementName.Span);
        BoundSpan<string> closeOpenTag = context.BindString(syntax.CloseOpenTag.Span);

        if (closeOpenTag.Data != "/>") {
            // there are two instances of the tag which should match
            if (syntax.CloseTag == null) { return null; }
            if (syntax.CloseTag.ElementName == null) { return null; }

            BoundSpan<string> closeTag = context.BindString(syntax.CloseTag.ElementName.Span);
            if (openTag.Data != closeTag.Data) {
                context.ReportNonMatchingElementTags(openTag.Data, closeTag.Span);
            }
        }

        HashSet<string> attributeKeys = [];
        List<BoundAttribute> attributes = [];
        foreach (var nextAttr in syntax.Attributes?.Nodes ?? []) {
            if (nextAttr.Kind != SyntaxKind.Attribute) {
                context.ReportUnexpectedSyntax(nextAttr.Kind, nextAttr.Span);
                continue;
            }

            BoundAttribute? attr = BindAttribute(context, (AttributeSyntax)nextAttr);
            if (attr == null) {
                context.ReportBindingError(BoundNodeKind.Attribute, nextAttr.Span);
                continue;
            }

            // you should not declare the same attribute twice
            if (attributeKeys.Contains(attr.Key.Data)) {
                context.ReportDuplicateAttribute(nextAttr.Span);
            }
            attributeKeys.Add(attr.Key.Data);

            attributes.Add(attr);
        }

        BoundValueSequence? contentValue = null;
        List<BoundElement> nestedElements = [];
        if (syntax.NestedElements != null) {
            if (syntax.NestedElements.Nodes.Length == 1 && syntax.NestedElements.Nodes[0].Kind == SyntaxKind.Content) {
                // e.g. <elem>text content potentially with @{interpolation}@</elem>
                contentValue = BindValueSequence(
                    context,
                    syntax.NestedElements.Nodes[0].Children
                );
            } else {
                // e.g. <elem><childElem /></elem>
                foreach (var nextChild in syntax.NestedElements.Nodes) {
                    switch (nextChild.Kind) {
                        case SyntaxKind.Comment:
                        case SyntaxKind.Trivia:
                            // binder just doesn't care
                            continue;
                        case SyntaxKind.Element:
                            BoundElement? element = BindElement(context, (ElementSyntax)nextChild);
                            if (element == null) {
                                context.ReportBindingError(BoundNodeKind.Element, nextChild.Span);
                            } else {
                                nestedElements.Add(element);
                            }
                            break;
                        default:
                            context.ReportUnexpectedSyntax(nextChild.Kind, nextChild.Span);
                            continue;
                    }
                }

            }
        }

        return new() {
            OpenTag = openTag,
            Attributes = attributes.ToImmutableArray(),
            ValueContent = contentValue,
            NestedElements = nestedElements.Count > 0
                ? nestedElements.ToImmutableArray()
                : null
        };
    }


    private static BoundAttribute? BindAttribute(BindingContext context, AttributeSyntax syntax) {
        if (syntax.OpenBlock == null) { return null; }
        if (syntax.CloseBlock == null) { return null; }

        BoundSpan<string> attributeKey = context.BindString(syntax.AttributeName.Span);
        BoundValueSequence valueSequence = BindValueSequence(context, syntax.Contents?.Nodes ?? []);

        return new() {
            Span = syntax.Span,
            Key = attributeKey,
            Value = valueSequence
        };
    }

    private static BoundValueSequence BindValueSequence(BindingContext context, IEnumerable<SyntaxBase?> syntaxes) {
        List<BoundNodeBase> valueNodes = [];

        DocumentSpan? arbitraryContentSpan = null;
        Action collapseArbitraryContent = () => {
            if (arbitraryContentSpan == null) {
                return;
            }

            BoundSpan<string> contentSpan = context.BindString(arbitraryContentSpan);
            valueNodes.Add(contentSpan);
            arbitraryContentSpan = null;
        };

        foreach (var item in syntaxes) {
            if (item == null) {
                // idk
                continue;
            } if (item.Kind == SyntaxKind.Single) {
                arbitraryContentSpan = arbitraryContentSpan == null
                    ? item.Span
                    : arbitraryContentSpan + item.Span;
            } else if (item.Kind == SyntaxKind.InterpolatedSymbol) {
                // we don't actually care about a sequence of literals, the binder just collapses them into a single bound element
                collapseArbitraryContent();
                InterpolatedSymbolSyntax interpolatedSymbol = (InterpolatedSymbolSyntax)item;
                if (interpolatedSymbol.Symbol == null) {
                    context.ReportBindingError(BoundNodeKind.Symbol, item.Span);
                    continue;
                }
                
                BoundNodeBase? boundInterpolation = Bind(context, interpolatedSymbol.Symbol);
                if (boundInterpolation == null) {
                    context.ReportBindingError(BoundNodeKind.Symbol, item.Span);
                    continue;
                }

                valueNodes.Add(boundInterpolation);
            } else {
                context.ReportUnexpectedSyntax(item.Kind, item.Span);
                continue;
            }
        }

        collapseArbitraryContent();
        BoundValueSequence valueSequence = new() {
            Nodes = valueNodes.ToImmutableArray()
        };

        return valueSequence;
    }

    private static BoundSymbol? BindNamespacedSymbol(BindingContext context, NamespacedSymbolSyntax syntax) {
        if (syntax.Nodes.Length % 2 != 1 || syntax.Nodes.Length < 3) {
            // we hard expect *at minimum* one slash, e.g. namespace/key
            context.ReportMalformedSymbol(syntax.Span);
            return null;
        }

        DocumentSpan namespaceSpan = syntax.Nodes
            .Take(syntax.Nodes.Length - 2)
            .Select(part => part.Span)
            .Aggregate((a,b) => a + b);

        BoundSymbol boundSymbol = new() {
            Namespace = context.BindString(namespaceSpan),
            Key = context.BindString(syntax.Nodes.Last().Span)
        };

        context.AddVariableReference(boundSymbol.Pointer, boundSymbol.Span);

        return boundSymbol;
    }
}