using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2553 impossible-guard rule, SME6481.</summary>
internal static class ImpossibleGuardAnalyzerTests
{
    /// <summary>The types the fixtures use: an identity, an encoding owner's marker and a closed and an open hierarchy.</summary>
    private const string Types = """
        using System;
        using System.Collections.Generic;
        using System.Collections.Frozen;
        enum Side { Left, Right }
        sealed class EncodingOwnerAttribute : Attribute { }
        abstract record Shown { private Shown() { } internal sealed record Frame(int Picture) : Shown; internal sealed record Ended : Shown; }
        abstract record Settings { internal sealed record Walker(int Speed) : Settings; }
        static class Ensure { internal static V Listed<K, V>(IReadOnlyDictionary<K, V> table, K key) where K : notnull => table[key]; }

        """;

    /// <summary>One positive per guard the rule reads, and the checks it leaves alone.</summary>
    internal static void VerifyImpossibleGuardAnalyzer()
    {
        // Enum.IsDefined on a Core value, generic and typed.
        Expect(Types + "class Hud { Side Read(Side side) => Enum.IsDefined(side) ? side : throw new ArgumentException(); }",
            "tests whether a Side is a member");
        Expect(Types + "class Hud { bool Read(Side side) => Enum.IsDefined(typeof(Side), side); }", "tests whether a Side is a member");
        // An encoding owner decodes numbers into members, so its test can fail.
        Expect(Types + "[EncodingOwner] class Codec { bool Read(Side side) => Enum.IsDefined(side); }", count: 0);

        // A lookup that can miss on a catalog naming every member: TryGetValue, ContainsKey and Ensure.Listed, through a
        // field, a property and a frozen copy.
        const string Total = "static readonly Dictionary<Side, int> Offsets = new() { [Side.Left] = -1, [Side.Right] = 1 };";
        Expect(Types + "class Walk { " + Total + " int Read(Side side) => Offsets.TryGetValue(side, out int offset) ? offset : throw new ArgumentException(); }",
            "asks whether 'Offsets' holds a Side, but it names every member");
        Expect(Types + "class Walk { " + Total + " bool Read(Side side) => Offsets.ContainsKey(side); }", "names every member");
        Expect(Types + "class Walk { " + Total + " int Read(Side side) => Ensure.Listed(Offsets, side); }", "names every member");
        Expect(Types + "class Walk { static IReadOnlyDictionary<Side, int> Offsets { get; } = new Dictionary<Side, int> { [Side.Left] = -1, [Side.Right] = 1 }.ToFrozenDictionary(); bool Read(Side side) => Offsets.ContainsKey(side); }",
            "names every member");
        Expect(Types + "class Walk { static readonly Dictionary<Side, int> Offsets = new() { { Side.Left, -1 }, { Side.Right, 1 } }; bool Read(Side side) => Offsets.ContainsKey(side); }",
            "names every member");
        // A catalog that leaves a member out can miss, and a parameter's table is not a catalog.
        Expect(Types + "class Walk { static readonly Dictionary<Side, int> Offsets = new() { [Side.Left] = -1 }; bool Read(Side side) => Offsets.ContainsKey(side); }", count: 0);
        Expect(Types + "class Walk { bool Read(IReadOnlyDictionary<Side, int> offsets, Side side) => offsets.ContainsKey(side); }", count: 0);

        // A downcast that throws on a closed hierarchy, by conditional, by a guard and by a try cast.
        Expect(Types + "class Play { int Read(Shown shown) => shown is Shown.Frame frame ? frame.Picture : throw new InvalidOperationException(); }",
            "throws unless the Shown is a Frame");
        Expect(Types + "class Play { int Read(Shown shown) { if (shown is not Shown.Frame frame) throw new InvalidOperationException(); return frame.Picture; } }",
            "throws unless the Shown is a Frame");
        Expect(Types + "class Play { Shown.Frame Read(Shown shown) => shown as Shown.Frame ?? throw new InvalidOperationException(); }",
            "throws unless the Shown is a Frame");
        // An open hierarchy can hold any case, and a downcast that does not throw is a choice.
        Expect(Types + "class Spawn { Settings.Walker Read(Settings settings) => settings as Settings.Walker ?? throw new InvalidOperationException(); }", count: 0);
        Expect(Types + "class Play { int Read(Shown shown) => shown is Shown.Frame frame ? frame.Picture : 0; }", count: 0);

        // Outside Core the rule does not apply.
        Expect(Types + "class Importer { bool Read(Side side) => Enum.IsDefined(side); }", count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>Compiles one source and compares the SME6481 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Frontend/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new ImpossibleGuardAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != ImpossibleGuardAnalyzer.ImpossibleGuardId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {ImpossibleGuardAnalyzer.ImpossibleGuardId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
