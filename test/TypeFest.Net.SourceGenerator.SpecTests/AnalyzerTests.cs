using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace TypeFest.Net.SourceGenerator.SpecTests;

public class AnalyzerTests : CSharpAnalyzerTest<TypeFestAnalyzer, DefaultVerifier>
{
    [Fact]
    public async Task OutOfSyncType_CausesDiagnostic()
    {
        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Omit<Person>("Name")|}]
            public partial class OtherPerson;
            """
        );
    }

    [Fact]
    public async Task UpToDateType_CausesNoDiagnostics()
    {
        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>("Age")]
            public partial class OtherPerson
            {
                /// <inheritdoc cref="global::Test.Person" />
                public int Age { get; }
            }
            """
        );
    }

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
    [InlineData("{|TF0002:\"Name\"|}", true)]
    [InlineData("\"Age\", {|TF0002:\"Name\"|}", true)]
    [InlineData("[{|TF0002:\"Name\"|}]", true)]
    public async Task DuplicateMemberNames_CausesDiagnostic(string extraArg, bool expectExtra)
    {
        if (expectExtra)
        {
            // TODO: Remove TF0006 when mode selector is done
            TestState.ExpectedDiagnostics.Add(DiagnosticResult.CompilerWarning("TF0006")
                .WithLocation(5, 2));
        }

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
    [InlineData("{|TF0001:null|}", false)]
    [InlineData("\"Name\", {|TF0001:null|}", true)]
    [InlineData("{|TF0001:null|}, \"Name\"", true)]
    [InlineData("\"Id\", \"Name\", {|TF0001:null|}", true)]
    [InlineData("\"Id\", [\"Name\", {|TF0001:null|}]", true)]
    [InlineData("\"Id\", new string[] { \"Name\", {|TF0001:null|} }", true)]
    [InlineData("\"Id\", new[] { \"Name\", {|TF0001:null|} }", true)]
    public async Task NullMemberName(string pickArgs, bool expectExtra)
    {
        if (expectExtra)
        {
            TestState.ExpectedDiagnostics.Add(
                DiagnosticResult.CompilerWarning("TF0006")
                    .WithLocation(5, 2)
            );
        }

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
