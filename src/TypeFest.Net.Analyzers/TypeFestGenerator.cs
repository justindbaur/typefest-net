using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TypeFest.Net.Analyzer.Shared;

namespace TypeFest.Net.Analyzers
{
    [Generator]
    public sealed class TypeFestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var usingSourceGen = context.AnalyzerConfigOptionsProvider
                .Select((options, _) => options.IsUsingSourceGenMode());

            var pickSpec = context.SyntaxProvider.ForAttributeWithMetadataName(
                    "TypeFest.Net.PickAttribute`1",
                    predicate: static (node, _) => true,
                    transform: PartialTypeSpec.CreatePick
                )
                .FilterDiagnostics()
                .WithTrackingName("Pick")
                .Combine(usingSourceGen)
                .Where(v => v.Right)
                .Select((v, _) => v.Left);

            context.RegisterSourceOutput(pickSpec, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.TargetType.FilenameHint}.Pick.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            var omitSpecs = context.SyntaxProvider.ForAttributeWithMetadataName(
                    "TypeFest.Net.OmitAttribute`1",
                    predicate: static (node, _) => true,
                    transform: PartialTypeSpec.CreateOmit
                )
                .FilterDiagnostics()
                .WithTrackingName("Omit")
                .Combine(usingSourceGen)
                .Where(v => v.Right)
                .Select((v, _) => v.Left);

            context.RegisterSourceOutput(omitSpecs, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.TargetType.FilenameHint}.Omit.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            var mapIntoSpecsAndDiagnostics = context.SyntaxProvider.ForAttributeWithMetadataName(
                "TypeFest.Net.MapIntoAttribute`1",
                predicate: (node, _) => true,
                transform: (context, _) => MapInfo.Create(context.TargetSymbol, context.Attributes[0], true)
            )
                .WithTrackingName("MapInto");

            var mapIntoSpecs = ReportDiagnostics(context, mapIntoSpecsAndDiagnostics);

            context.RegisterSourceOutput(mapIntoSpecs, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.PartialType.FilenameHint}.MapInto.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            var mapFromSpecsAndDiagnostics = context.SyntaxProvider.ForAttributeWithMetadataName(
                "TypeFest.Net.MapFromAttribute`1",
                predicate: (node, _) => true,
                transform: (context, _) => MapInfo.Create(context.TargetSymbol, context.Attributes[0], false)
            )
                .WithTrackingName("MapFrom");

            var mapFromSpecs = ReportDiagnostics(context, mapFromSpecsAndDiagnostics);

            context.RegisterSourceOutput(mapFromSpecs, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.PartialType.FilenameHint}.MapFrom.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            });
        }

        private static IncrementalValuesProvider<T> ReportDiagnostics<T>(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<Result<T?>> source)
        {
            var diagnostics = source
                .Select((v, _) => v.Errors)
                .Where(d => d.Count > 0);

            context.RegisterSourceOutput(diagnostics, (context, diagnostics) =>
            {
                foreach (var diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic.ToDiagnostic());
                }
            });

            return source
                .Select((v, _) => v.Item)
                .Where(i => i != null)!;
        }
    }
}