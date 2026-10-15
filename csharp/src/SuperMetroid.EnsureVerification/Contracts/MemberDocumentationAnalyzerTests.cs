using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks that XML documentation is required for declarations at every accessibility and static state.</summary>
internal static class MemberDocumentationAnalyzerTests
{
    /// <summary>
    /// Confirms private static and instance members, fields, enum values, and extension
    /// blocks are reported when undocumented, while genuine XML comments satisfy the rule.
    /// </summary>
    internal static void VerifyMemberDocumentationAnalyzer()
    {
        const string source = """
            /// <summary>Fixture owner.</summary>
            internal sealed class Fixture
            {
                // A plain comment does not document this private static method.
                private static void UndocumentedStatic() { }
                private void UndocumentedInstance() { }
                private static int UndocumentedField;

                /// <summary>Documented method.</summary>
                private static void Documented() { }
            }

            /// <summary>Fixture values.</summary>
            internal enum Mode
            {
                /// <summary>Documented value.</summary>
                Documented,
                Undocumented
            }

            /// <summary>Fixture extensions.</summary>
            internal static class FixtureExtensions
            {
                // A plain comment does not document an extension block.
                extension(Fixture self)
                {
                    /// <summary>Reads a fixture value through an extension.</summary>
                    internal int DocumentedExtension() => 1;
                }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create("MemberDocumentationCase", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Diagnostic[] errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid documentation fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        Diagnostic[] diagnostics = compilation.WithAnalyzers([new MemberDocumentationAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        string[] actual = diagnostics.Select(d => d.GetMessage()).OrderBy(message => message).ToArray();
        string[] expected = new[]
        {
            "FieldDeclaration needs an XML documentation comment",
            "ExtensionBlockDeclaration needs an XML documentation comment",
            "Undocumented needs an XML documentation comment",
            "UndocumentedInstance needs an XML documentation comment",
            "UndocumentedStatic needs an XML documentation comment",
        }.OrderBy(message => message).ToArray();
        if (!actual.SequenceEqual(expected) || diagnostics.Any(d =>
                d.Id != MemberDocumentationAnalyzer.Id || d.Severity != DiagnosticSeverity.Error))
        {
            throw new InvalidOperationException(
                $"Expected documentation errors for every undocumented fixture member, got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}].");
        }
    }
}
