using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer;
using SuperMetroid.EnsureAnalyzer.CodeFixes;

namespace SuperMetroid.EnsureVerification;

/// <summary>Checks the #624 adapter-chain rules and their code fix.</summary>
internal static class AdapterChainAnalyzerTests
{
    /// <summary>Usings every fixture shares.</summary>
    private const string Header = "using System; using System.Linq; using System.Collections.Generic; using SuperMetroid.Core; ";

    /// <summary>Positive and negative cases for SME6390/SME6391 and the unambiguous fixes.</summary>
    internal static void VerifyAdapterChainAnalyzer()
    {
        // SME6390: a LINQ copy that only feeds another LINQ operator.
        Expect("class C { bool M(int[] a) => a.ToArray().Any(x => x > 1); }", AdapterChainAnalyzer.MaterializationId, "call it on the source");
        Expect("class C { int M(List<int> a) => a.ToArray().Where(x => x > 1).Count(); }", AdapterChainAnalyzer.MaterializationId);
        Expect("class C { bool M(IEnumerable<int> a, int[] b) => a.ToArray().SequenceEqual(b.Select(x => x)); }", AdapterChainAnalyzer.MaterializationId);
        Expect("class C { int[] M(IEnumerable<int> a) => a.Select(x => x).ToArray().Select(x => x + 1).ToArray(); }", AdapterChainAnalyzer.MaterializationId);
        // A span or memory copied only for LINQ where a span query reads it in place.
        Expect("class C { bool M(ReadOnlySpan<int> s) => s.ToArray().Any(x => x > 1); }", AdapterChainAnalyzer.MaterializationId, "span query 'Any'");
        Expect("class C { int[] M(Span<int> s) => s.ToArray().Select(x => x + 1).ToArray(); }", AdapterChainAnalyzer.MaterializationId, "'SelectToArray'");
        Expect("class C { int M(ReadOnlyMemory<int> m) => m.ToArray().Sum(x => x); }", AdapterChainAnalyzer.MaterializationId, "'.Span.Sum'");
        // SME6391: AsSpan() only to reach an array comparison.
        Expect("class C { bool M(byte[] a, byte[] b) => a.AsSpan().SequenceEqual(b); }", AdapterChainAnalyzer.SpanAdapterId);

        // Retained conversions with a purpose: ownership copies, slicing, interop, deferred results, comparers.
        Expect("class C { int[] M(List<int> a) { int[] copy = a.ToArray(); return copy; } }");
        Expect("class C { bool M(byte[] a, byte[] b) => a.AsSpan(1).SequenceEqual(b); }");
        Expect("class C { void Use(int[] x) { } void M(ReadOnlySpan<int> s) => Use(s.ToArray()); }");
        Expect("class C { IEnumerable<int> M(IEnumerable<int> a) { var copy = a.ToArray(); return copy.Where(x => x > 1); } }");
        Expect("class C { bool M(int[] a, int[] b, IEqualityComparer<int> c) => a.AsSpan().SequenceEqual(b, c); }");
        Expect("class C { int[] M(ReadOnlySpan<int> s) => s.ToArray().Reverse().ToArray(); }");
        Expect("class C { bool M(System.IO.MemoryStream a, byte[] b) => a.ToArray().SequenceEqual(b); }");
        Expect("class C { bool M(ReadOnlySpan<int> s) => s.Any(x => x > 1); }");
        Expect("class C { bool M(byte[] a, byte[] b) => a.SequenceEqual(b); }");

        // The unambiguous fixes remove the adapter; a span bridge's query rename is left to the author.
        ExpectFix("class C { bool M(int[] a) => a.ToArray().Any(x => x > 1); }", "class C { bool M(int[] a) => a.Any(x => x > 1); }");
        ExpectFix("class C { bool M(byte[] a, byte[] b) => a.AsSpan().SequenceEqual(b); }", "class C { bool M(byte[] a, byte[] b) => a.SequenceEqual(b); }");
        ExpectFix("class C { bool M(ReadOnlySpan<int> s) => s.ToArray().Any(x => x > 1); }", null);
        Console.WriteLine("#624 adapter chains: SME6390/SME6391 report copies and adapters made only to reach an operator; ownership copies, slicing, interop and comparer calls stay.");
    }

    /// <summary>Compiles one fixture and compares its adapter diagnostics.</summary>
    private static void Expect(string body, string? id = null, string? message = null)
    {
        string source = Header + body;
        var compilation = Compile(source);
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new AdapterChainAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (id is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected adapter diagnostic {diagnostics[0]} in: {body}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != id || diagnostics[0].Severity != DiagnosticSeverity.Error ||
            (message is not null && !diagnostics[0].GetMessage().Contains(message, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Expected one {id} containing '{message}', got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {body}");
        }
    }

    /// <summary>Applies the single offered fix, or checks that none is offered.</summary>
    private static void ExpectFix(string body, string? expectedBody)
    {
        using var workspace = new AdhocWorkspace();
        Project project = workspace.AddProject("AdapterFixCase", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
            .WithMetadataReferences(AnalyzerTests.References);
        Document document = project.AddDocument("Case.cs", SourceText.From(Header + body));
        Compilation compilation = document.Project.GetCompilationAsync().GetAwaiter().GetResult() ??
            throw new InvalidOperationException("Adapter fix fixture produced no compilation.");
        Diagnostic diagnostic = compilation.WithAnalyzers([new AdapterChainAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().Single();
        var actions = new List<CodeAction>();
        new AdapterChainCodeFixProvider().RegisterCodeFixesAsync(
            new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None)).GetAwaiter().GetResult();
        if (expectedBody is null)
        {
            if (actions.Count != 0)
                throw new InvalidOperationException($"Unexpected adapter fix '{actions[0].Title}' for: {body}");
            return;
        }
        if (actions.Count != 1)
            throw new InvalidOperationException($"Expected one adapter fix, got {actions.Count} for: {body}");
        Document changed = actions[0].GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult()
            .OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id) ??
            throw new InvalidOperationException("The adapter fix removed the edited document.");
        string text = changed.GetTextAsync().GetAwaiter().GetResult().ToString()[Header.Length..];
        if (text != expectedBody)
            throw new InvalidOperationException($"Adapter fix produced '{text}', expected '{expectedBody}'.");
    }

    /// <summary>A nullable-enabled compilation of <paramref name="source"/> that must compile without errors.</summary>
    private static CSharpCompilation Compile(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: "Case.cs");
        var compilation = CSharpCompilation.Create("AdapterChainCase", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid adapter fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        return compilation;
    }
}
