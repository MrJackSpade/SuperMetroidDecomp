using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Checks normal suit pointer selection and live palette restoration against retail colors.</summary>
internal static class NormalSuitPalettePointersVerification
{
    /// <summary>Compares four equipment selections and every installed Samus CGRAM color.</summary>
    internal static void VerifyNormalSuitPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledSamusSuitColors suitColors = SamusSuitColorFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(rom)));
        SamusEquipmentFlags[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        ushort[] expectedPointers = [0x9400, 0x9520, 0x9800, 0x9800];
        for (int selection = 0; selection < equipment.Length; selection++)
        {
            ushort offset = equipment[selection].GetSuitPaletteTableOffset();
            int source = SamusPaletteDefinitions.Common.NormalSuitPointers + offset;
            ushort nativePointer = LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, source), CpuBusRegionRead.Read(rom, source + 1));
            AssertEqual(expectedPointers[selection], nativePointer,
                $"normal suit pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer, SamusPaletteDefinitions.Common.NormalSuitPalettePointer(offset),
                $"compiled normal suit pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 2,
                Presentation = { SuitColors = suitColors },
            };
            var installedColors = new SamusColourRig();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColors.Samus, samus),
                "installed special palette restores normal suit without a CPU bus");
            for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
                AssertEqual((ushort)(RomDataReader.ReadWordFixedBank(rom,
                        SamusSourceDefinitions.PaletteBank.Value | (nativePointer + color * sizeof(ushort))) & 0x7fff),
                    installedColors.Colors[SamusPaletteDefinitions.Common.SuitFirstColour + color],
                    $"normal suit CGRAM selection {selection}, color {color} matches ROM");
            AssertEqual(3, samus.SpecialSuperPaletteFlags,
                "special palette counter increments");
        }
        Console.WriteLine("Normal suit palettes: four equipment cases match ROM and live CGRAM with no CPU bus.");
    }

    /// <summary>Runs the focused suit pointer and CGRAM parity check.</summary>
    [VerificationCommand("--normal-suit-palette-pointers")]
    internal static int NormalSuitPalettePointersCommand()
    {
        Suite(nameof(VerifyNormalSuitPalettePointers), () => VerifyNormalSuitPalettePointers());
        return 0;
    }
}
