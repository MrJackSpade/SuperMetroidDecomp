using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1452 catalog-data rule, SME6451.</summary>
internal static class CatalogDecisionAnalyzerTests
{
    /// <summary>A catalog file by the encoding scanner's suffix list.</summary>
    private const string Catalog = "C:/fixture-repo/csharp/src/SuperMetroid.Core/Rooms/DoorDefinitions.cs";

    /// <summary>A functional Core file.</summary>
    private const string Functional = "C:/fixture-repo/csharp/src/SuperMetroid.Core/Rooms/DoorScroll.cs";

    /// <summary>The advice every SME6451 message ends with.</summary>
    private const string Advice = "; a catalog holds data only: move the decision to the domain type that owns the behaviour, or to the import pipeline when it decodes cartridge bytes";

    /// <summary>The domain types the fixtures construct.</summary>
    private const string Types = """
        enum Side { Left, Right }
        sealed record Door(Side Side, int Frames);

        """;

    /// <summary>One positive per kind of decision, and the data shapes and functional code the rule leaves alone.</summary>
    internal static void VerifyCatalogDecisionAnalyzer()
    {
        // A switch, statement or expression.
        Expect(Types + "static class DoorDefinitions { static int Frames(Side side) => side switch { Side.Left => 4, _ => 8 }; }",
            "The catalog 'DoorDefinitions' holds a 'switch' expression" + Advice);
        Expect(Types + "static class DoorDefinitions { static int Frames(Side side) { switch (side) { case Side.Left: return 4; default: return 8; } } }",
            "The catalog 'DoorDefinitions' holds a 'switch' statement" + Advice, count: 2);
        // An if, here a guard with its comparison; an if needs a block, which is a body with statements.
        Expect(Types + "static class DoorDefinitions { static int Frames(int frame) { if (frame < 0) throw new System.ArgumentOutOfRangeException(nameof(frame)); return frame; } }",
            "The catalog 'DoorDefinitions' holds a comparison '<'" + Advice, count: 3);
        Expect(Types + "static class DoorDefinitions { static int Frames(bool closed) { if (closed) return 4; return 8; } }",
            "The catalog 'DoorDefinitions' holds an 'if'" + Advice, count: 2);
        // A conditional, a loop and a LINQ predicate.
        Expect(Types + "static class DoorDefinitions { static int Frames(bool closed) => closed ? 4 : 8; }",
            "The catalog 'DoorDefinitions' holds a conditional '?:'" + Advice);
        Expect(Types + "static class DoorDefinitions { static System.Collections.Generic.IEnumerable<int> Frames() { for (int frame = 0; frame < 4; frame++) yield return frame; } }",
            "The catalog 'DoorDefinitions' holds a loop" + Advice, count: 3);
        Expect(Types + "static class DoorDefinitions { static readonly Door[] All = [new(Side.Left, 4)]; static Door Left => System.Linq.Enumerable.First(All, door => door.Side == Side.Left); }",
            "The catalog 'DoorDefinitions' holds a predicate passed to 'First'" + Advice, count: 2);
        // A constructor that assigns state runs a procedure.
        Expect(Types + "sealed class DoorDefinitions { private readonly int frames; DoorDefinitions(int frames) { this.frames = frames; } }",
            "The catalog 'DoorDefinitions' holds a body with statements" + Advice);

        // Pure data: literals, record construction, collection expressions, dictionary initializers, nameof and
        // constant arithmetic, and a member that only returns a constructed value.
        Expect(Types + """
            static class DoorDefinitions
            {
                internal const int Frames = 4 * 2;
                internal const bool Wide = Frames > 4;
                internal static readonly string Name = nameof(Door);
                internal static readonly Door Left = new(Side.Left, Frames);
                internal static readonly Door[] All = [Left, new(Side.Right, 1 << 3)];
                internal static readonly System.Collections.Generic.Dictionary<Side, Door> BySide = new() { [Side.Left] = Left };
                internal static Door Right(int frames) => new(Side.Right, frames);
                internal static Door Closed { get { return new(Side.Left, 0); } }
            }
            """, count: 0);
        // Functional code decides; the rule is about catalogs.
        Expect(Types + "static class DoorScroll { static int Frames(Side side) => side switch { Side.Left => 4, _ => 8 }; }",
            count: 0, path: Functional);
        // Outside Core the import pipeline decodes.
        Expect(Types + "static class DoorDefinitions { static int Frames(Side side) => side switch { Side.Left => 4, _ => 8 }; }",
            count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>
    /// Compiles one source and compares the SME6451 count; every diagnostic names the catalog's type, and one of them
    /// carries <paramref name="message"/> when it is given.
    /// </summary>
    private static void Expect(string source, string? message = null, int count = 1, string path = Catalog,
        string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new CatalogDecisionAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != CatalogDecisionAnalyzer.CatalogDecisionId || d.Severity != DiagnosticSeverity.Error) ||
            !diagnostics.All(d => d.GetMessage().StartsWith("The catalog '", StringComparison.Ordinal)) ||
            (message is not null && !diagnostics.Any(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {CatalogDecisionAnalyzer.CatalogDecisionId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
