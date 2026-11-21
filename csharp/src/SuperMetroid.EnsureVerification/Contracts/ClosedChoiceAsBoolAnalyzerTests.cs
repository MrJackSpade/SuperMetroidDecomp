using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2560 closed-choice rule, SME6485.</summary>
internal static class ClosedChoiceAsBoolAnalyzerTests
{
    /// <summary>The enums the fixtures choose between.</summary>
    private const string Types = """
        using System;
        enum Side { Left, Right }
        enum Stage { First, Second, Third }
        [Flags] enum Marks { None = 0, Lit = 1 }

        """;

    /// <summary>The advice every SME6485 message ends with.</summary>
    private const string Advice = "hold and pass the Side value itself";

    /// <summary>One positive per way a bool carries a two-member choice, and the uses the rule leaves alone.</summary>
    internal static void VerifyClosedChoiceAsBoolAnalyzer()
    {
        // A conditional on a bool parameter, field or property, negated or not, choosing between the two members.
        Expect(Types + "class C { Side Of(bool left) => left ? Side.Left : Side.Right; }", "'left ? Side.Left : Side.Right'");
        Expect(Types + "class C { bool _left; Side Of() => _left ? Side.Left : Side.Right; }", Advice);
        Expect(Types + "class C { bool Left { get; set; } Side Of() => !Left ? Side.Right : Side.Left; }", Advice);
        Expect(Types + "record Spec(bool Left) { Side Of() => Left ? Side.Left : Side.Right; }", Advice);
        // A comparison with a member stored into a bool field or property, initialized into one, or passed to a bool parameter.
        Expect(Types + "class C { bool _left; void Set(Side side) => _left = side == Side.Left; }", "'side == Side.Left'");
        Expect(Types + "class C { bool Left { get; set; } void Set(Side side) => Left = side != Side.Right; }", Advice);
        Expect(Types + "class C { public bool Left { get; init; } static C Of(Side side) => new() { Left = side == Side.Left }; }", Advice);
        Expect(Types + "class C { static Side Start => Side.Left; readonly bool _left = Start == Side.Left; }", Advice);
        Expect(Types + "class C { static void Take(bool left) { } void Pass(Side side) => Take(side == Side.Left); }", Advice);
        // The import pipeline is covered too.
        Expect(Types + "class Importer { Side Of(bool left) => left ? Side.Left : Side.Right; }", Advice, assemblyName: "SuperMetroid.AssetExtraction");
        // A wider enum, composable flags, one member on both arms, or arms that are not members are not a two-member choice.
        Expect(Types + "class C { Stage Of(bool first) => first ? Stage.First : Stage.Second; }", count: 0);
        Expect(Types + "class C { Marks Of(bool lit) => lit ? Marks.Lit : Marks.None; }", count: 0);
        Expect(Types + "class C { Side Of(bool left) => left ? Side.Left : Side.Left; }", count: 0);
        Expect(Types + "class C { Side Of(bool left, Side other) => left ? Side.Left : other; }", count: 0);
        // A condition that is no stored bool, and a comparison that only answers a question, are not a stored choice.
        Expect(Types + "class C { static bool Lefty(int x) => x < 0; Side Of(int x) => Lefty(x) ? Side.Left : Side.Right; }", count: 0);
        Expect(Types + "class C { Side Of(int x) { bool left = x < 0; return left ? Side.Left : Side.Right; } }", count: 0);
        Expect(Types + "class C { int _speed; bool IsNegative => _speed < 0; Side Heading() => IsNegative ? Side.Left : Side.Right; }", count: 0);
        Expect(Types + "class C { Side _side; bool IsLeft => _side == Side.Left; }", count: 0);
        Expect(Types + "class C { int Of(Side side) { bool left = side == Side.Left; return left ? 1 : 2; } }", count: 0);
        Expect(Types + "class C { bool _wide; void Set(Stage stage) => _wide = stage == Stage.Third; }", count: 0);
        // Outside Core and the import pipeline the rule does not apply.
        Expect(Types + "class Host { Side Of(bool left) => left ? Side.Left : Side.Right; }", count: 0, assemblyName: "SuperMetroid.Diagnostics");
    }

    /// <summary>Compiles one source and compares the SME6485 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Samus/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new ClosedChoiceAsBoolAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != ClosedChoiceAsBoolAnalyzer.ClosedChoiceAsBoolId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {ClosedChoiceAsBoolAnalyzer.ClosedChoiceAsBoolId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
