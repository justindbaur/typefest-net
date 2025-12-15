using Microsoft.CodeAnalysis;

namespace TypeFest.Net.Analyzer.Shared;

internal static class Diagnostics
{
    private const string HelpLinkUri = "https://justinbaur.dev/docs/type-fest/errors#{0}";

    /// <summary>TF0001</summary>
    public static DiagnosticDescriptor NullArgument { get; } = new DiagnosticDescriptor(
        id: "TF0001",
        title: "null is not an invalid argument.",
        messageFormat: "null is not a valid argument and will be ignored.",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0001"));


    /// <summary>TF0002</summary>
    public static DiagnosticDescriptor DuplicateArgument { get; } = new DiagnosticDescriptor(
        id: "TF0002",
        title: "Argument is a duplicate and will be ignored.",
        messageFormat: "'{0}' is a duplicate argument and will be ignored.",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0002"));


    /// <summary>TF0003</summary>
    public static DiagnosticDescriptor InvalidPropertyName { get; } = new DiagnosticDescriptor(
        id: "TF0003",
        title: "Argument does not correspond to a property on the source type.",
        messageFormat: "'{0}' is not a valid member on '{1}'",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0003")
    );

    /// <summary>TF0004</summary>
    public static DiagnosticDescriptor InvalidTypeKind { get; } = new DiagnosticDescriptor(
        id: "TF0004",
        title: "Cannot create a type based on a different type kind.",
        messageFormat: "Cannot create a {0} based on a {1}.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0004")
    );

    /// <summary>TF0005</summary>
    public static DiagnosticDescriptor TargetTypeContainsMember { get; } = new DiagnosticDescriptor(
        id: "TF0005",
        title: "The target type already contains a member with the given name.",
        messageFormat: "'{0}' is already a member on {1}",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0005")
    );

    /// <summary>TF0006</summary>
    public static DiagnosticDescriptor PartialTypeOutOfSync { get; } = new DiagnosticDescriptor(
        id: "TF0006",
        title: "Type is out-of-sync",
        messageFormat: "{0} is missing the following arguments from {1}: {2}",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: string.Format(HelpLinkUri, "TF0006")
    );
}