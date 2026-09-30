using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    /// <summary>
    /// Confirms #1152's required color dependency using compiler-only fixtures.
    /// These are finite API-contract checks, not a gameplay or palette-program sweep.
    /// </summary>
    private static void VerifyPaletteFxDependencyContract()
    {
        VerifyPaletteCall("explicit provider", "fx.Step(bus, cgram, colors, 0, 0, false, false);");
        VerifyPaletteCall("old positional call", "fx.Step(bus, cgram, 0, 0, false, false);", "CS7036");
        VerifyPaletteCall("omitted named provider",
            "fx.Step(bus, cgram, samusY: 0, equippedItems: 0, enemyZeroIsDead: false, areaMiniBossDefeated: false);",
            "CS7036");
        VerifyPaletteCall("explicit named provider",
            "fx.Step(bus, cgram, colors: colors, samusY: 0, equippedItems: 0, enemyZeroIsDead: false, areaMiniBossDefeated: false);");
        VerifyPaletteCall("wrong provider type", "fx.Step(bus, cgram, new object(), 0, 0, false, false);", "CS1503");
        VerifyPaletteCall("null literal", "fx.Step(bus, cgram, null, 0, 0, false, false);", "CS8625");
        VerifyPaletteCall("nullable provider", "fx.Step(bus, cgram, optionalColors, 0, 0, false, false);", "CS8604");
        VerifyPaletteCall("removed optional binding", "fx.BindPresentationColors(colors);", "CS1061");

        // Guard against adding a compatibility overload or hidden serialized provider
        // that would undo the required dependency, especially after state restoration.
        MethodInfo[] steps = typeof(RoomPaletteFxSystem).GetMethods().Where(method => method.Name == "Step").ToArray();
        if (steps.Length != 1 || !steps[0].GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(IPaletteFxColorSource) && !parameter.IsOptional))
            throw new InvalidOperationException("Palette FX must expose exactly one Step with mandatory colors.");
        if (typeof(RoomPaletteFxSystem).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(field => typeof(IPaletteFxColorSource).IsAssignableFrom(field.FieldType)))
            throw new InvalidOperationException("Palette FX must not retain a hidden color binding across execution/state restore.");

        // Deliberate null-forgiving/reflection callers can bypass C# nullability.
        // They must still fail before mutating native slot timers or CGRAM.
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var cgram = new SnesCgram();
        var fx = new RoomPaletteFxSystem();
        fx.SpawnDefinition(bus, CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineDefinitionPointer, 0);
        try
        {
            fx.Step(bus, cgram, null!, 0, 0, false, false);
            throw new InvalidOperationException("Expected the explicit null dependency to fail.");
        }
        catch (ArgumentNullException exception) when (exception.ParamName == "colors")
        {
            // Expected failure stays inside the guarded verifier, never Windows UI.
        }
        int colorIndex = CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineColorIndex / sizeof(ushort);
        if (cgram.Colors[colorIndex] != 0 || fx.ActiveCount != 1)
            throw new InvalidOperationException("Rejected null colors mutated CGRAM or native slots.");
        fx.Step(bus, cgram, new ContractPaletteColors(), 0, 0, false, false);
        if (cgram.Colors[colorIndex] != 31)
            throw new InvalidOperationException("Rejected null colors advanced the native palette instruction timer.");
        Console.WriteLine("#1152: eight compiler dependency contracts and pre-mutation null guard pass; no gameplay search.");
    }

    private static void VerifyPaletteCall(string description, string statement, string? expectedError = null)
    {
        string source = $$"""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            class PaletteCaller
            {
                void Run(RoomPaletteFxSystem fx, ISnesAddressSpace bus, SnesCgram cgram,
                    IPaletteFxColorSource colors, IPaletteFxColorSource? optionalColors)
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

    private sealed class ContractPaletteColors : IPaletteFxColorSource
    {
        public bool TryReadColor(ushort pointer, out ushort color)
        {
            color = 31; // Constructed red confirms the first native frame executes after the rejection.
            return pointer == CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineColorPointer(0, 0);
        }
    }
}
