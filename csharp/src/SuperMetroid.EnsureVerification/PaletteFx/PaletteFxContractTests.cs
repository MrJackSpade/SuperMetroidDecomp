using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.EnsureVerification.PaletteFx;

/// <summary>Checks that palette FX execution requires a supplied color source at compile and runtime.</summary>
internal static class PaletteFxContractTests
{
    /// <summary>
    /// Confirms #1152's required color dependency using compiler-only fixtures.
    /// These are finite API-contract checks, not a gameplay or palette-program sweep.
    /// </summary>
    internal static void VerifyPaletteFxDependencyContract()
    {
        VerifyPaletteCall("explicit provider", "fx.Step(cgram, colors, frame);");
        VerifyPaletteCall("old positional call", "fx.Step(cgram, frame);", "CS7036");
        VerifyPaletteCall("omitted named provider",
            "fx.Step(cgram, conditions: frame);",
            "CS7036");
        VerifyPaletteCall("explicit named provider",
            "fx.Step(cgram, colors: colors, conditions: frame);");
        VerifyPaletteCall("wrong provider type", "fx.Step(cgram, new object(), frame);", "CS1503");
        VerifyPaletteCall("null literal", "fx.Step(cgram, null, frame);", "CS8625");
        VerifyPaletteCall("nullable provider", "fx.Step(cgram, optionalColors, frame);", "CS8604");
        VerifyPaletteCall("removed optional binding", "fx.BindPresentationColors(colors);", "CS1061");

        // Guard against adding a compatibility overload or hidden serialized provider
        // that would undo the required dependency, especially after state restoration.
        MethodInfo[] steps = typeof(RoomPaletteFxSystem).GetMethods().Where(method => method.Name == "Step").ToArray();
        if (steps.Length != 1 || !steps[0].GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(IPaletteFxColorSource) && !parameter.IsOptional))
            throw new InvalidOperationException("Palette FX must expose exactly one Step with mandatory colors.");
        if (MetadataShape.Fields(typeof(RoomPaletteFxSystem), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(field => typeof(IPaletteFxColorSource).IsAssignableFrom(field.FieldType)))
            throw new InvalidOperationException("Palette FX must not retain a hidden color binding across execution/state restore.");

        // The colors dependency is non-null by contract and the null-forgiving operator is banned (#623), so the
        // compiler rejects a null caller. A frontend screen's step writes no colours (#1512): it hands the screen each
        // record as it is shown, which the screen shades its pictures with. The gunship engine flickers on the Ceres
        // destruction cinematic, so its first step shows the flicker's first record in the engine's colours.
        var fxSession = new PaletteFxSessionState();
        var fx = new RoomPaletteFxSystem(fxSession);
        fx.SpawnDefinition(PaletteFxObject.CutsceneGunshipEngineFlicker, 0);
        var shown = new List<(PaletteFxColours Colours, PaletteFxFrame Record)>();
        fx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false },
            (colours, record) => shown.Add((colours, record)));
        PaletteFxFrame first = PaletteFxColorFrames.Of(PaletteFxProgram.CutsceneGunshipEngineFlicker1, colorsPerFrame: 1).Color(0, 0).Frame;
        if (shown.Count != 1 || shown[0] != (PaletteFxColours.CutsceneGunshipEngine, first)
            || fx.RecordOf(PaletteFxColours.CutsceneGunshipEngine) != first || fx.ActiveCount != 1)
            throw new InvalidOperationException("The first screen step did not show the gunship engine's first record.");
        Console.WriteLine("#1152: eight compiler dependency contracts and the supplied-colors step pass; no gameplay search.");
    }

    /// <summary>Compiles one caller statement and checks its expected color-provider diagnostic.</summary>
    private static void VerifyPaletteCall(string description, string statement, string? expectedError = null)
    {
        string source = $$"""
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            using SuperMetroid.Core.Fx.PaletteFx;
            class PaletteCaller
            {
                void Run(RoomPaletteFxSystem fx, IPaletteFxColourOwners cgram,
                    IPaletteFxColorSource colors, IPaletteFxColorSource? optionalColors, PaletteFxConditions frame)
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
