using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Checks suit-dependent Speed Booster palette selection and its installed CGRAM colors.</summary>
internal static class SpeedBoostPalettePointersVerification
{
    /// <summary>Matches four equipment selections to native pointers and live palette output, including failure without installed colors.</summary>
    internal static void VerifySpeedBoostPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusFullBodyCycleColorCatalog cycleColors = SamusFullBodyCycleColorFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(rom)));
        SamusEquipmentFlags[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        ushort[] expectedPointers = [0x9b80, 0x9d80, 0x9f80, 0x9f80];
        for (int selection = 0; selection < equipment.Length; selection++)
        {
            ushort offset = equipment[selection].GetSuitPaletteTableOffset();
            int source = SamusPaletteDefinitions.FullBodyCycles.SpeedBoostPointers + offset;
            ushort nativePointer = rom.ReadCartridgeWord(source);
            AssertEqual(expectedPointers[selection], nativePointer,
                $"Speed Booster pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer,
                SamusPaletteDefinitions.FullBodyCycles.SpeedBoostPalettePointer(offset),
                $"compiled Speed Booster pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 1,
                Presentation = { FullBodyCycleColors = cycleColors },
            };
            var installedColors = new SamusColourRig();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColors.Samus, samus),
                "installed special palette loads Speed Booster shade without a CPU bus");
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual((ushort)(ReadNativeSpeedBoosterShadeColor(rom, nativePointer, color) & 0x7fff),
                    installedColors.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + color],
                    $"Speed Booster CGRAM selection {selection}, color {color} matches ROM");
            AssertEqual(2, samus.SpecialSuperPaletteFlags,
                "special palette counter increments");
        }

        // The last alternating phase still draws and advances onto the drained/rainbow branch, which the
        // alternation then leaves alone: the cartridge's signed test stops the phase there instead of wrapping.
        const int LastAlternatingPhase = 32767;
        var lastPhase = new SamusState
        {
            SpecialSuperPaletteFlags = LastAlternatingPhase,
            Presentation = { FullBodyCycleColors = cycleColors },
        };
        AssertTrue(SamusSpecialSuperPalette.Update(new SamusColourRig().Samus, lastPhase),
            "the last alternating phase still owns the palette");
        AssertEqual(LastAlternatingPhase + 1, lastPhase.SpecialSuperPaletteFlags,
            "the last alternating phase advances onto the drained branch");
        AssertTrue(!SamusSpecialSuperPalette.Update(new SamusColourRig().Samus, lastPhase),
            "the drained branch is not the Metroid alternation");
        AssertEqual(LastAlternatingPhase + 1, lastPhase.SpecialSuperPaletteFlags,
            "the alternation leaves the drained branch's phase alone");
        var highestPhase = new SamusState
        {
            SpecialSuperPaletteFlags = ushort.MaxValue,
            Presentation = { FullBodyCycleColors = cycleColors },
        };
        AssertTrue(!SamusSpecialSuperPalette.Update(new SamusColourRig().Samus, highestPhase),
            "the highest stored phase is the drained branch too");
        AssertEqual(ushort.MaxValue, highestPhase.SpecialSuperPaletteFlags,
            "the highest stored phase does not wrap to zero");
        AssertThrows<InvalidOperationException>(() => SamusSpecialSuperPalette.Update(
            new SamusColourRig().Samus, new SamusState { SpecialSuperPaletteFlags = 1 }),
            "Metroid attachment requires installed Speed Booster colors");
        Console.WriteLine("Speed Booster palettes: four equipment cases match ROM and live CGRAM with no CPU bus; missing assets fail.");
    }

    /// <summary>Reads one native full-body shade as the verification oracle for installed Speed Booster colors.</summary>
    private static ushort ReadNativeSpeedBoosterShadeColor(IImportCartridgeSource rom, int nativePointer, int color) =>
        RomDataReader.ReadWordFixedBank(rom, SamusSourceDefinitions.PaletteBank.Value | (nativePointer + color * sizeof(ushort)));

    /// <summary>Runs the Speed Booster palette pointer and CGRAM checks.</summary>
    [VerificationCommand("--speed-boost-palette-pointers")]
    internal static int SpeedBoostPalettePointersCommand()
    {
        Suite(nameof(VerifySpeedBoostPalettePointers), () => VerifySpeedBoostPalettePointers());
        return 0;
    }
}
