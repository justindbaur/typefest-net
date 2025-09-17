namespace TypeFest.Net.SourceGenerator.Utilities;

internal sealed record Result<T>(T Item, ImmutableEquatableArray<DiagnosticInfo> Errors);