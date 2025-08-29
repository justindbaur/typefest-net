using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TypeFest.Net.SourceGenerator;

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    SyntaxTree? SyntaxTree,
    TextSpan TextSpan,
    ImmutableEquatableArray<string> Arguments)
{
    public Diagnostic ToDiagnostic()
    {
        if (SyntaxTree is not null)
        {
            return Diagnostic.Create(Descriptor, Location.Create(SyntaxTree, TextSpan), Arguments.ToArray());
        }

        return Diagnostic.Create(Descriptor, null, Arguments.ToArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, ISymbol symbol, params object[] args)
    {
        var location = symbol.Locations.First();

        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableEquatableArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode node, params object[] args)
    {
        var location = node.GetLocation();

        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableEquatableArray());
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, params object[] args)
    {
        if (location is null)
        {
            return new(descriptor, null, default, args.Select(static arg => arg.ToString()).ToImmutableEquatableArray());
        }

        return new(descriptor, location.SourceTree, location.SourceSpan, args.Select(static arg => arg.ToString()).ToImmutableEquatableArray());
    }
}