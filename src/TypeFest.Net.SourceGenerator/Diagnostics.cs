using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator;

public static class Diagnostics
{
    private const string HelpLinkUri = "https://justinbaur.dev/docs/type-fest/errors#{0}";

    public static DiagnosticDescriptor NullArgument { get; } = new DiagnosticDescriptor(
        id: "TF0001",
        title: "null is an invalid argument.",
        messageFormat: "",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TI0001"));

    public static DiagnosticDescriptor DuplicateArgument { get; } = new DiagnosticDescriptor(
        id: "TF0002",
        title: "Argument is a duplicate and will be ignored.",
        messageFormat: "'{0}' is a duplicate argument and will be ignored.",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TI0002"));

    public static DiagnosticDescriptor InvalidPropertyName { get; } = new DiagnosticDescriptor(
        id: "TF0003",
        title: "Argument does not correspond to a property on the source type.",
        messageFormat: "'{0}' is not a valid member on '{1}'",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TI0003")
    );

    public static DiagnosticDescriptor InvalidTypeKind { get; } = new DiagnosticDescriptor(
        id: "TF0004",
        title: "Cannot create a type based on a different type kind.",
        messageFormat: "Cannot create a {0} based on a {1}.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TI0004")
    );
}