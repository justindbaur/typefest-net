using Microsoft.CodeAnalysis.Testing;

namespace TypeFest.Net.Analyzers.Tests;

public static class SolutionStateExtensions
{
    public static void SetGenerateMode(this SolutionState state, bool useSourceGenMode)
    {
        var modeString = useSourceGenMode ? "SourceGen" : "CodeFix";
        state.AnalyzerConfigFiles.Add(("/.editorconfig", $"""
        is_global = true
        build_property.TypeFestNet_GenerateMode = {modeString}
        """));
    }
}
