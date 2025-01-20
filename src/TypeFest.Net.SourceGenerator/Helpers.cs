using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator;

internal static class Helpers
{
    public static Location? GetLocation(this AttributeData attributeData)
    {
        var syntaxReference = attributeData.ApplicationSyntaxReference;
        return syntaxReference?.SyntaxTree.GetLocation(syntaxReference.Span);
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