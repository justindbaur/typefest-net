using Microsoft.CodeAnalysis.Diagnostics;

namespace TypeFest.Net.Analyzers;

internal static class AnalyzerConfigOptionsProviderExtensions
{
    public static bool IsUsingSourceGenMode(this AnalyzerConfigOptionsProvider provider)
    {
        if (!provider.GlobalOptions.TryGetValue("build_property.TypeFestNet_GenerateMode", out var generateMode))
        {
            // No setting defaults to source gen mode
            return true;
        }

        return !string.Equals(generateMode, "CodeFix", System.StringComparison.OrdinalIgnoreCase);
    }
}