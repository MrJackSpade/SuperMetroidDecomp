using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Hardware;
using SuperMetroid.EnsureVerification.CartridgeEncoding;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Hardware;

/// <summary>Checks the CPU-bus declaration ratchet (#1320, SME6292-SME6293) against small C# source fixtures.</summary>
internal static class CpuBusDeclarationAnalyzerTests
{
    /// <summary>Stable temporary root used to give fixture sources repository-relative paths.</summary>
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "cpu-bus-declaration-case");

    /// <summary>The fixture file every case compiles, as the baseline names it.</summary>
    private const string Game = "csharp/src/SuperMetroid.Core/Game/Walker.cs";

    /// <summary>A baseline with no entries: every declaration outside a boundary is reported in place.</summary>
    private const string Empty = "# path<TAB>rule<TAB>count\n";

    /// <summary>Marker attributes and bus contracts shaped like Core's.</summary>
    private const string Api =
        "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusAccessAttribute : System.Attribute { } " +
        "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusBoundaryAttribute(string reason) : System.Attribute { } " +
        "[CpuBusAccess] interface IBus { byte ReadByte(int address); } " +
        "[CpuBusAccess] sealed class RawBuffer { } " +
        "interface IOwnedState { } ";

    /// <summary>Positive and negative cases for each diagnostic.</summary>
    internal static void VerifyCpuBusDeclarationAnalyzer()
    {
        // Without the baseline additional file the rule is inactive: tools and tests may take a bus.
        Expect(null, Api + "class Game { void M(IBus bus) { } }");

        // SME6292: parameters, fields, properties, record and primary-constructor parameters typed as a bus interface.
        Expect(Empty, Api + "class Game { void M(IBus? bus) { } }",
            (CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Empty, Api + "class Game { private readonly IBus bus = null!; byte M() => 0; }",
            (CpuBusDeclarationAnalyzer.DeclarationId, "field 'bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Empty, Api + "class Game { public IBus Bus { get; } = null!; }",
            (CpuBusDeclarationAnalyzer.DeclarationId, "property 'Bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Empty, Api + "record Room(IBus Bus, int Area);",
            (CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'Bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Empty, Api + "class View(IBus bus) { }",
            (CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus' is typed as CPU-bus interface 'IBus'"));

        // Boundary types and members, a raw API type's own implementation, owned state and concrete raw buffers pass.
        Expect(Empty, Api + "[CpuBusBoundary(\"domain view\")] sealed class SamusMemory(IBus bus) { public byte Health => bus.ReadByte(0); }");
        Expect(Empty, Api + "class Codec { [CpuBusBoundary(\"import\")] static byte Import(IBus bus) => bus.ReadByte(0); }");
        Expect(Empty, Api + "class Codec { [CpuBusBoundary(\"view\")] IBus Source { get => null!; } }");
        Expect(Empty, Api + "[CpuBusAccess] static class WorkRam { public static byte Read(IBus bus) => bus.ReadByte(0); }");
        Expect(Empty, Api + "class Game { void M(IOwnedState state, RawBuffer buffer) { } }");

        // The reviewed count may only fall: equal passes, more reports once, fewer reports the stale entry.
        string two = $"{Game}\tSME6292\t2\n";
        Expect(two, Api + "class Game { void M(IBus bus) { } void N(IBus bus) { } }");
        Expect(two, Api + "class Game { void M(IBus bus) { } void N(IBus bus) { } void O(IBus bus) { } }",
            (CpuBusDeclarationAnalyzer.DeclarationId, "holds 3 CPU-bus declarations but its reviewed baseline allows 2"));
        Expect(two, Api + "class Game { void M(IBus bus) { } }",
            (CpuBusDeclarationAnalyzer.StaleCountId, "now holds 1 CPU-bus declaration(s) but the baseline allows 2"));
    }

    /// <summary>
    /// Runs <see cref="CpuBusDeclarationAnalyzer"/> over <paramref name="source"/> at <see cref="Game"/> with an
    /// optional baseline (null omits the additional file) and requires exactly the expected diagnostics, in order.
    /// </summary>
    private static void Expect(string? baseline, string source, params (string Id, string Message)[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            Path.Combine(Root, Game.Replace('/', Path.DirectorySeparatorChar)));
        var compilation = CSharpCompilation.Create("SuperMetroid.Core", [tree], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        ImmutableArray<AdditionalText> additional = baseline is null ? []
            : [new CartridgeEncodingAnalyzerTests.MemoryAdditionalText(Path.Combine(Root, "csharp", "cpu-bus-declaration-baseline.txt"), baseline)];
        Diagnostic[] diagnostics = compilation
            .WithAnalyzers([new CpuBusDeclarationAnalyzer()], new AnalyzerOptions(additional))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start)
            .ToArray();
        string Describe() => $"[{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}";
        if (diagnostics.Length != expected.Length)
            throw new InvalidOperationException($"Expected {expected.Length} CPU-bus declaration diagnostic(s), got {Describe()}");
        for (int index = 0; index < expected.Length; index++)
        {
            if (diagnostics[index].Id != expected[index].Id ||
                !diagnostics[index].GetMessage().Contains(expected[index].Message, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Expected {expected[index].Id} containing '{expected[index].Message}', got {Describe()}");
            }
        }
    }
}
