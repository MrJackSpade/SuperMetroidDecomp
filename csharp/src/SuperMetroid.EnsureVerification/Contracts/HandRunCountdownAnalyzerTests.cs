using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2561 hand-run countdown rule, SME6463.</summary>
internal static class HandRunCountdownAnalyzerTests
{
    /// <summary>The advice every SME6463 message ends with.</summary>
    private const string Advice = "by hand; hold it as an UpdateCountdown (Count, Step or StepEnds say how it ends), or as ElapsedUpdates for a count with no length";

    /// <summary>One positive per way a wait is stepped by hand, and the uses and owners the rule leaves alone.</summary>
    internal static void VerifyHandRunCountdownAnalyzer()
    {
        // Every in-place step by one of a wait-named field, property or local.
        Expect(Body("ActionTimer--;"), "'ActionTimer--' steps the wait 'ActionTimer' " + Advice);
        Expect(Body("if (--ActionTimer == 0) return 1;"), "'--ActionTimer' steps the wait 'ActionTimer' " + Advice);
        Expect(Body("_holdUpdatesLeft++;"), "'_holdUpdatesLeft++' steps the wait '_holdUpdatesLeft' " + Advice);
        Expect(Body("++framesFallen;"), "'++framesFallen' steps the wait 'framesFallen' " + Advice);
        Expect(Body("CooldownTimer -= 1;"), "'CooldownTimer -= 1' steps the wait 'CooldownTimer' " + Advice);
        Expect(Body("RiseWait += 1;"), "'RiseWait += 1' steps the wait 'RiseWait' " + Advice);
        Expect(Body("ActionTimer = ActionTimer - 1;"), "'ActionTimer = ActionTimer - 1' steps the wait 'ActionTimer' " + Advice);
        Expect(Body("ActionTimer = System.Math.Max(0, ActionTimer - 1);"), "steps the wait 'ActionTimer' " + Advice);
        Expect(Body("RiseWait = System.Math.Min(8, 1 + RiseWait);"), "steps the wait 'RiseWait' " + Advice);
        Expect(Body("int timer = 3;\n        timer--;"), "'timer--' steps the wait 'timer' " + Advice);
        Expect(Body("int tick = 0;\n        tick++;"), "'tick++' steps the wait 'tick' " + Advice);

        // A wait set, read, compared or moved by more than one, a count that times no wait, an enum and a loop index are
        // not stepped by hand.
        Expect(Body("ActionTimer = 5;"), count: 0);
        Expect(Body("ActionTimer = RiseWait;"), count: 0);
        Expect(Body("CooldownTimer -= 2;"), count: 0);
        Expect(Body("ActionTimer = ActionTimer - RiseWait;"), count: 0);
        Expect(Body("return ActionTimer == 0 ? 1 : 0;"), count: 0);
        Expect(Body("BombCount++;"), count: 0);
        Expect(Body("HitCounter++;"), count: 0);
        Expect(Body("Phase++;"), count: 0);
        Expect(Body("for (int index = 0; index < 4; index++) BombCount += index;"), count: 0);
        // The shared countdown types themselves step their counts, and outside Core the rule does not apply.
        Expect(Owner, count: 0);
        Expect(Body("ActionTimer--;"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A Core class whose method <c>Run</c> runs <paramref name="statements"/> over a few counts.</summary>
    private static string Body(string statements) => $$"""
        namespace SuperMetroid.Core.Enemies
        {
            internal enum Stage { First, Second }
            internal sealed class Fixture
            {
                internal int ActionTimer { get; set; }
                internal int RiseWait { get; set; }
                internal int BombCount { get; set; }
                internal int HitCounter { get; set; }
                internal Stage Phase;
                private int _holdUpdatesLeft;
                private ushort CooldownTimer;
                private int framesFallen;
                internal int Run()
                {
                    {{statements}}
                    return _holdUpdatesLeft + CooldownTimer + framesFallen;
                }
            }
        }
        """;

    /// <summary>The shared countdown and elapsed count, which own stepping their counts by one.</summary>
    private const string Owner = """
        namespace SuperMetroid.Core
        {
            public sealed class UpdateCountdown
            {
                public int UpdatesLeft { get; private set; }
                internal bool Count() { UpdatesLeft--; return UpdatesLeft == 0; }
            }
            public sealed class ElapsedUpdates
            {
                private int ticks;
                internal void Count() => ticks++;
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6463 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new HandRunCountdownAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != HandRunCountdownAnalyzer.HandRunCountdownId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {HandRunCountdownAnalyzer.HandRunCountdownId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
