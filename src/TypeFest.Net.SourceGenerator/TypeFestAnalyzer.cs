using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TypeFest.Net.SourceGenerator.Utilities;

namespace TypeFest.Net.SourceGenerator;

internal struct AttributeInfo
{
    public required ISymbol Target { get; init; }
    public required AttributeData Data { get; init; }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeFestAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostics.NullArgument, Diagnostics.DuplicateArgument, Diagnostics.InvalidPropertyName, Diagnostics.InvalidTypeKind, Diagnostics.PartialTypeOutOfSync);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = (AttributeSyntax)context.Node;

        Result<PartialTypeSpec?> result;

        if (TryGetInfo(node, context, "TypeFest.Net.OmitAttribute`1", "OmitAttribute", "Omit", out var omitInfo))
        {
            result = PartialTypeSpec.CreateOmit(omitInfo.Target, omitInfo.Data, node, context.CancellationToken);
        }
        else if (TryGetInfo(node, context, "TypeFest.Net.PickAttribute`1", "PickAttribute", "Pick", out var pickInfo))
        {
            result = PartialTypeSpec.CreatePick(pickInfo.Target, pickInfo.Data, node, context.CancellationToken);
        }
        else
        {
            return;
        }

        // TODO: Switch to reporting errors here
        foreach (var diagnostic in result.Errors)
        {
            var diag = diagnostic.ToDiagnostic();
            // Special case the removal of this diagnostic from the analyzer until
            // runtime mode is known in PartialTypeSpec
            if (diag.Id == Diagnostics.TargetTypeContainsMember.Id)
            {
                continue;
            }
            context.ReportDiagnostic(diag);
        }

        if (result.Item != null && result.Item.HasChanges)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.PartialTypeOutOfSync,
                node.GetLocation(),
                result.Item.TargetType.FilenameHint, result.Item.SourceType.FilenameHint, string.Join(", ", result.Item.GetMemberNames())
            ));
        }
    }

    private static bool TryGetInfo(AttributeSyntax syntax, SyntaxNodeAnalysisContext context, string metadataName, string fullAttributeName, string shortName, out AttributeInfo attributeInfo)
    {
        attributeInfo = default;
        if (syntax.Name is not GenericNameSyntax genericName)
        {
            return false;
        }

        if (genericName.Identifier.Text != shortName && genericName.Identifier.Text != fullAttributeName)
        {
            return false;
        }

        // The first parent will be the attribute list this attribute is in
        // and the grandparent will be the type it's attached to
        var targetSymbol = context.SemanticModel.GetDeclaredSymbol(syntax.Parent!.Parent!);
        if (targetSymbol is null)
        {
            return false;
        }

        var attributeType = context.Compilation.GetTypeByMetadataName(metadataName);

        if (attributeType is null)
        {
            throw new InvalidOperationException($"Could not find a type by the given metadata name: {metadataName}");
        }

        Debug.Assert(attributeType.IsGenericType);
        attributeType = attributeType.ConstructUnboundGenericType();

        var allAttributes = targetSymbol.GetAttributes();

        var attribute = allAttributes
            .SingleOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass?.ConstructUnboundGenericType(), attributeType));

        if (attribute == null)
        {
            return false;
        }

        attributeInfo = new AttributeInfo
        {
            Target = targetSymbol,
            Data = attribute,
        };
        return true;
    }
}