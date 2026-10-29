using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1463 whole-image-fixture rule, SME6452.</summary>
internal static class WholeImageFixtureInLoopAnalyzerTests
{
    /// <summary>A verification file.</summary>
    private const string Path = "C:/fixture-repo/csharp/src/SuperMetroid.Verification/Frontend/MapChecks.cs";

    /// <summary>The marking attribute, a marked helper, an unmarked one, and a screen whose image they build.</summary>
    private const string Helpers = """
        using System.Linq;
        [System.AttributeUsage(System.AttributeTargets.Method)]
        sealed class WholeImageFixtureAttribute(string builds) : System.Attribute { public string Builds { get; } = builds; }
        sealed class Screen { public int Area; }
        static class Images
        {
            [WholeImageFixture("the screen's 64 KiB memory image")]
            public static byte[] Compose(Screen screen) => new byte[65536];
            public static byte[] Cheap(Screen screen) => new byte[4];
        }

        """;

    /// <summary>Positives for each repeating position and negatives for each allowed shape.</summary>
    internal static void VerifyWholeImageFixtureInLoopAnalyzer()
    {
        // Rebuilt for every compared byte: inline in a for, foreach and while body, and in a loop condition.
        Expect(Helpers + "static class C { static int M(Screen s) { int sum = 0; for (int i = 0; i < 8; i++) sum += Images.Compose(s)[i]; return sum; } }",
            "'Compose' builds the screen's 64 KiB memory image on every call and is called inside a loop");
        Expect(Helpers + "static class C { static int M(Screen s, int[] offsets) { int sum = 0; foreach (int i in offsets) sum += Images.Compose(s)[i]; return sum; } }",
            "is called inside a loop");
        Expect(Helpers + "static class C { static void M(Screen s) { int i = 0; while (Images.Compose(s)[i] == 0) i++; } }",
            "is called inside a loop");
        // Rebuilt per element of a LINQ operator.
        Expect(Helpers + "static class C { static bool M(Screen s) => Enumerable.Range(0, 8).All(i => Images.Compose(s)[i] == 0); }",
            "is called per element of a LINQ operator");

        // Built once into a local per state: before the loop, or as the loop body's own local after the state changes.
        Expect(Helpers + "static class C { static int M(Screen s) { byte[] image = Images.Compose(s); int sum = 0; for (int i = 0; i < 8; i++) sum += image[i]; return sum; } }",
            count: 0);
        Expect(Helpers + "static class C { static int M(Screen s) { int sum = 0; for (int area = 0; area < 6; area++) { s.Area = area; byte[] image = Images.Compose(s); sum += image[0]; } return sum; } }",
            count: 0);
        // Once per call of the member, an unmarked helper in a loop, and a foreach over a built image.
        Expect(Helpers + "static class C { static int M(Screen s) => Images.Compose(s)[0]; }", count: 0);
        Expect(Helpers + "static class C { static int M(Screen s) { int sum = 0; for (int i = 0; i < 4; i++) sum += Images.Cheap(s)[i]; return sum; } }",
            count: 0);
        Expect(Helpers + "static class C { static int M(Screen s) { int sum = 0; foreach (byte b in Images.Compose(s)) sum += b; return sum; } }",
            count: 0);
        Console.WriteLine("Whole-image fixtures: rebuilt per loop iteration or LINQ element is SME6452; built once into a local per state is allowed.");
    }

    /// <summary>Runs the analyzer over <paramref name="source"/> and requires <paramref name="count"/> SME6452 diagnostics containing <paramref name="message"/>.</summary>
    private static void Expect(string source, string? message = null, int count = 1)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: Path);
        var compilation = CSharpCompilation.Create("SuperMetroid.Verification", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new WholeImageFixtureInLoopAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != WholeImageFixtureInLoopAnalyzer.FixtureInLoopId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.Any(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {WholeImageFixtureInLoopAnalyzer.FixtureInLoopId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
