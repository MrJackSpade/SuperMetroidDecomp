using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.Core;
using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.EnsureVerification;

/// <summary>Runs Ensure and XML documentation analyzer fixtures.</summary>
internal static partial class Program
{
    /// <summary>References platform and Core assemblies for in-memory Roslyn compilations.</summary>
    private static readonly MetadataReference[] References = BuildReferences();

    /// <summary>Confirms recognized Ensure suggestions and intentionally unsupported guard forms.</summary>
    private static void VerifyAnalyzer()
    {
        VerifyDocumentationAnalyzer();
        VerifySnippet("using System; class C { public void M(object value) { ArgumentNullException.ThrowIfNull(value); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");
        VerifySnippet("using System; class C { public void M(int value) { ArgumentOutOfRangeException.ThrowIfNegative(value); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.AtLeastZero");
        VerifySnippet("using System; class C { public void M(object? value) { if (value is null) throw new ArgumentNullException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");
        VerifySnippet("using System; class C { public void M(int value) { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.AtLeastZero");
        VerifySnippet("using System; class C { public void M(int value) { if (value < 0 || value > 64) throw new ArgumentOutOfRangeException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.BetweenInclusive");
        VerifySnippet("using System; class C { public void M(int[] items) { if (items.Length != 4) throw new ArgumentOutOfRangeException(nameof(items)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.LengthEqual");
        VerifySnippet("using System; class C { public void M(int[] items) { if (items.Length != 4) throw new ArgumentException(\"Expected four items.\", nameof(items)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.LengthEqual");
        VerifySnippet("using System; class C { public object M(object? value) => value ?? throw new ArgumentNullException(nameof(value)); }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");

        // Guards without a shared Ensure operation are left as written.
        VerifySnippet("using System; class C { public void M(int value) { if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value)); } }");
        VerifySnippet("using System; class C { public void M(int value) { if (value is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(value)); } }");
        VerifySnippet("using System; enum Mode { A, B } class C { public void M(Mode mode) { if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode)); } }");
        VerifySnippet("using System; using SuperMetroid.Core; class C { public void M(object value) { Ensure.NotNull(value); } }");
        VerifySnippet("using System; using System.IO; class C { public void M(int value) { if (value < 0) throw new InvalidDataException(\"corrupt state\"); } }");
        VerifySnippet("using System; class C { private void PortNative(int spawnArgument) { if (spawnArgument < 0) throw new ArgumentOutOfRangeException(nameof(spawnArgument)); } }");
        VerifySnippet("using System; class C { public void M(int value) { if (value > 3) throw new ArgumentOutOfRangeException(nameof(value), \"Cartridge phase is invalid.\"); } }");
        VerifySnippet("using System; class C { public void M(int width, int height) { if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width)); } }");
    }

