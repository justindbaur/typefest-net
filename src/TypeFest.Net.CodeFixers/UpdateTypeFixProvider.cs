using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
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

        var syntaxRoot = await doc.GetSyntaxRootAsync(token);

        if (syntaxRoot is null)
        {
            return doc;
        }

        var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);

        if (node is not AttributeSyntax attributeSyntax)
        {
            return doc;
        }

        if (attributeSyntax.Parent is not AttributeListSyntax attributeList)
        {
            return doc;
        }

        if (attributeList.Parent is not TypeDeclarationSyntax typeDeclaration)
        {
            return doc;
        }

        if (attributeSyntax.Name is not GenericNameSyntax genericName)
        {
            return doc;
        }

        var sourceType = genericName.TypeArgumentList.Arguments[0];

        // TODO: Simplify
        return doc.WithSyntaxRoot(
            syntaxRoot.ReplaceNode(
                typeDeclaration,
                typeSpec.Build(typeDeclaration, sourceType).WithSemicolonToken(Token(SyntaxKind.None))
            )
        );
    }
}