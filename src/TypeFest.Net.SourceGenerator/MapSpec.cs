using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator
{
    public record MapTypeInfo
    {
        public required string Name { get; init; }
    }

    public class MapInfo
    {
        public required string TypeOne { get; init; }
        public required string TypeTwo { get; init; }
        public required bool InToMode { get; set; }
        public required ImmutableArray<string> Members { get; init; }

        public static (MapInfo? MapInfo, ImmutableArray<Diagnostic> Diagnostics) Create(ISymbol targetSymbol, AttributeData attribute, bool isTo)
        {
            if (targetSymbol is not INamedTypeSymbol namedTargetSymbol)
            {
                throw new Exception();
            }

            if (attribute.AttributeClass == null)
            {
                throw new Exception();
            }

            if (attribute.AttributeClass.TypeArguments is not [var singleTypeArg]
                || singleTypeArg is not INamedTypeSymbol namedTypeArgument)
            {
                throw new Exception();
            }

            var props = "";

            if (attribute.NamedArguments.SingleOrDefault((arg) => arg.Key == "Ignore") is KeyValuePair<string, TypedConstant> ignoreMembers)
            {
                
            }
        }
    }
}