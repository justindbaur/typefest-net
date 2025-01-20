using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator
{
    public abstract class TypeSpec
    {
        public TypeSpec(INamedTypeSymbol targetType)
        {
            TargetType = targetType;
        }

        public INamedTypeSymbol TargetType { get; }
        public abstract void Emit(IndentedTextWriter writer);

        private static bool TryCreate(
            ISymbol targetSymbol,
            AttributeData attributeData,
            [NotNullWhen(true)] out INamedTypeSymbol? targetType,
            [NotNullWhen(true)] out INamedTypeSymbol? sourceType,
            [NotNullWhen(true)] out ImmutableHashSet<string>? members,
            out ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            if (targetSymbol is not INamedTypeSymbol namedTargetSymbol)
            {
                throw new InvalidOperationException("Target type is not a named type symbol, I think this should be impossible.");
            }

            if (attributeData.AttributeClass == null)
            {
                throw new InvalidOperationException("AttributeData is expected to have a attribute class at this point, how did this happen?");
            }

            if (attributeData.AttributeClass.TypeArguments is not [var singleTypeArg]
                || singleTypeArg is not INamedTypeSymbol namedSourceSymbol)
            {
                throw new InvalidOperationException("Expected a single, named type argument for one of our attributes.");
            }

            if (attributeData.ConstructorArguments is not [var firstArg, var paramsArg])
            {
                throw new InvalidOperationException($"Expected two args but instead got {attributeData.ConstructorArguments.Length}.");
            }

            if (paramsArg.Kind != TypedConstantKind.Array)
            {
                throw new InvalidOperationException("Expected an array for the second argument.");
            }

            var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();

            // We do allow a struct -> class and class -> struct
            if ((namedSourceSymbol.TypeKind == TypeKind.Enum && namedTargetSymbol.TypeKind != TypeKind.Enum)
                || (namedSourceSymbol.TypeKind != TypeKind.Enum && namedTargetSymbol.TypeKind == TypeKind.Enum))
            {
                var location = attributeData.GetLocation();
                diagnosticsBuilder.Add(Diagnostic.Create(
                    Diagnostics.InvalidTypeKind,
                    location,
                    messageArgs: [namedTargetSymbol.TypeKind, namedSourceSymbol.TypeKind]
                ));
                sourceType = null;
                targetType = null;
                diagnostics = diagnosticsBuilder;
                members = null;
                return false;
            }

            var memberBuilder = ImmutableHashSet.CreateBuilder<string>();

            ImmutableHashSet<string> sourceMembers;
            if (namedSourceSymbol.TypeKind == TypeKind.Enum)
            {
                sourceMembers = namedSourceSymbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Select(fs => fs.Name)
                    .ToImmutableHashSet();
            }
            else
            {
                sourceMembers = namedSourceSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(ps => ps.DeclaredAccessibility == Accessibility.Public)
                    .Select(ps => ps.Name)
                    .ToImmutableHashSet();
            }

            bool ValidateAndAddMember(TypedConstant typedConstant)
            {
                if (typedConstant.IsNull)
                {
                    // TODO: Get location more specific to this argument
                    var location = attributeData.GetLocation();

                    diagnosticsBuilder.Add(Diagnostic.Create(
                        Diagnostics.NullArgument,
                        location,
                        messageArgs: "null"
                    ));
                    return false;
                }

                var member = (string)typedConstant.Value!;

                if (!memberBuilder.Add(member))
                {
                    // TODO: Get location more specific to this argument
                    var location = attributeData.GetLocation();

                    diagnosticsBuilder.Add(Diagnostic.Create(
                        Diagnostics.DuplicateArgument,
                        location,
                        messageArgs: member
                    ));
                    return true;
                }

                // Check that it exists on the source type
                if (!sourceMembers.Contains(member))
                {
                    // TODO: Get location more specific to this argument
                    var location = attributeData.GetLocation();

                    diagnosticsBuilder.Add(Diagnostic.Create(
                        Diagnostics.InvalidPropertyName,
                        location,
                        messageArgs: [member, namedSourceSymbol.Name]
                    ));
                    return true;
                }

                return true;
            }

            if (!ValidateAndAddMember(firstArg))
            {
                sourceType = null;
                targetType = null;
                diagnostics = diagnosticsBuilder;
                members = memberBuilder.ToImmutableHashSet();
                return false;
            }

            foreach (var paramArg in paramsArg.Values)
            {
                if (!ValidateAndAddMember(paramArg))
                {
                    sourceType = null;
                    targetType = null;
                    diagnostics = diagnosticsBuilder;
                    members = memberBuilder.ToImmutableHashSet();
                    return false;
                }
            }

            sourceType = namedSourceSymbol;
            targetType = namedTargetSymbol;
            diagnostics = diagnosticsBuilder;
            members = memberBuilder.ToImmutableHashSet();
            return true;
        }

        public static (TypeSpec? Spec, ImmutableArray<Diagnostic> Diagnostics) CreateOmit(ISymbol targetSymbol, AttributeData attributeData)
        {
            // TODO: Validate more closely that it's _our_ OmitAttribute

            if (!TryCreate(
                targetSymbol,
                attributeData,
                out var targetType,
                out var sourceType,
                out var members,
                out var diagnostics))
            {
                return (null, diagnostics.ToImmutableArray());
            }

            if (targetType.TypeKind is TypeKind.Class or TypeKind.Struct)
            {
                var nonOmittedProperties = sourceType.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(ps => ps.DeclaredAccessibility == Accessibility.Public)
                    .Where(ps => !members.Contains(ps.Name))
                    .ToImmutableArray();

                return (new NonEnumTypeSpec(targetType, sourceType, nonOmittedProperties), diagnostics.ToImmutableArray());
            }
            else if (targetType.TypeKind == TypeKind.Enum)
            {
                var nonOmittedFields = sourceType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(fs => !members.Contains(fs.Name))
                    .ToImmutableArray();

                return (new EnumTypeSpec(targetType, sourceType, nonOmittedFields), diagnostics.ToImmutableArray());
            }
            else
            {
                throw new InvalidOperationException($"TypeKind of {targetType.TypeKind} is not supported.");
            }
        }

        public static (TypeSpec? Spec, ImmutableArray<Diagnostic> Diagnostics) CreatePick(ISymbol targetSymbol, AttributeData attributeData)
        {
            // TODO: Validate more closely that it's _our_ PickAttribute

            if (!TryCreate(
                targetSymbol,
                attributeData,
                out var targetType,
                out var sourceType,
                out var members,
                out var diagnostics))
            {
                return (null, diagnostics.ToImmutableArray());
            }

            if (targetType.TypeKind is TypeKind.Class or TypeKind.Struct)
            {
                var pickedProperties = sourceType.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(ps => ps.DeclaredAccessibility == Accessibility.Public)
                    .Where(ps => members.Contains(ps.Name))
                    .ToImmutableArray();

                return (new NonEnumTypeSpec(targetType, sourceType, pickedProperties), diagnostics.ToImmutableArray());
            }
            else if (targetType.TypeKind == TypeKind.Enum)
            {
                var pickedProperties = sourceType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(fs => members.Contains(fs.Name))
                    .ToImmutableArray();

                return (new EnumTypeSpec(targetType, sourceType, pickedProperties), diagnostics.ToImmutableArray());
            }
            else
            {
                throw new InvalidOperationException($"TypeKind of {targetType.TypeKind} is not supported.");
            }
        }
    }

    internal sealed class NonEnumTypeSpec : TypeSpec
    {
        internal NonEnumTypeSpec(INamedTypeSymbol targetType, INamedTypeSymbol sourceType, ImmutableArray<IPropertySymbol> members)
            : base(targetType)
        {
            SourceType = sourceType;
            Members = members;
        }

        public INamedTypeSymbol SourceType { get; }
        public ImmutableArray<IPropertySymbol> Members { get; }

        public override void Emit(IndentedTextWriter writer)
        {
            writer.WriteLine("// <auto-generated/>");
            writer.WriteLine($"namespace {TargetType.ContainingNamespace.ToDisplayString()}");
            writer.WriteLine("{");
            writer.Indent++;

            var type = TargetType.IsValueType ? "struct" : "class";

            writer.WriteLine($"partial {type} {TargetType.Name}");
            writer.WriteLine("{");
            writer.Indent++;

            var sourceConstructors = SourceType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(ms => ms.MethodKind == MethodKind.Constructor);

            var allSourceProperties = SourceType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(ps => ps.DeclaredAccessibility == Accessibility.Public)
                .Select(ps => (ps.Name, ps.Type));

            bool hasRecordLikeConstructor = false;

            foreach (var sourceConstructor in sourceConstructors)
            {
                var parameters = sourceConstructor.Parameters
                    .Select(p => (p.Name, p.Type));

                if (allSourceProperties.SequenceEqual(parameters, NameTypeEqualityComparer.Instance))
                {
                    hasRecordLikeConstructor = true;
                }
            }

            if (hasRecordLikeConstructor)
            {
                // Generate a record like constructor
                var constructorArgs = string.Join(
                    ", ",
                    Members.Select(m => $"{m.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {m.Name}")
                );
                writer.WriteLine($"public {TargetType.Name}({constructorArgs})");
                writer.WriteLine("{");
                writer.Indent++;

                // Assign all parameters to members
                foreach (var member in Members)
                {
                    writer.WriteLine($"this.{member.Name} = {member.Name};");
                }

                // Close constructor
                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLineNoTabs(string.Empty);
            }

            foreach (var member in Members)
            {
                writer.WriteLine($"/// <inheritdoc cref=\"{SourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{member.Name}\" />");

                var setter = member.SetMethod != null
                    ? member.IsRequired
                        ? "init; "
                        : "set; "
                    : string.Empty;

                // TODO: Copy getters and setters from source
                writer.WriteLine($"public {member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {member.Name} {{ get; {setter}}}");
            }

            writer.WriteLineNoTabs(string.Empty);
            writer.WriteLine($"public static {TargetType.Name} From({SourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} value)");
            writer.WriteLine("{");
            writer.Indent++;

            // Use record like constructor if we created it.
            if (hasRecordLikeConstructor)
            {
                var constructorArgs = string.Join(
                    ", ",
                    Members.Select(m => $"value.{m.Name}")
                );
                writer.WriteLine($"return new {TargetType.Name}({constructorArgs});");
            }
            else
            {
                writer.WriteLine($"return new {TargetType.Name}");
                writer.WriteLine("{");
                writer.Indent++;

                foreach (var member in Members)
                {
                    writer.WriteLine($"{member.Name} = value.{member.Name},");
                }

                // Close object creation
                writer.Indent--;
                writer.WriteLine("};");
            }

            // Close From method
            writer.Indent--;
            writer.WriteLine("}");

            // TODO: Generate Apply method

            // Close type definition
            writer.Indent--;
            writer.WriteLine("}");

            // Close namespace
            writer.Indent--;
            writer.Write("}");
        }

        private class NameTypeEqualityComparer : IEqualityComparer<(string Name, ITypeSymbol Type)>
        {
            public static NameTypeEqualityComparer Instance = new();

            public bool Equals((string Name, ITypeSymbol Type) x, (string Name, ITypeSymbol Type) y)
            {
                return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase)
                    && SymbolEqualityComparer.Default.Equals(x.Type, y.Type);
            }

            public int GetHashCode((string Name, ITypeSymbol Type) obj)
            {
                throw new NotImplementedException();
            }
        }
    }

    internal sealed class EnumTypeSpec : TypeSpec
    {
        public EnumTypeSpec(INamedTypeSymbol targetType, INamedTypeSymbol sourceType, ImmutableArray<IFieldSymbol> fields)
            : base(targetType)
        {
            SourceType = sourceType;
            Fields = fields;
        }

        public INamedTypeSymbol SourceType { get; }
        public ImmutableArray<IFieldSymbol> Fields { get; }

        public override void Emit(IndentedTextWriter writer)
        {
            writer.WriteLine("// <auto-generated/>");
            writer.WriteLine($"namespace {TargetType.ContainingNamespace.ToDisplayString()}");
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"partial enum {TargetType.Name}");
            writer.WriteLine("{");
            writer.Indent++;

            var sourceFields = SourceType.GetMembers()
                .OfType<IFieldSymbol>()
                .ToImmutableDictionary(fs => fs.Name);

            foreach (var field in Fields)
            {
                writer.WriteLine($"{field.Name} = {field.ConstantValue},");
            }

            // Close enum definition
            writer.Indent--;
            writer.WriteLine("}");

            // Close namespace definition
            writer.Indent--;
            writer.Write("}");
        }
    }
}