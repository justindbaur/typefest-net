using Microsoft.CodeAnalysis;

namespace TypeFest.Net.SourceGenerator.SpecTests;

public class PickTests : TestBase
{
    [Fact]
    public async Task SingleArg()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name")]
            public partial class EditPerson;

            public class Person
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
            }
            """,
            """
            // <auto-generated/>
            namespace TestNamespace
            {
                partial class EditPerson
                {
                    /// <inheritdoc cref="global::TestNamespace.Person.Name" />
                    public string Name { get; set; }
                }
            }
            """
        );
    }

    [Fact]
    public async Task MultiArg()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name", "Age", "Occupation")]
            public partial class EditPerson;

            public class Person
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public int Age { get; set; }
                public string Occupation { get; set; }
            }
            """,
            """
            // <auto-generated/>
            namespace TestNamespace
            {
                partial class EditPerson
                {
                    /// <inheritdoc cref="global::TestNamespace.Person.Name" />
                    public string Name { get; set; }
                    /// <inheritdoc cref="global::TestNamespace.Person.Age" />
                    public int Age { get; set; }
                    /// <inheritdoc cref="global::TestNamespace.Person.Occupation" />
                    public string Occupation { get; set; }
                }
            }
            """);
    }

    [Fact]
    public async Task ParamsAsArray()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name", ["Age", "Occupation"])]
            public partial class EditPerson;

            public class Person
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
                public int Age { get; set; }
                public string Occupation { get; set; }
            }
            """,
            """
            // <auto-generated/>
            namespace TestNamespace
            {
                partial class EditPerson
                {
                    /// <inheritdoc cref="global::TestNamespace.Person.Name" />
                    public string Name { get; set; }
                    /// <inheritdoc cref="global::TestNamespace.Person.Age" />
                    public int Age { get; set; }
                    /// <inheritdoc cref="global::TestNamespace.Person.Occupation" />
                    public string Occupation { get; set; }
                }
            }
            """);
    }

    [Fact]
    public async Task Enum()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Test>("One", "Two")]
            public partial enum PickTest;

            public enum Test
            {
                One,
                Two,
                Three,
            }
            """,
            """
            // <auto-generated/>
            namespace TestNamespace
            {
                partial enum PickTest
                {
                    /// <inheritdoc cref="global::TestNamespace.Test.One" />
                    One = 0,
                    /// <inheritdoc cref="global::TestNamespace.Test.Two" />
                    Two = 1,
                }
            }
            """);
    }

    [Fact]
    public async Task Record()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name", "Age")]
            public partial record EditPerson;

            public record Person(Guid Id, string Name, int Age);
            """,
            """
            // <auto-generated/>
            namespace TestNamespace
            {
                partial record EditPerson
                {
                    /// <inheritdoc cref="global::TestNamespace.Person.Name" />
                    public string Name { get; init; }
                    /// <inheritdoc cref="global::TestNamespace.Person.Age" />
                    public int Age { get; init; }
                }
            }
            """);
    }

    [Theory]
    [InlineData("class", "Class")]
    [InlineData("struct", "Struct")]
    public async Task TargetEnum_SourceNotEnum(string kind, string kindString)
    {
        var result = await RunAsync($$"""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name")]
            public partial enum EditPerson;

            public {{kind}} Person
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
            }
            """
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("TF0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal($"Cannot create a Enum based on a {kindString}.", diagnostic.GetMessage());
    }

    [Theory]
    [InlineData("class", "Class")]
    [InlineData("struct", "Struct")]
    public async Task TargetNotEnum_SourceEnum(string kind, string kindString)
    {
        var result = await RunAsync($$"""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Test>("One")]
            public partial {{kind}} PickTest;

            public enum Test
            {
                One,
                Two,
            }
            """
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("TF0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal($"Cannot create a {kindString} based on a Enum.", diagnostic.GetMessage());
    }

    [Fact]
    public async Task DuplicateMemberNames()
    {
        var result = await RunAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("Name", "Name")]
            public partial class EditPerson;

            public class Person
            {
                public Guid Id { get; set; }
                public string Name { get; set; }
            }
            """
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("TF0002", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("'Name' is a duplicate argument and will be ignored.", diagnostic.GetMessage());
    }

    [Fact]
    public async Task InvalidMemberName()
    {
        var result = await RunAsync("""
            using TypeFest.Net;

            namespace TestNamespace;

            [Pick<Person>("IdWrong")]
            public partial class EditPerson;

            public class Person
            {
                public Guid Id { get; set; }
            }
            """
        );

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("TF0003", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("'IdWrong' is not a valid member on 'Person'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task InGlobalNamespace()
    {
        await RunAndCompareAsync("""
            using TypeFest.Net;
            public class Person
            {
                public string Name { get; set; }
            }

            [Pick<Person>("Name")]
            partial class EditPerson;
            """,
            // TODO: Do we are about the extra new line?
            """
            // <auto-generated/>
            partial class EditPerson
            {
                /// <inheritdoc cref="global::Person.Name" />
                public string Name { get; set; }
            }

            """
        );
    }
}