namespace TypeFest.Net.Analyzer.Shared;

internal sealed record Result<T>(T Item, ImmutableEquatableArray<DiagnosticInfo> Errors);