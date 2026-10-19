using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6354, which keeps underscore-prefixed members private (#1328).</summary>
internal static class UnderscoreMemberAnalyzerTests
{
    /// <summary>Non-private underscore fields and properties are reported; private ones and ordinary names are not.</summary>
    internal static void VerifyUnderscoreMemberAnalyzer()
    {
        Expect("class C { internal int _count; }", "'_count' is internal");
        Expect("class C { public int _count { get; set; } }", "'_count' is public");
        Expect("class C { protected internal int _count; }", "'_count' is protectedorinternal");
        Expect("class C { private int _count; }");
        Expect("class C { private protected int _count; }");
        Expect("class C { int _count; }");
        Expect("class C { internal int Count; }");
    }

    /// <summary>Compiles one file and checks that it reports exactly one SME6354 containing <paramref name="message"/>, or none.</summary>
    private static void Expect(string source, string? message = null)
    {
        var compilation = CSharpCompilation.Create("UnderscoreFixture", [CSharpSyntaxTree.ParseText(source)], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid underscore fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new UnderscoreMemberAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        bool matches = message is null
            ? diagnostics.Length == 0
            : diagnostics.Length == 1 && diagnostics[0].Id == UnderscoreMemberAnalyzer.UnderscoreMemberId &&
              diagnostics[0].GetMessage().Contains(message, StringComparison.Ordinal);
        if (!matches)
        {
            throw new InvalidOperationException($"Expected {(message is null ? "no diagnostic" : $"SME6354 containing '{message}'")} for {source}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
