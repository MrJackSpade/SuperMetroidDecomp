using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.EnsureVerification.PaletteFx;

/// <summary>Checks that palette FX execution writes no colours: a step takes its conditions alone (#1512, #1583).</summary>
internal static class PaletteFxContractTests
{
    /// <summary>
    /// Confirms that a palette-FX step writes no colours (#1512), using compiler-only fixtures: a gameplay step takes its
    /// conditions alone, since every record shown is a shade the import pre-coloured from the installed colours.
    /// These are finite API-contract checks, not a gameplay or palette-program sweep.
    /// </summary>
    internal static void VerifyPaletteFxDependencyContract()
    {
        VerifyPaletteCall("conditions only", "fx.Step(frame);");
        VerifyPaletteCall("removed optional binding", "fx.BindPresentationColors(frame);", "CS1061");

        // Guard against a compatibility overload that would bring colour writing back: Step takes the conditions alone, and
        // no overload takes a colour source besides. Core declares no colour source at all; the colours are the import's (#1583).
        VerifyPaletteCall("no overload takes colours with the conditions", "fx.Step(frame, null);", "CS1501");

        // A frontend screen's step writes no colours either: each effect holds the record it shows, which the screen
        // shades its pictures with (#1597). The gunship engine flickers on the Ceres destruction cinematic, so its first
        // step shows the flicker's first record in the engine's colours.
        var fx = ColourEffectSystem.ForScreen();
        fx.StartScreen(ColourEffectId.CutsceneGunshipEngineFlicker);
        fx.StepScreen();
        IReadOnlyList<ShownColourEffectFrame> shown = fx.Shown;
        var first = new ColourEffectFrame(ColourEffectId.CutsceneGunshipEngineFlicker, 0);
        if (shown.Count != 1 || shown[0] != new ShownColourEffectFrame(ColourEffectColours.CutsceneGunshipEngine, first)
            || fx.FrameOf(ColourEffectColours.CutsceneGunshipEngine) != first || fx.ActiveCount != 1)
            throw new InvalidOperationException("The first screen step did not show the gunship engine's first record.");
        Console.WriteLine("#1512: three compiler contracts, the colourless step shape and the screen step pass; no gameplay search.");
    }

    /// <summary>Compiles one caller statement and checks its expected diagnostic.</summary>
    private static void VerifyPaletteCall(string description, string statement, string? expectedError = null)
    {
        string source = $$"""
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.ColourEffects;
            class PaletteCaller
            {
                void Run(ColourEffectSystem fx, ColourEffectInputs frame)
                {
                    {{statement}}
                }
            }
            """;
        var compilation = CSharpCompilation.Create("PaletteDependencyContract",
            [CSharpSyntaxTree.ParseText(source)], References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable,
                generalDiagnosticOption: ReportDiagnostic.Error));
        Diagnostic[] errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (expectedError is null ? errors.Length != 0 : errors.Length != 1 || errors[0].Id != expectedError)
            throw new InvalidOperationException(
                $"{description}: expected {expectedError ?? "successful compilation"}; got {string.Join("; ", errors.Select(error => error.ToString()))}");
    }
}
