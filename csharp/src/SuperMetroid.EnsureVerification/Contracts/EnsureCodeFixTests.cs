using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.EnsureAnalyzer.Contracts;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks Ensure guard diagnostics and the concrete code edits offered for recognized guard shapes.</summary>
internal static class EnsureCodeFixTests
{
    /// <summary>#677: SME6202 cases and the Ensure code fix for every recognized guard shape.</summary>
    internal static void VerifyEnsureCallsAndCodeFix()
    {
        // SME6202: Ensure captures the value expression; a manual name or a fitting cast defeats it.
        VerifySnippet("using SuperMetroid.Core; class C { public void M(object value) { Ensure.NotNull(value, \"value\"); } }",
            EnsureUsageAnalyzer.RedundantArgumentId, "Remove the manual parameter name");
        VerifySnippet("using SuperMetroid.Core; class C { public void M(object value) { Ensure.NotNull(value, parameterName: \"other\"); } }",
            EnsureUsageAnalyzer.RedundantArgumentId, "Ensure.NotNull captures the value expression");
        VerifySnippet("using SuperMetroid.Core; class C { public void M(long value) { Ensure.AtLeastZero((int)value); } }",
            EnsureUsageAnalyzer.RedundantArgumentId, "Do not cast the value to fit Ensure.AtLeastZero");
        VerifySnippet("using SuperMetroid.Core; class C { public void M(object value) { Ensure.NotNull(value); } }");
        VerifySnippet("using SuperMetroid.Core; class C { public void M(int value) { Ensure.BetweenInclusive(0, 7, value); } }");
        // A cast on a bound, not on the validated value, is the caller's own arithmetic.
        VerifySnippet("using SuperMetroid.Core; class C { public void M(int value, long top) { Ensure.BetweenInclusive(0, (int)top, value); } }");

        const string header = "using System; using SuperMetroid.Core; ";
        ExpectEnsureFix(header + "class Game { public void M(object value) { ArgumentNullException.ThrowIfNull(value); } }",
            "class Game { public void M(object value) { Ensure.NotNull(value); } }");
        ExpectEnsureFix(header + "class Game { public void M(int value) { ArgumentOutOfRangeException.ThrowIfNegative(value); } }",
            "class Game { public void M(int value) { Ensure.AtLeastZero(value); } }");
        ExpectEnsureFix(header + "class Game { object f = new(); public Game(object value) { f = value ?? throw new ArgumentNullException(nameof(value)); } }",
            "class Game { object f = new(); public Game(object value) { f = Ensure.NotNull(value); } }");
        ExpectEnsureFix(header + "class Game { public void M(object? value) { if (value is null) throw new ArgumentNullException(nameof(value)); } }",
            "class Game { public void M(object? value) { Ensure.NotNull(value); } }");
        ExpectEnsureFix(header + "class Game { public void M(int[] items) { if (items.Length != 4) throw new ArgumentException(\"Need four.\", nameof(items)); } }",
            "class Game { public void M(int[] items) { Ensure.LengthEqual(items, 4); } }");
        ExpectEnsureFix(header + "class Game { public void M(int value) { if (value < 0 || value > 64) throw new ArgumentOutOfRangeException(nameof(value)); } }",
            "class Game { public void M(int value) { Ensure.BetweenInclusive(0, 64, value); } }");
        ExpectEnsureFix(header + "class Game { public void M(object value) { Ensure.NotNull(value, \"value\"); } }",
            "class Game { public void M(object value) { Ensure.NotNull(value); } }", EnsureUsageAnalyzer.RedundantArgumentId);
        // A value that does not fit an operation's type is diagnostic-only: no cast is invented.
        ExpectEnsureFix(header + "class Game { public void M(long value) { ArgumentOutOfRangeException.ThrowIfNegative(value); } }", null);
        ExpectEnsureFix(header + "class Game { public void M<T>(T value) { ArgumentNullException.ThrowIfNull(value); } }", null);
        Console.WriteLine("#677: SME6202 rejects manual names and fitting casts; the Ensure fix rewrites every recognized guard shape.");
    }

    /// <summary>Applies the single Ensure fix for the first <paramref name="id"/> diagnostic in class <c>Game</c>.</summary>
    private static void ExpectEnsureFix(string source, string? expectedGame, string id = EnsureUsageAnalyzer.GuardId)
    {
        using var workspace = new AdhocWorkspace();
        Project project = workspace.AddProject("EnsureFixCase", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
            .WithMetadataReferences(References);
        Document document = project.AddDocument("Case.cs", SourceText.From(source));
        Compilation compilation = document.Project.GetCompilationAsync().GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Ensure fix fixture produced no compilation.");
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid Ensure fix fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        int game = source.IndexOf("class Game", StringComparison.Ordinal);
        Diagnostic? diagnostic = compilation.WithAnalyzers([new EnsureUsageAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .Where(d => d.Id == id && d.Location.SourceSpan.Start > game)
            .OrderBy(d => d.Location.SourceSpan.Start)
            .FirstOrDefault();
        var actions = new List<CodeAction>();
        if (diagnostic is not null)
        {
            var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
            new EnsureUsageCodeFixProvider().RegisterCodeFixesAsync(context).GetAwaiter().GetResult();
        }
        if (expectedGame is null)
        {
            if (actions.Count != 0)
                throw new InvalidOperationException($"Unexpected Ensure fix '{actions[0].Title}' for: {source}");
            return;
        }
        if (diagnostic is null || actions.Count != 1)
            throw new InvalidOperationException($"Expected one {id} fix, got {actions.Count} for: {source}");
        Document changed = actions[0].GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult()
            .OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id) ??
            throw new InvalidOperationException("The code fix removed the edited document.");
        string text = changed.GetTextAsync().GetAwaiter().GetResult().ToString();
        string actualGame = text[text.IndexOf("class Game", StringComparison.Ordinal)..];
        if (Normalize(actualGame) != Normalize(expectedGame))
            throw new InvalidOperationException($"Ensure fix produced '{actualGame}', expected '{expectedGame}'.");

        static string Normalize(string code) => string.Join(' ', code.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
