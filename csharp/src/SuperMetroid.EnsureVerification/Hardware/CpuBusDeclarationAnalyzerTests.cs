using SuperMetroid.EnsureVerification.CartridgeEncoding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Hardware;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Hardware;

/// <summary>Checks the CPU-bus declaration rule (#1320, SME6292) against small C# source fixtures.</summary>
internal static class CpuBusDeclarationAnalyzerTests
{
    /// <summary>Stable temporary root used to give fixture sources repository-relative paths.</summary>
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "cpu-bus-declaration-case");

    /// <summary>The fixture file every case compiles.</summary>
    private const string Game = "csharp/src/SuperMetroid.Core/Game/Walker.cs";

    /// <summary>Marker attributes and bus contracts shaped like Core's.</summary>
    private const string Api =
        "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusAccessAttribute : System.Attribute { } " +
        "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusBoundaryAttribute(string reason) : System.Attribute { } " +
        "[CpuBusAccess] interface IBus { byte ReadByte(int address); } " +
        "[CpuBusAccess] sealed class RawBuffer { } " +
        "interface IOwnedState { } ";

    /// <summary>Positive and negative cases for the diagnostic and its Core scope.</summary>
    internal static void VerifyCpuBusDeclarationAnalyzer()
    {
        // Core owns gameplay; tools and tests outside it may take a bus.
        Expect("SuperMetroid.Verification", Api + "class Game { void M(IBus bus) { } }");

        // SME6292: parameters, fields, properties, record and primary-constructor parameters typed as a bus interface.
        Expect(Api + "class Game { void M(IBus? bus) { } }",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Api + "class Game { private readonly IBus bus = null!; byte M() => 0; }",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "field 'bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Api + "class Game { public IBus Bus { get; } = null!; }",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "property 'Bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Api + "record Room(IBus Bus, int Area);",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'Bus' is typed as CPU-bus interface 'IBus'"));
        Expect(Api + "class View(IBus bus) { }",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus' is typed as CPU-bus interface 'IBus'"));
        // Every declaration is reported where it is.
        Expect(Api + "class Game { void M(IBus bus) { } void N(IBus bus) { } }",
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus'"),
            new ExpectedDiagnostic(CpuBusDeclarationAnalyzer.DeclarationId, "parameter 'bus'"));

        // Boundary types and members, a raw API type's own implementation, owned state and concrete raw buffers pass.
        Expect(Api + "[CpuBusBoundary(\"domain view\")] sealed class SamusMemory(IBus bus) { public byte Health => bus.ReadByte(0); }");
        Expect(Api + "class Codec { [CpuBusBoundary(\"import\")] static byte Import(IBus bus) => bus.ReadByte(0); }");
        Expect(Api + "class Codec { [CpuBusBoundary(\"view\")] IBus Source { get => null!; } }");
        Expect(Api + "[CpuBusAccess] static class WorkRam { public static byte Read(IBus bus) => bus.ReadByte(0); }");
        Expect(Api + "class Game { void M(IOwnedState state, RawBuffer buffer) { } }");
    }

    /// <summary>Runs the analyzer over <paramref name="source"/> compiled as Core; see the assembly overload.</summary>
    private static void Expect(string source, params ExpectedDiagnostic[] expected) =>
        Expect("SuperMetroid.Core", source, expected);

    /// <summary>
    /// Runs <see cref="CpuBusDeclarationAnalyzer"/> over <paramref name="source"/> at <see cref="Game"/>, compiled as
    /// <paramref name="assemblyName"/>, and requires exactly the expected diagnostics, in order.
    /// </summary>
    private static void Expect(string assemblyName, string source, params ExpectedDiagnostic[] expected)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            Path.Combine(Root, Game.Replace('/', Path.DirectorySeparatorChar)));
        var compilation = CSharpCompilation.Create(assemblyName, [tree], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        Diagnostic[] diagnostics = compilation
            .WithAnalyzers([new CpuBusDeclarationAnalyzer()])
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
