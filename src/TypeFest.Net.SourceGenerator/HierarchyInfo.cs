using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SymbolDisplayTypeQualificationStyle;

namespace TypeFest.Net.SourceGenerator;

internal sealed record TypeInfo(string QualifiedName, TypeKind TypeKind, bool IsRecord);

internal sealed record HierarchyInfo(string FilenameHint, string MetadataName, string Namespace, ImmutableEquatableArray<TypeInfo> Hierarchy)
{
	public static HierarchyInfo From(INamedTypeSymbol typeSymbol)
	{
	    var hierarchy = new List<TypeInfo>();

		for (INamedTypeSymbol? parent = typeSymbol;
		     parent is not null;
			 parent = parent.ContainingType)
        {
			hierarchy.Add(new TypeInfo(
				parent.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
				parent.TypeKind,
				parent.IsRecord
			));
		}

		return new(
			typeSymbol.GetFullyQualifiedMetadataName(),
			typeSymbol.MetadataName,
			typeSymbol.ContainingNamespace.ToDisplayString(new(typeQualificationStyle: NameAndContainingTypesAndNamespaces)),
			hierarchy.ToImmutableEquatableArray() 
		);
	}

	public string GlobalName()
	{
		var names = string.Join(".", Hierarchy.Select(t => t.QualifiedName).Reverse());

   		if (Namespace is "")
		{
			return $"global::{names}";
		}

		return $"global::{Namespace}.{names}";
	}
}
