using SuperMetroid.Core.Hardware;
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
        byte[] angleBands =
        [
            XrayScopeDefinitions.AnimationAngles.RightFrameOne,
            XrayScopeDefinitions.AnimationAngles.RightFrameTwo,
            XrayScopeDefinitions.AnimationAngles.RightFrameThree,
            XrayScopeDefinitions.AnimationAngles.RightFrameFour,
            XrayScopeDefinitions.AnimationAngles.LeftFrameFour,
            XrayScopeDefinitions.AnimationAngles.LeftFrameThree,
            XrayScopeDefinitions.AnimationAngles.LeftFrameTwo,
            XrayScopeDefinitions.AnimationAngles.LeftFrameOne,
        ];
        for (int index = 1; index < angleBands.Length; index++)
        {
            AssertTrue(angleBands[index - 1] < angleBands[index],
                $"X-ray animation boundary {index} follows its predecessor");
        }

        AssertEqual(RotationAngle.HalfTurn.TableIndex + 1,
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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        TouchRange(bus, XrayScopeDefinitions.Palette.VisorWords,
            XrayScopeDefinitions.Palette.FullCycleEndWordOffset,
            "X-ray visor palette words");
        TouchRange(bus, XrayScopeDefinitions.Palette.NormalSuitPointers,
            3 * sizeof(ushort), "X-ray normal-suit palette pointers");
        TouchRange(bus, XrayScopeDefinitions.Window.AbsoluteTangentTable,
            XrayScopeDefinitions.Window.AbsoluteTangentWordCount * sizeof(ushort),
            "X-ray absolute tangent table");

        Console.WriteLine(
            "  Samus X-ray ROM data: animation bands, visor palettes, and all 129 tangent words are in range.");
    }
}
