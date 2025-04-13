using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TypeFest.Net.SourceGenerator
{
    [Generator]
    public sealed class TypeFestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pickSpecAndDiagnostics = context.SyntaxProvider.ForAttributeWithMetadataName(
                    "TypeFest.Net.PickAttribute`1",
                    predicate: static (node, _) => true,
                    transform: static (context, _) => PartialTypeSpec.CreatePick(context.TargetSymbol, context.Attributes[0])
                )
                .WithTrackingName("Pick");

            var pickSpec = ReportDiagnostics(context, pickSpecAndDiagnostics);

            context.RegisterSourceOutput(pickSpec, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.FullName}.Pick.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            var omitSpecsAndDiagnostics = context.SyntaxProvider.ForAttributeWithMetadataName(
                    "TypeFest.Net.OmitAttribute`1",
                    predicate: static (node, _) => true,
                    transform: static (context, _) => PartialTypeSpec.CreateOmit(context.TargetSymbol, context.Attributes[0])
                )
                .WithTrackingName("Omit");

            var omitSpecs = ReportDiagnostics(context, omitSpecsAndDiagnostics);

            context.RegisterSourceOutput(omitSpecs, (context, spec) =>
            {
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                using var writer = new IndentedTextWriter(sw);

                spec.Emit(writer);

                context.AddSource($"{spec.FullName}.Omit.g.cs",
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

                context.AddSource($"{spec.SourceType}.MapInto.g.cs",
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

                context.AddSource($"{spec.SourceType}.MapFrom.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));
            });
        }

        private static IncrementalValuesProvider<T> ReportDiagnostics<T>(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<(T? Spec, ImmutableArray<Diagnostic> Diagnostics)> source)
        {
            var diagnostics = source
                .Select((v, _) => v.Diagnostics)
                .Where(d => d.Length > 0);

            context.RegisterSourceOutput(diagnostics, (context, diagnostics) =>
            {
                foreach (var diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            });

            return source
                .Select((v, _) => v.Spec)
                .Where(i => i != null)!;
        }
    }
}