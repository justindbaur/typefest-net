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

    [Fact]
    public async Task RequiredInitProperty()
    {

        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Person>("Occupation")|}]
            public partial class MyThing;
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Person>("Occupation")]
            public partial class MyThing
            {
                /// <inheritdoc cref="Person.Occupation"/>
                public required string Occupation { get; init; }
            }
            """
        );
    }

    [Fact]
    public async Task FullyQualifiedNamespace()
    {

        await RunAsync(
            """
            using TypeFest.Net;

            namespace AnotherNamespace;

            [{|TF0006:Pick<Test.Person>("Name")|}]
            public partial class MyThing;
            """,
            """
            using TypeFest.Net;

            namespace AnotherNamespace;

            [Pick<Test.Person>("Name")]
            public partial class MyThing
            {
                /// <inheritdoc cref="Test.Person.Name"/>
                public string Name { get; set; }
            }
            """
        );
    }

    [Fact]
    public async Task Enum_Works()
    {
        await RunAsync(
            """
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Color>("Red")|}]
            public enum MyEnum;
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Color>("Red")]
            public enum MyEnum
            {
                Red = 2,
            }
            """
        );
    }

    [Fact]
    public async Task Record_Works()
    {
        await RunAsync("""
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<PersonRecord>("Name")|}]
            public record MyPersonRecord;
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<PersonRecord>("Name")]
            public record MyPersonRecord
            {
                public MyPersonRecord(string Name)
                {
                    this.Name = Name;
                }

                /// <inheritdoc cref="PersonRecord.Name"/>
                public string Name { get; init; }
            }
            """
        );
    }

    [Fact]
    public async Task ClassWithConstructor_Works()
    {
        await RunAsync("""
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Dog>("Breed", "Name")|}]
            public class CreateDog;
            """,
            """
            using TypeFest.Net;

            namespace Test;

            [Pick<Dog>("Breed", "Name")]
            public class CreateDog
            {
                public CreateDog(string breed, string name)
                {
                    Breed = breed;
                    Name = name;
                }

                /// <inheritdoc cref="Dog.Breed"/>
                public string Breed { get; }

                /// <inheritdoc cref="Dog.Name"/>
                public string Name { get; }
            }
            """
        );
    }

    [Fact]
    public async Task CanSimplifyNamespace_Does()
    {
        await RunAsync("""
            using System.Net;
            using TypeFest.Net;

            namespace Test;

            [{|TF0006:Pick<Data>("Address")|}]
            public class MyData;
            """,
            """
            using System.Net;
            using TypeFest.Net;

            namespace Test;

            [Pick<Data>("Address")]
            public class MyData
            {
                /// <inheritdoc cref="Data.Address"/>
                public IPAddress Address { get; set; }
            }
            """
        );
    }

    private async Task RunAsync([StringSyntax("C#-test")] string testCode, [StringSyntax("C#-test")] string fixedCode)
    {
        TestCode = testCode;
        FixedCode = fixedCode;
        TestState.SetGenerateMode(false);
        TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        TestState.AdditionalReferences.Add(typeof(PickAttribute<>).Assembly);
        var staticSource = """
        using System;

        namespace Test;

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public required string Occupation { get; init; }
        }

        public record PersonRecord(string Name, int Age);

        public class Dog
        {
            public Dog(Guid id, string breed, string name)
            {
                Id = id;
                Breed = breed;
                Name = name;
            }

            public Guid Id { get; }
            public string Breed { get; }
            public string Name { get; }
        }

        public class Data
        {
            public System.Net.IPAddress Address { get; set; }
        }

        public enum Color
        {
            Blue,
            Green,
            Red,
            Purple,
        }
        """;
        TestState.Sources.Add(("Person.cs", staticSource));
        FixedState.Sources.Add(("Person.cs", staticSource));

        await RunAsync(TestContext.Current.CancellationToken);
    }
}