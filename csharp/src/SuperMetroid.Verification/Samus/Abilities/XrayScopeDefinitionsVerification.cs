using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementDefinitionsVerification;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Audits X-ray angle bands and retail data ranges used by Samus presentation.</summary>
internal static class XrayScopeDefinitionsVerification
{
    /// <summary>
    /// Validates the shared X-ray palette and tangent ranges against the private retail
    /// cartridge. The behavioral verifier separately exercises standing, crouching,
    /// turning, visor cycling, restoration, and the visible window produced by the table.
    /// </summary>
    internal static void VerifySamusXrayRomData()
    {
        RotationAngle[] angleBands =
        [
            XrayScopeDefinitions.AnimationAngles.RightRaisedFrom,
            XrayScopeDefinitions.AnimationAngles.RightLevelFrom,
            XrayScopeDefinitions.AnimationAngles.RightLoweredFrom,
            XrayScopeDefinitions.AnimationAngles.RightDownFrom,
            XrayScopeDefinitions.AnimationAngles.LeftDownBelow,
            XrayScopeDefinitions.AnimationAngles.LeftLoweredBelow,
            XrayScopeDefinitions.AnimationAngles.LeftLevelBelow,
            XrayScopeDefinitions.AnimationAngles.LeftRaisedBelow,
        ];
        for (int index = 1; index < angleBands.Length; index++)
        {
            AssertTrue(angleBands[index - 1] < angleBands[index],
                $"X-ray animation boundary {index} follows its predecessor");
        }

        AssertEqual(RotationAngle.StepsPerTurn / 2 + 1,
            XrayScopeDefinitions.Window.AbsoluteTangentWordCount,
            "X-ray absolute tangent table includes both half-turn endpoints");
        AssertEqual(XrayScopeDefinitionsPaletteConstants.SamusCgramIndex + 4,
            XrayScopeDefinitionsPaletteConstants.VisorCgramIndex,
            "X-ray visor occupies Samus palette color four");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus X-ray ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        TouchRange(bus, XrayScopeDefinitionsConstants.VisorShades.VisorWords,
            (SuperMetroid.Core.Samus.Colours.SamusShadeDefinitions.Visor.XrayWideningShades + XrayScopeDefinitionsConstants.VisorShades.FullCycleShades) * sizeof(ushort),
            "X-ray visor palette words");
        TouchRange(bus, XrayScopeDefinitionsConstants.VisorShades.NormalSuitPointers,
            3 * sizeof(ushort), "X-ray normal-suit palette pointers");
        TouchRange(bus, XrayScopeDefinitions.Window.AbsoluteTangentTable,
            XrayScopeDefinitions.Window.AbsoluteTangentWordCount * sizeof(ushort),
            "X-ray absolute tangent table");

        Console.WriteLine(
            "  Samus X-ray ROM data: animation bands, visor palettes, and all 129 tangent words are in range.");
    }
}
