using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2510 hand-mapped random sample field rule, SME6460.</summary>
internal static class RandomSampleSelectionAnalyzerTests
{
    /// <summary>The advice every SME6460 message ends with.</summary>
    private const string Advice = " a value read from a random sample field; make the choice a named WeightedRandomChoice or RandomChance";

    /// <summary>
    /// The random sample field, the enemies' draw that takes one and the element lookup a fixture calls, as Core declares
    /// them.
    /// </summary>
    private const string Declarations = """
        namespace SuperMetroid.Core.Compatibility
        {
            public readonly record struct RandomSampleField(int Shift, int Width)
            {
                public int Of(int sample) => (sample >> Shift) & ((1 << Width) - 1);
                public int OfAdvancedBy(int sample, int steps) => (Of(sample) + steps) & ((1 << Width) - 1);
            }
        }
        namespace SuperMetroid.Core
        {
            using System.Collections.Generic;
            using SuperMetroid.Core.Compatibility;
            public static class Ensure
            {
                public static T Element<T>(IReadOnlyList<T> items, int index) => items[index];
            }
            public interface IRandom { int Generate(); }
            public static class Draws
            {
                public static int Next(this IRandom random, RandomSampleField field) => field.Of(random.Generate());
            }
        }
        """;

    /// <summary>One positive per way a field read is mapped by hand, and the number uses the rule leaves alone.</summary>
    internal static void VerifyRandomSampleSelectionAnalyzer()
    {
        // A list indexed by a field read, directly, through the draw, through a local, and by Ensure.Element.
        Expect(Body("return Speeds[Choice.Of(sample)];"), "'Speeds[Choice.Of(sample)]' indexes a list with" + Advice);
        Expect(Body("return Speeds[random.Next(Choice)];"), "'Speeds[random.Next(Choice)]' indexes a list with" + Advice);
        Expect(Body("int pick = Choice.Of(sample);\n        return Speeds[pick];"), "'Speeds[pick]' indexes a list with" + Advice);
        Expect(Body("return Ensure.Element(List, Choice.OfAdvancedBy(sample, 1));"),
            "'Ensure.Element(List, Choice.OfAdvancedBy(sample, 1))' looks up a list element by" + Advice);
        Expect(Body("return List[Choice.Of(sample)];"), "'List[Choice.Of(sample)]' indexes a list with" + Advice);
        // A field read compared with a constant, either side, through a local assigned later, in a redraw loop, and as a
        // constant pattern or a switch.
        Expect(Body("return random.Next(Choice) == 3 ? 1 : 0;"), "'random.Next(Choice) == 3' compares with a constant" + Advice);
        Expect(Body("return 0 < Choice.Of(sample) ? 1 : 0;"), "'0 < Choice.Of(sample)' compares with a constant" + Advice);
        Expect(Body("int choice;\n        do\n            choice = random.Next(Choice);\n        while (choice == 0);\n        return 1;"),
            "'choice == 0' compares with a constant" + Advice);
        Expect(Body("int count = Choice.Of(sample);\n        return count is 0 ? 2 : count;"),
            "'count is 0' matches a constant pattern against" + Advice);
        Expect(Body("return Choice.Of(sample) switch { 0 => 4, _ => 1 };"), "switches on" + Advice);
        Expect(Body("switch (Choice.Of(sample)) { case 0: return 4; default: return 1; }"), "switches on" + Advice);

        // A field read used as a number, passed on, combined in arithmetic or compared with another number is not a choice.
        Expect(Body("return Choice.Of(sample) + 16;"), count: 0);
        Expect(Body("int offset = Choice.Of(sample);\n        return -offset;"), count: 0);
        Expect(Body("return System.Math.Max(random.Next(Choice), 8);"), count: 0);
        Expect(Body("return Choice.Of(sample) < sample ? 1 : 0;"), count: 0);
        Expect(Body("int index = sample & 3;\n        return Speeds[index];"), count: 0);
        // The field's own reads are arithmetic, and outside Core the rule does not apply.
        Expect(Body("return 0;"), count: 0);
        Expect(Body("return Speeds[Choice.Of(sample)];"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A Core class whose method <c>Pick</c> runs <paramref name="statements"/> over a field, a list and a draw.</summary>
    private static string Body(string statements) => Declarations + $$"""

        namespace SuperMetroid.Core.Enemies
        {
            using System.Collections.Generic;
            using SuperMetroid.Core.Compatibility;
            internal static class Fixture
            {
                private static readonly RandomSampleField Choice = new(1, 3);
                private static readonly int[] Speeds = [1, 2, 3, 4, 5, 6, 7, 8];
                private static readonly IReadOnlyList<int> List = Speeds;
                internal static int Pick(IRandom random, int sample)
                {
                    {{statements}}
                }
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6460 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RandomSampleSelectionAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != RandomSampleSelectionAnalyzer.HandMappedRandomFieldId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {RandomSampleSelectionAnalyzer.HandMappedRandomFieldId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
