using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace TypeFest.Net.CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UpdateTypeFixProvider)), Shared]
public class UpdateTypeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("TF0001");

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Update out-of-sync type",
                    async (token) => await Fix(context.Document, token),
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

    private static async Task<Document> Fix(Document doc, CancellationToken token)
    {
        Debug.WriteLine("In fixer");
        await Task.Yield();
        return doc;
    }
}
