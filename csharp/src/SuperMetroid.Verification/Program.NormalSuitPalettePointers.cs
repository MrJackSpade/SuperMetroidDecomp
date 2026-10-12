using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>Represents normal suit palette pointers verification.</summary>
internal static class NormalSuitPalettePointersVerification
{
    /// <summary>Verifies normal suit palette pointers.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the normal-suit palette colors")]
    internal static void VerifyNormalSuitPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusSuitColorCatalog suitColors = SamusSuitColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.SamusSuitColorExtractor.Extract(rom)));
        ushort[] equipment =
        [
            0,
            (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        ushort[] expectedPointers = [0x9400, 0x9520, 0x9800, 0x9800];
        for (int selection = 0; selection < equipment.Length; selection++)
        {
            ushort offset = equipment[selection].GetSuitPaletteTableOffset();
            int source = SamusPaletteRomData.Common.NormalSuitPointers + offset;
            ushort nativePointer = (ushort)(SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, source), CpuBusRegionRead.Read(rom, source + 1)));
            AssertEqual(expectedPointers[selection], nativePointer,
                $"normal suit pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer, SamusPaletteRomData.Common.NormalSuitPalettePointer(offset),
                $"compiled normal suit pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 2,
                Presentation = { SuitColors = suitColors },
            };
            var installedColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColors, samus),
                "installed special palette restores normal suit without a CPU bus");
            for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
                AssertEqual((ushort)(RomDataReader.ReadWordFixedBank(rom,
                        SamusPaletteRomData.Banks.Palette | (nativePointer + color * sizeof(ushort))) & 0x7fff),
                    installedColors.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                    $"normal suit CGRAM selection {selection}, color {color} matches ROM");
            AssertEqual((ushort)3, samus.SpecialSuperPaletteFlags,
                "special palette counter increments");
        }
        Console.WriteLine("Normal suit palettes: four equipment cases match ROM and live CGRAM with no CPU bus.");
    }

    /// <summary>Normal suit palette pointers command.</summary>
    [VerificationCommand("--normal-suit-palette-pointers")]
    internal static int NormalSuitPalettePointersCommand()
    {
        Suite(nameof(VerifyNormalSuitPalettePointers), () => VerifyNormalSuitPalettePointers());
        return 0;
    }
}
