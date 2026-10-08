using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.Core;
using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    private static readonly MetadataReference[] References = BuildReferences();

    private static void VerifyAnalyzer()
    {
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
