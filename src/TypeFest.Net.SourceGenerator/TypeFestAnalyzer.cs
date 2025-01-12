// using System;
// using System.Collections.Immutable;

// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.Diagnostics;

// namespace TypeFest.Net.SourceGenerator;

// [DiagnosticAnalyzer(LanguageNames.CSharp)]
// public sealed class TypeFestAnalyzer 
//     : DiagnosticAnalyzer
// {
//     public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
//     [
//         Diagnostics.DuplicateArgument,
//     ];

//     public override void Initialize(AnalysisContext context)
//     {
//         throw new NotImplementedException();
//     }
// }