    /// <summary>Confirms missing-documentation errors and documented or implicit-symbol exclusions.</summary>
    private static void VerifyDocumentationAnalyzer()
    {
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                /// <summary>Documented private member.</summary>
                private void M() { }
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                private void M() { }
            }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                /// <summary>Documented method.</summary>
                void M() { }
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                /// <summary>Documented property.</summary>
                int P { get; set; }
                /// <summary>Documented field.</summary>
                int F;
                /// <summary>Documented event.</summary>
                event System.Action E { add { } remove { } }
                /// <summary>Documented constructor.</summary>
                C() { }
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                /// <summary>Documented property.</summary>
                public int P { get; set; }
                /// <summary>Documented event.</summary>
                public event System.Action? Changed;
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            partial class C { }
            /// <summary>Documented type part.</summary>
            partial class C { }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            record R(int Value);
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            /// <param name="Value">The value stored by the record.</param>
            record R(int Value);
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented enum.</summary>
            enum E { A }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented enum.</summary>
            enum E
            {
                /// <summary>Documented value.</summary>
                A
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            record R(int Value) { }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            /// <param name="Value">The value stored by the record.</param>
            record R(int Value) { }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented primary constructor.</summary>
            class C(int value) { }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented primary constructor.</summary>
            /// <param name="value">The constructor value.</param>
            class C(int value) { }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Partially documented primary constructor.</summary>
            /// <param name="first">The first constructor value.</param>
            class C(int first, int second) { }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented primary constructor.</summary>
            /// <param name="first">The first constructor value.</param>
            /// <param name="second">The second constructor value.</param>
            class C(int first, int second) { }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Partially documented struct primary constructor.</summary>
            /// <param name="first">The first constructor value.</param>
            struct S(int first, int second) { }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented struct primary constructor.</summary>
            /// <param name="first">The first constructor value.</param>
            /// <param name="second">The second constructor value.</param>
            struct S(int first, int second) { }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Partially documented positional record.</summary>
            /// <param name="First">The first stored value.</param>
            record R(int First, int Second);
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented positional record struct.</summary>
            /// <param name="First">The first stored value.</param>
            /// <param name="Second">The second stored value.</param>
            readonly record struct R(int First, int Second);
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            class C
            {
                /// <summary>Documented constant.</summary>
                const int F = 1;
                /// <summary>Documented event.</summary>
                public event System.Action? E;
                /// <summary>Documented conversion.</summary>
                public static explicit operator int(C value) => 0;
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented type.</summary>
            partial class P { }
            partial class P { }
            """);
        VerifyDocumentationSnippet("""
            System.Console.WriteLine("Top-level entry point.");
            """, outputKind: OutputKind.ConsoleApplication);
        VerifyDocumentationSnippet("""
            /// <summary>Documented receiver type.</summary>
            class C { }
            /// <summary>Documented extension owner.</summary>
            static class Extensions
            {
                extension(C value)
                {
                    /// <summary>Documented extension method.</summary>
                    internal void M() { }
                    /// <summary>Documented extension property.</summary>
                    internal int P => 0;
                }
            }
            """);
        VerifyDocumentationSnippet("""
            /// <summary>Documented receiver type.</summary>
            class C { }
            /// <summary>Documented extension owner.</summary>
            static class Extensions
            {
                extension(C value)
                {
                    internal void M() { }
                    /// <summary>Documented extension property.</summary>
                    internal int P => 0;
                }
            }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Documented receiver type.</summary>
            class C { }
            /// <summary>Documented extension owner.</summary>
            static class Extensions
            {
                extension(C value)
                {
                    /// <summary>Documented extension method.</summary>
                    internal void M() { }
                    internal int P => 0;
                }
            }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Contract implemented explicitly by the fixture.</summary>
            interface I
            {
                /// <summary>Performs the contract operation.</summary>
                void M();
            }
            /// <summary>Explicit contract implementation.</summary>
            class C : I
            {
                void I.M() { }
            }
            """, XmlDocumentationAnalyzer.RuleId);
        VerifyDocumentationSnippet("""
            /// <summary>Contract implemented explicitly by the fixture.</summary>
            interface I
            {
                /// <summary>Performs the contract operation.</summary>
                void M();
            }
            /// <summary>Explicit contract implementation.</summary>
            class C : I
            {
                /// <summary>Performs the interface contract for this implementation.</summary>
                void I.M() { }
            }
            """);
    }

    /// <summary>Compiles a declaration fixture and checks its XML documentation diagnostics.</summary>
    /// <param name="source">The source declarations to analyze.</param>
    /// <param name="expectedId">The sole expected diagnostic identifier, or <see langword="null"/> for none.</param>
    /// <param name="outputKind">The output kind used to compile the fixture.</param>
    private static void VerifyDocumentationSnippet(string source, string? expectedId = null,
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "XmlDocumentationAnalyzerCase",
            [tree], References,
            new CSharpCompilationOptions(outputKind, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid documentation fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new XmlDocumentationAnalyzer());
        var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (expectedId is null && diagnostics.Length == 0)
            return;
        if (expectedId is not null && diagnostics.Length == 1 && diagnostics[0].Id == expectedId &&
            diagnostics[0].Severity == DiagnosticSeverity.Error)
            return;
        throw new InvalidOperationException(
            $"Expected {(expectedId is null ? "no" : $"one {expectedId}")} documentation diagnostic, got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
    }

    /// <summary>Compiles a guard fixture and checks its Ensure usage diagnostic.</summary>
    /// <param name="source">The source statement to analyze.</param>
    /// <param name="expectedId">The expected diagnostic identifier, or <see langword="null"/> for none.</param>
    /// <param name="expectedMessage">Optional text expected in the diagnostic message.</param>
    private static void VerifySnippet(string source, string? expectedId = null, string? expectedMessage = null)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "EnsureAnalyzerCase",
            [tree],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new EnsureUsageAnalyzer());
        var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (expectedId is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected analyzer diagnostic: {diagnostics[0]} in {source}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != expectedId ||
            diagnostics[0].Severity != DiagnosticSeverity.Warning ||
            (expectedMessage is not null && !diagnostics[0].GetMessage().Contains(expectedMessage, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Expected one {expectedId} warning containing '{expectedMessage}', got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }

    /// <summary>Creates metadata references from the runtime and the loaded Core assembly.</summary>
    /// <returns>The unique metadata references required by the verifier's Roslyn compilations.</returns>
    private static MetadataReference[] BuildReferences()
    {
        string assemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("Trusted platform assemblies are unavailable.");
        return assemblies.Split(Path.PathSeparator)
            .Append(typeof(Ensure).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}
