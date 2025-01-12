using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator;

internal static class Helpers
{
    public static Location? GetLocation(this AttributeData attributeData)
    {
        var syntaxReference = attributeData.ApplicationSyntaxReference;
        return syntaxReference?.SyntaxTree.GetLocation(syntaxReference.Span);
    }
}