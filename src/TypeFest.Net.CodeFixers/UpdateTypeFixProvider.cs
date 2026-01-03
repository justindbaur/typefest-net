using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Simplification;
using TypeFest.Net.Analyzer.Shared;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TypeFest.Net.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UpdateTypeFixProvider)), Shared]
public class UpdateTypeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("TF0006");

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Update out-of-sync type",
                    async (token) => await Fix(context.Document, diagnostic, token),
                    "TF0006"
                ),
                diagnostic
            );
        }

        return Task.CompletedTask;
    }

    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    private static async Task<Document> Fix(Document doc, Diagnostic diagnostic, CancellationToken token)
    {
        var typeSpec = PartialTypeSpec.Deserialize(diagnostic.Properties);

        token.ThrowIfCancellationRequested();

        var syntaxRoot = await doc.GetSyntaxRootAsync(token);

        if (syntaxRoot is null)
        {
            Debug.WriteLine("SyntaxRoot could not be retrieved.");
            return doc;
        }

        var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);

        if (node is not AttributeSyntax attributeSyntax)
        {
            Debug.WriteLine("Node is not an attribute");
            return doc;
        }

        if (attributeSyntax.Parent is not AttributeListSyntax attributeList)
        {
            Debug.WriteLine("Node parent is not an attribute list.");
            return doc;
        }

        if (attributeList.Parent is not BaseTypeDeclarationSyntax typeDeclaration)
        {
            Debug.WriteLine($"Attribute is not attached to a type: {attributeList.Parent!.GetType()}");
            return doc;
        }

        if (attributeSyntax.Name is not GenericNameSyntax genericName)
        {
            Debug.WriteLine("Attribute name is not generic.");
            return doc;
        }

        var sourceType = genericName.TypeArgumentList.Arguments[0];

        return doc.WithSyntaxRoot(
            syntaxRoot.ReplaceNode(
                typeDeclaration,
                typeSpec.Build(typeDeclaration, sourceType)
                    .WithSemicolonToken(Token(SyntaxKind.None))
                    .WithAdditionalAnnotations(
                        // Simplifier is for things like namespaces, we can shorten them if the consumer has them in context already
                        Simplifier.Annotation,
                        // TODO: I'm not totally sure what formatting rules we might be breaking but I bet there are some
                        // if i even find some, then I should make a test that shows it is eorking
                        Formatter.Annotation
                    )
            )
        );
    }
}