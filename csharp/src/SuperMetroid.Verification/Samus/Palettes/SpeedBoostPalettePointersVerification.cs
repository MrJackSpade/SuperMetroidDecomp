using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;

/// <summary>Checks suit-dependent Speed Booster palette selection and its installed CGRAM colors.</summary>
internal static class SpeedBoostPalettePointersVerification
{
    /// <summary>Matches four equipment selections to native pointers and live palette output, including failure without installed colors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for speed boost palette pointers")]
    internal static void VerifySpeedBoostPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusFullBodyCycleColorCatalog cycleColors = SamusFullBodyCycleColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(rom)));
        ushort[] equipment =
        [
            0,
            (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        ushort[] expectedPointers = [0x9b80, 0x9d80, 0x9f80, 0x9f80];
        for (int selection = 0; selection < equipment.Length; selection++)
        {
            ushort offset = equipment[selection].GetSuitPaletteTableOffset();
            int source = SamusPaletteRomData.FullBodyCycles.SpeedBoostPointers + offset;
            ushort nativePointer = rom.ReadCartridgeWord(source);
            AssertEqual(expectedPointers[selection], nativePointer,
                $"Speed Booster pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer,
                SamusPaletteRomData.FullBodyCycles.SpeedBoostPalettePointer(offset),
                $"compiled Speed Booster pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 1,
                Presentation = { FullBodyCycleColors = cycleColors },
            };
            var installedColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColors, samus),
                "installed special palette loads Speed Booster shade without a CPU bus");
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual((ushort)(ReadNativeSpeedBoosterShadeColor(rom, nativePointer, color) & 0x7fff),
                    installedColors.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                    $"Speed Booster CGRAM selection {selection}, color {color} matches ROM");
            AssertEqual((ushort)2, samus.SpecialSuperPaletteFlags,
                "special palette counter increments");
        }
        AssertThrows<InvalidOperationException>(() => SamusSpecialSuperPalette.Update(
            new SnesCgram(), new SamusState { SpecialSuperPaletteFlags = 1 }),
            "Metroid attachment requires installed Speed Booster colors");
        Console.WriteLine("Speed Booster palettes: four equipment cases match ROM and live CGRAM with no CPU bus; missing assets fail.");
    }

    /// <summary>Reads one native full-body shade as the verification oracle for installed Speed Booster colors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Speed Booster full-body shade colors")]
    private static ushort ReadNativeSpeedBoosterShadeColor(IImportCartridgeSource rom, int nativePointer, int color) =>
        RomDataReader.ReadWordFixedBank(rom, SamusPaletteRomData.Banks.Palette | (nativePointer + color * sizeof(ushort)));

    /// <summary>Runs the Speed Booster palette pointer and CGRAM checks.</summary>
    [VerificationCommand("--speed-boost-palette-pointers")]
    internal static int SpeedBoostPalettePointersCommand()
    {
        Suite(nameof(VerifySpeedBoostPalettePointers), () => VerifySpeedBoostPalettePointers());
        return 0;
    }
}
