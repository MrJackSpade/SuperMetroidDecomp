using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2566 unread-result rule, SME6474.</summary>
internal static class UnreadResultAnalyzerTests
{
    /// <summary>The advice a discarded out result ends with.</summary>
    private const string OutAdvice = "discards an out result";

    /// <summary>The advice an out result no caller reads ends with.</summary>
    private const string UnreadOutAdvice = "has an out result no caller reads";

    /// <summary>The advice a result no caller reads ends with.</summary>
    private const string UnreadAdvice = "returns a value no caller reads";

    /// <summary>The advice a discarded parameter ends with.</summary>
    private const string ParameterAdvice = "takes a parameter it discards";

    /// <summary>The advice a method returning only null ends with.</summary>
    private const string NullAdvice = "returns null on every path";

    /// <summary>The Core guard class, whose methods return the value they checked.</summary>
    private const string Declarations = """
        namespace SuperMetroid.Core
        {
            public static class Ensure
            {
                public static T NotNull<T>(T value) where T : class => value ?? throw new System.ArgumentNullException(nameof(value));
            }
        }
        """;

    /// <summary>One positive per unread form, and the uses the rule leaves alone.</summary>
    internal static void VerifyUnreadResultAnalyzer()
    {
        // A result no caller reads, discarded by name and through a conditional access.
        Expect(Members("""
            private static int Look(int value) => value + 1;
            internal static void Run() { _ = Look(1); }
            """), "'Look' " + UnreadAdvice);
        Expect(Members("""
            private int Look() => 1;
            internal static void Run(Fixture? fixture) { _ = fixture?.Look(); }
            """), "'Look' " + UnreadAdvice);
        // An out result discarded with the call's whole report, and an out result no caller reads.
        Expect(Members("""
            private static void Find(int value, out int found) => found = value;
            internal static void Run() => Find(1, out _);
            internal static int Read() { Find(2, out int found); return found; }
            """), "'Find' " + OutAdvice);
        Expect(Members("""
            private static bool TryFind(int value, out int found) { found = value; return value > 0; }
            internal static bool Run() => TryFind(1, out _);
            """), "'TryFind' " + UnreadOutAdvice);
        // A result no caller reads, every call a statement of its own.
        Expect(Members("""
            private static bool Step(int[] values) { values[0]++; return values[0] > 3; }
            internal static void Run(int[] values) { Step(values); Step(values); }
            """), "'Step' " + UnreadAdvice);
        // A parameter discarded by name.
        Expect(Members("internal static int Run(int value, string _) => value;"), "'Run' " + ParameterAdvice);
        // A method declared to return a value that returns null on every path.
        Expect(Members("""
            internal static string? Name(int value) { if (value > 0) return null; return null; }
            internal static int Read() => Name(1)?.Length ?? 0;
            """), "'Name' " + NullAdvice);
        Expect(Members("""
            internal static int? Count() => null;
            internal static int Read() => Count() ?? 0;
            """), NullAdvice);

        // A guard's result, a framework result, a result some caller reads, a delegate's contract, an override, an
        // interface implementation, a framework out result and other assemblies pass.
        Expect(Members("internal static void Run(object value) { _ = SuperMetroid.Core.Ensure.NotNull(value); }"), count: 0);
        // A result some caller reads is the operation's report; a public method's callers are not all in its assembly.
        Expect(Members("""
            private static int Look(int value) => value + 1;
            internal static void Run() { _ = Look(1); }
            internal static int Read() => Look(2);
            """), count: 0);
        Expect(Declarations + " namespace SuperMetroid.Core.Enemies { public static class Open { public static int Look(int value) => value + 1; internal static void Run() { _ = Look(1); } } }", count: 0);
        Expect(Members("internal static void Run(System.Collections.Generic.List<int> values) { _ = values.Remove(1); values.Add(2); }"), count: 0);
        Expect(Members("""
            private static bool Step(int[] values) { values[0]++; return values[0] > 3; }
            internal static void Run(int[] values) { Step(values); if (Step(values)) values[0] = 0; }
            """), count: 0);
        Expect(Members("""
            private static int Ignore(int _) => 0;
            internal static System.Func<int, int> Run() => Ignore;
            """), count: 0);
        Expect(Members("""
            private static string? Nothing(int value) => null;
            internal static System.Func<int, string?> Run() => Nothing;
            """), count: 0);
        Expect(Members("public override bool Equals(object? _) => false;\npublic override int GetHashCode() => 0;"), count: 0);
        Expect(Members("public int CompareTo(Fixture? _) => 0;", baseList: " : System.IComparable<Fixture>"), count: 0);
        Expect(Members("internal static int Run(string text) => int.TryParse(text, out _) ? 1 : 0;"), count: 0);
        Expect(Members("""
            private static bool TryFind(int value, out int found) { found = value; return value > 0; }
            internal static bool Run() => TryFind(1, out _);
            internal static int Read() => TryFind(2, out int found) ? found : 0;
            """), count: 0);
        Expect(Members("internal static System.Func<int, int> Run() => _ => 1;"), count: 0);
        Expect(Members("""
            private static int Look(int value) => value + 1;
            internal static void Run() { _ = Look(1); }
            """), count: 0, assemblyName: "SuperMetroid.Rendering.Direct3D11");
        Expect(Members("""
            private static int Look(int value) => value + 1;
            internal static void Run() { _ = Look(1); }
            """), "'Look' " + UnreadAdvice, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A Core class <c>Fixture</c> holding <paramref name="members"/>.</summary>
    private static string Members(string members, string baseList = "") => Declarations + $$"""

        namespace SuperMetroid.Core.Enemies
        {
            internal sealed class Fixture{{baseList}}
            {
                {{members}}
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6474 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new UnreadResultAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != UnreadResultAnalyzer.UnreadResultId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {UnreadResultAnalyzer.UnreadResultId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
