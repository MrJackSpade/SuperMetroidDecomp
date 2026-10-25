using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1418 enum-value rule, SME6450.</summary>
internal static class EnumValueReadAnalyzerTests
{
    /// <summary>The enums and the encoding-owner attribute the fixtures use.</summary>
    private const string Types = """
        enum Draw { Idle, Wave }
        [System.Flags] enum Items { None = 0, Varia = 1, Gravity = 2 }
        sealed class SnesEncodingOwnerAttribute : System.Attribute { }

        """;

    /// <summary>The advice every SME6450 message ends with.</summary>
    private const string Advice = "; translate it in an encoding codec, dispatch through a behaviour reference, or look it up by identity";

    /// <summary>One positive per kind of read, and the typed uses and owners the rule leaves alone.</summary>
    internal static void VerifyEnumValueReadAnalyzer()
    {
        // Casts in both directions.
        Expect(Types + "class Dispatch { int Run(Draw draw) => (int)draw; }", "The cast '(int)draw' reads the numeric value of 'Draw'" + Advice);
        Expect(Types + "class Dispatch { ushort Run(Draw draw) => (ushort)draw; }", "The cast '(ushort)draw' reads the numeric value of 'Draw'" + Advice);
        Expect(Types + "class Dispatch { Draw Run(int word) => (Draw)word; }", "The cast '(Draw)word' reads the numeric value of 'Draw'" + Advice);
        // Enum.ToObject and Convert.
        Expect(Types + "class Dispatch { object Run(int word) => System.Enum.ToObject(typeof(Draw), word); }",
            "'System.Enum.ToObject(typeof(Draw), word)' reads the numeric value of 'Draw'" + Advice);
        Expect(Types + "class Dispatch { int Run(Draw draw) => System.Convert.ToInt32(draw); }",
            "'System.Convert.ToInt32(draw)' reads the numeric value of 'Draw'" + Advice);
        // An enum value combined with an integral literal: a comparison and arithmetic.
        Expect(Types + "class Suits { bool Run(Items items) => (items & Items.Gravity) != 0; }",
            "'(items & Items.Gravity) != 0' reads the numeric value of 'Items'" + Advice);
        Expect(Types + "class Dispatch { Draw Run(Draw draw) => draw + 1; }", "'draw + 1' reads the numeric value of 'Draw'" + Advice);

        // Typed uses read no value.
        Expect(Types + "class Suits { bool Run(Items items) => items.HasFlag(Items.Gravity) && (items & Items.Varia) == Items.Varia; }", count: 0);
        Expect(Types + "class Dispatch { bool Run(Draw draw) => draw == Draw.Wave; }", count: 0);
        // The type that owns an encoding converts, including from a nested type.
        Expect(Types + "[SnesEncodingOwner] class Codec { int Run(Draw draw) => (int)draw; }", count: 0);
        Expect(Types + "[SnesEncodingOwner] class Codec { static class Bits { static ushort Run(Items items) => (ushort)items; } }", count: 0);
        // Outside Core the import pipeline translates.
        Expect(Types + "class Importer { Draw Run(int word) => (Draw)word; }", count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>Compiles one source and compares the SME6450 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Samus/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new EnumValueReadAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != EnumValueReadAnalyzer.EnumValueReadId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {EnumValueReadAnalyzer.EnumValueReadId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
