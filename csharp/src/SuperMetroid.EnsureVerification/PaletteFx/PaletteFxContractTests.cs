using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.EnsureVerification.PaletteFx;

/// <summary>Checks that palette FX execution writes no colours: a step takes its conditions and no colour source (#1512).</summary>
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
        VerifyPaletteCall("removed optional binding", "fx.BindPresentationColors(colors);", "CS1061");

        // Guard against a compatibility overload or a hidden colour source that would bring colour writing back.
        MethodInfo[] steps = typeof(RoomPaletteFxSystem).GetMethods().Where(method => method.Name == "Step").ToArray();
        if (steps.Length != 1 || steps[0].GetParameters().Any(parameter =>
                typeof(IPaletteFxColorSource).IsAssignableFrom(parameter.ParameterType)))
            throw new InvalidOperationException("Palette FX must expose exactly one Step, which takes no colour source.");
        if (MetadataShape.Fields(typeof(RoomPaletteFxSystem), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(field => typeof(IPaletteFxColorSource).IsAssignableFrom(field.FieldType)))
            throw new InvalidOperationException("Palette FX must not retain a colour source across execution/state restore.");

        // A frontend screen's step writes no colours either: it hands the screen each record as it is shown, which the
        // screen shades its pictures with. The gunship engine flickers on the Ceres destruction cinematic, so its first
        // step shows the flicker's first record in the engine's colours.
        var fxSession = new PaletteFxSessionState();
        var fx = new RoomPaletteFxSystem(fxSession);
        fx.Start(PaletteFxObject.CutsceneGunshipEngineFlicker, 0);
        var shown = new List<(PaletteFxColours Colours, PaletteFxFrame Record)>();
        fx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, TorizoIsDead = false, AreaMiniBossDefeated = false },
            (colours, record) => shown.Add((colours, record)));
        PaletteFxFrame first = PaletteFxColorFrames.Of(PaletteFxProgram.CutsceneGunshipEngineFlicker1, colorsPerFrame: 1).Color(0, 0).Frame;
        if (shown.Count != 1 || shown[0] != (PaletteFxColours.CutsceneGunshipEngine, first)
            || fx.RecordOf(PaletteFxColours.CutsceneGunshipEngine) != first || fx.ActiveCount != 1)
            throw new InvalidOperationException("The first screen step did not show the gunship engine's first record.");
        Console.WriteLine("#1512: two compiler contracts, the colourless step shape and the screen step pass; no gameplay search.");
    }

    /// <summary>Compiles one caller statement and checks its expected diagnostic.</summary>
    private static void VerifyPaletteCall(string description, string statement, string? expectedError = null)
    {
        string source = $$"""
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            using SuperMetroid.Core.Fx.PaletteFx;
            class PaletteCaller
            {
                void Run(RoomPaletteFxSystem fx, IPaletteFxColorSource colors, PaletteFxConditions frame)
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
