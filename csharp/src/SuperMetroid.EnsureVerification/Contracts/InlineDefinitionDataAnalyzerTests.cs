using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2575 inline definition data rule, SME6472.</summary>
internal static class InlineDefinitionDataAnalyzerTests
{
    /// <summary>A behaviour file of Core.</summary>
    private const string Behaviour = "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Walker.cs";

    /// <summary>A catalog file by the encoding scanner's suffix list.</summary>
    private const string Catalog = "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/WalkerDefinitions.cs";

    /// <summary>The advice every SME6472 message ends with.</summary>
    private const string Advice = "; definition data lives in a domain-named catalog member whose summary gives its meaning, and behaviour code reads it by name";

    /// <summary>The Core value types, the enum and the spawner the fixtures use.</summary>
    private const string Types = """
        namespace SuperMetroid.Core.Enemies
        {
            enum Strength { Light, Strong }
            readonly record struct Shake(Strength Strength);
            readonly record struct Offset(int X, int Y);
            readonly record struct Speed(int Raw)
            {
                internal static Speed FromWholePixels(int pixels) => new(pixels * 65536);
                internal static Speed OnePixel => FromWholePixels(1);
            }
            sealed class Spawner
            {
                internal void Spawn(int count, Offset at) { }
            }
        }

        """;

    /// <summary>One positive per shape of literal definition data, and the code and data the rule leaves alone.</summary>
    internal static void VerifyInlineDefinitionDataAnalyzer()
    {
        // A value built from literals in a method, by a static factory and by a constructor.
        Expect(Type("sealed class Walker { Speed Probe() => Speed.FromWholePixels(1); }"),
            "'Walker' builds a 'Speed' from the literal 1" + Advice);
        Expect(Type("sealed class Walker { Offset Step() => new Offset(1, 0); }"),
            "'Walker' builds a 'Offset' from the literal 1" + Advice);
        Expect(Type("sealed class Walker { Offset Step() => new(-6, 2); }"), "from the literals -6, 2");
        // A value built from literals as static data of a type that holds state.
        Expect(Type("sealed class Walker { static readonly Offset[] Bursts = [new(6, 2), new(-4, 5)]; int frame; }"), count: 2);
        // An instance member's initializer.
        Expect(Type("sealed class Walker { Speed speed = Speed.FromWholePixels(4); }"), "from the literal 4");
        // A literal passed by name, to another type's member and to the type's own.
        Expect(Type("sealed class Walker { void Die(Spawner spawner) => spawner.Spawn(count: 5, new Offset()); }"),
            "'Walker' passes the literal 5 as 'count'" + Advice);
        Expect(Type("sealed class Walker { void Go() => Move(steps: 2); void Move(int steps) { } }"), "passes the literal 2 as 'steps'");
        // A static type that decides holds behaviour.
        Expect(Type("static class WalkerSteps { static Offset Of(bool up) => up ? new(0, -1) : new(0, 1); }"), count: 2);

        // A catalog holds definition data.
        Expect(Type("static class WalkerDefinitions { static Speed Probe => Speed.FromWholePixels(1); }"), count: 0, path: Catalog);
        // A static type whose members decide nothing is a table of data.
        Expect(Type("static class WalkerAnimations { static readonly Offset[] Steps = [new(1, 2)]; static Offset[] Turn => [new(-1, 2)]; }"), count: 0);
        // Zero, the absence of a quantity, by position or by name.
        Expect(Type("sealed class Walker { Offset Rest() => new(0, 0); void Go() => Move(steps: 0); void Move(int steps) { } }"), count: 0);
        // A literal beside computed state is arithmetic on it.
        Expect(Type("sealed class Walker { Offset Below(int x) => new(x, 1); }"), count: 0);
        // A value type naming its own values, a named constant, an enum-only value and a literal passed by position.
        Expect(Type("sealed class Walker { const int Count = 5; void Die(Spawner spawner) => spawner.Spawn(count: Count, new Offset()); Shake Quake() => new(Strength.Light); Speed Probe() => Speed.OnePixel; }"),
            count: 0);
        Expect(Type("sealed class Walker { void Die(Spawner spawner) => spawner.Spawn(5, new Offset()); }"), count: 0);
        // A member declared outside Core.
        Expect(Type("sealed class Walker { double Rounded(double value) => System.Math.Round(value, digits: 1); }"), count: 0);
        // Outside Core the import pipeline decodes.
        Expect(Type("sealed class Walker { Speed Probe() => Speed.FromWholePixels(1); }"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>The fixture types and <paramref name="declaration"/>, declared in Core's enemies namespace.</summary>
    private static string Type(string declaration) => Types + $$"""
        namespace SuperMetroid.Core.Enemies
        {
            {{declaration}}
        }
        """;

    /// <summary>
    /// Compiles one source and compares the SME6472 count; every diagnostic carries <paramref name="message"/> when it is
    /// given.
    /// </summary>
    private static void Expect(string source, string? message = null, int count = 1, string path = Behaviour,
        string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InlineDefinitionDataAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != InlineDefinitionDataAnalyzer.InlineDefinitionDataId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {InlineDefinitionDataAnalyzer.InlineDefinitionDataId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
