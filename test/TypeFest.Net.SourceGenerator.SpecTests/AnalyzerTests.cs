using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace TypeFest.Net.SourceGenerator.SpecTests;

public class AnalyzerTests : CSharpAnalyzerTest<TypeFestAnalyzer, DefaultVerifier>
{
    // TODO: Move these to CodeFixer tests
    // [Fact]
    // public async Task OutOfSyncType_CausesDiagnostic()
    // {
    //     await RunAsync(
    //         """
    //         using TypeFest.Net;

    //         namespace Test;

    //         [{|TF0006:Omit<Person>("Name")|}]
    //         public partial class OtherPerson;
    //         """
    //     );
    // }

    // [Fact]
    // public async Task UpToDateType_CausesNoDiagnostics()
    // {
    //     await RunAsync(
    //         """
    //         using TypeFest.Net;

    //         namespace Test;

    //         [Pick<Person>("Age")]
    //         public partial class OtherPerson
    //         {
    //             /// <inheritdoc cref="global::Test.Person" />
    //             public int Age { get; }
    //         }
    //         """
    //     );
    // }

    [Theory]
    [InlineData("class", "Omit<Color>(\"Red\")")]
    [InlineData("struct", "Omit<Color>(\"Red\")")]
    [InlineData("record", "Omit<Color>(\"Red\")")]
    [InlineData("record struct", "Omit<Color>(\"Red\")")]
    [InlineData("class", "Pick<Color>(\"Red\")")]
    [InlineData("struct", "Pick<Color>(\"Red\")")]
    [InlineData("record", "Pick<Color>(\"Red\")")]
    [InlineData("record struct", "Pick<Color>(\"Red\")")]
    //TODO: Test analyzer mode (no partial) amd test enum
    public async Task TypeKind_Mismatch_CausesDiagnostic(string typeKindString, string attribute)
    {
        await RunAsync(
            $$"""
            using TypeFest.Net;

            namespace Test;

            [{|TF0004:{{attribute}}|}]
            public partial {{typeKindString}} TestTarget;
            """
        );
    }

    [Theory]
    [InlineData("{|TF0002:\"Name\"|}")]
    [InlineData("\"Age\", {|TF0002:\"Name\"|}")]
    [InlineData("[{|TF0002:\"Name\"|}]")]
    public async Task DuplicateMemberNames_CausesDiagnostic(string extraArg)
    {
        await RunAsync(
            $"""
            using TypeFest.Net;

            namespace Test;

            [Omit<Person>("Name", {extraArg})]
            public partial class MyThing;
            """
        );
    }

    [Theory]
    [InlineData("{|TF0001:null|}")]
    [InlineData("\"Name\", {|TF0001:null|}")]
    [InlineData("{|TF0001:null|}, \"Name\"")]
    [InlineData("\"Id\", \"Name\", {|TF0001:null|}")]
    [InlineData("\"Id\", [\"Name\", {|TF0001:null|}]")]
    [InlineData("\"Id\", new string[] { \"Name\", {|TF0001:null|} }")]
    [InlineData("\"Id\", new[] { \"Name\", {|TF0001:null|} }")]
    public async Task NullMemberName(string pickArgs)
    {
        await RunAsync(
            $$"""
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>({{pickArgs}})]
            public partial class EditPerson;
            """
        );
    }

    [Fact]
    public async Task InvalidMemberName()
    {
        await RunAsync("""
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>({|TF0003:"IdWrong"|})]
            public partial class EditPerson;
            """
        );
    }

    private async Task RunAsync([StringSyntax("C#-test")]string source)
    {
        TestCode = source;

        // TODO: Do this for the code fixer tests
        // TestState.AnalyzerConfigFiles.Add(("/.editorconfig", """
        // is_global = true
        // build_property.TypeFestNet_GenerateMode = CodeFix
        // """));

        TestState.AdditionalReferences.Add(typeof(PickAttribute<>).Assembly);
        TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        TestState.Sources.Add(("Person.cs",
        """
        using System;

        namespace Test;

        public class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public struct PersonStruct
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public record PersonRecord
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public record struct PersonRecordStruct
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public enum Color
        {
            Red,
            Green,
            Blue,
            Yellow,
            Purple,
        }
        """));

        await RunAsync(TestContext.Current.CancellationToken);
    }
}
