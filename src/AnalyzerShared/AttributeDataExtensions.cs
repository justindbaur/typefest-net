using Microsoft.CodeAnalysis;

namespace TypeFest.Net.Analyzer.Shared;

internal static class AttributeDataExtensions
{
    public static Location? GetLocation(this AttributeData attributeData)
    {
        var syntaxReference = attributeData.ApplicationSyntaxReference;
        return syntaxReference?.SyntaxTree.GetLocation(syntaxReference.Span);
    }
}