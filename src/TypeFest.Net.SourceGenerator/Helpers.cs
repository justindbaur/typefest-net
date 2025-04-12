using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator;

internal static class Helpers
{
    public static Location? GetLocation(this AttributeData attributeData)
    {
        var syntaxReference = attributeData.ApplicationSyntaxReference;
        return syntaxReference?.SyntaxTree.GetLocation(syntaxReference.Span);
    }

    public static ImmutableArray<string> GetNamespaceParts(this ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace;
        var namespaceParts = ImmutableArray.CreateBuilder<string>();

        while (ns != null && !ns.IsGlobalNamespace)
        {
            namespaceParts.Add(ns.Name);
            ns = ns.ContainingNamespace;
        }

        return namespaceParts.ToImmutableArray();
    }

    public static string Qualified(this ITypeSymbol typeSymbol)
    {
        var containingNamespace = typeSymbol.ContainingNamespace;
        if (containingNamespace.IsGlobalNamespace)
        {
            return typeSymbol.Name;
        }

        return $"{containingNamespace.Name}.{typeSymbol.Name}";
    }
}