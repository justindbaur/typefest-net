using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeFest.Net.Analyzer.Shared;

public static class GeneratorAttributeSyntaxContextExtensions
{
    public static (AttributeData Data, AttributeSyntax Syntax) GetSingleDataAndSyntax(this GeneratorAttributeSyntaxContext context)
    {
        Debug.Assert(context.Attributes.Length == 1);
        var data = context.Attributes[0];

        Debug.Assert(data.AttributeClass is not null);

        if (context.TargetNode is not MemberDeclarationSyntax memberDeclaration)
        {
            throw new InvalidOperationException("TargetNode with attributes is expected to be castable to MemberDeclarationSyntax");
        }

        var syntax = memberDeclaration.AttributeLists
            .SelectMany(a => a.Attributes)
            .Single(a => a.Name is SimpleNameSyntax simpleName && (data.AttributeClass!.Name == simpleName.Identifier.Text || data.AttributeClass.Name == $"{simpleName.Identifier.Text}Attribute"));

        return (data, syntax);
    }
}