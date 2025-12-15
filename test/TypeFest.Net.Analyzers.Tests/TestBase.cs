using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace TypeFest.Net.SourceGenerator.SpecTests;

public abstract class TestBase
{
    protected static async Task<GeneratorDriverRunResult> RunAsync(string source)
    {
        return await RunCoreAsync(source, TestContext.Current.CancellationToken);
    }

    private static async Task<GeneratorDriverRunResult> RunCoreAsync(string source, CancellationToken cancellationToken)
    {
        var projectName = $"TestProject-{Guid.NewGuid()}";

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);

        var project = new AdhocWorkspace().CurrentSolution
            .AddProject(projectName, projectName, LanguageNames.CSharp)
            .WithCompilationOptions(compilationOptions)
            .WithParseOptions(parseOptions);

        var dotNetDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

        project = project
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(PickAttribute<>).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(MetadataReference.CreateFromFile(Path.Join(dotNetDir, "System.Runtime.dll")));

        project = project
            .AddDocument("Test.cs", SourceText.From(source, Encoding.UTF8))
            .Project;

        var compilation = await project.GetCompilationAsync(cancellationToken)
            ?? throw new InvalidOperationException();
        var diagnostics = compilation.GetDiagnostics(cancellationToken);

        Debug.WriteLine("Original diagnostics:");

        foreach (var d in diagnostics)
        {
            Debug.WriteLine($"\t{d}");
        }

        var generator = new TypeFestGenerator().AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true),
            parseOptions: parseOptions
        );

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var updatedDiagnostics, cancellationToken);

        Debug.WriteLine("Updated diagnostics:");

        foreach (var d in updatedDiagnostics)
        {
            Debug.WriteLine($"\t{d}");
        }

        return driver.GetRunResult();
    }

    protected static async Task RunAndCompareAsync(string source, string output)
    {
        var runResult = await RunAsync(source);

        var result = Assert.Single(runResult.Results);

        var generatedSource = Assert.Single(result.GeneratedSources);

        var text = generatedSource.SourceText.ToString();

        Assert.Equal(output, text);
    }
}