using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using TypeFest.Net.CodeFixers;

namespace TypeFest.Net.Analyzers.Tests;

public class UpdateTypeCodeFixProviderTests : CSharpCodeFixTest<TypeFestAnalyzer, UpdateTypeFixProvider, DefaultVerifier>
{
    [Fact]
    public async Task Test()
    {
        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Person>("Name")|}]
            public partial class MyThing;
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>("Name")]
            public partial class MyThing
            {
                /// <inheritdoc cref="Person.Name"/>
                public string Name { get; set; }
            }
            """
        );
    }

    [Fact]
    public async Task ExistingMembers_Works()
    {
        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Person>("Name")|}]
            public class MyThing
            {
                public string MyAction()
                {
                    return "Hello!";
                }
            }
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>("Name")]
            public class MyThing
            {
                public string MyAction()
                {
                    return "Hello!";
                }
                /// <inheritdoc cref="Person.Name"/>
                public string Name { get; set; }
            }
            """
        );
    }

    private async Task RunAsync([StringSyntax("C#-test")] string testCode, [StringSyntax("C#-test")] string fixedCode)
    {
        TestCode = testCode;
        FixedCode = fixedCode;
        TestState.AnalyzerConfigFiles.Add(("/.editorconfig", """
        is_global = true
        build_property.TypeFestNet_GenerateMode = CodeFix
        """));
        TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        TestState.AdditionalReferences.Add(typeof(PickAttribute<>).Assembly);
        var staticSource = """
        namespace Test;

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        """;
        TestState.Sources.Add(("Person.cs", staticSource));
        FixedState.Sources.Add(("Person.cs", staticSource));

        await RunAsync(TestContext.Current.CancellationToken);
    }
}