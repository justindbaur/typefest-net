using Microsoft.CodeAnalysis;
using TypeFest.Net.Analyzer.Shared;

namespace TypeFest.Net.SourceGenerator;

public static class IncrementalValuesExtensions
{
    internal static IncrementalValuesProvider<T> FilterDiagnostics<T>(this IncrementalValuesProvider<Result<T?>> values)
    {
        return values
            .Select((r, _) => r.Item)
            .Where(i => i != null)!;
    }
}
