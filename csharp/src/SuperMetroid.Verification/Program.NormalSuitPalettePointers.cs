using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies native normal-suit palette pointers for all Varia and Gravity combinations and checks that installed palette restoration matches cartridge colors without runtime bus reads.</summary>
    private static void VerifyNormalSuitPalettePointers()
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
            ushort nativePointer = (ushort)(rom.ReadByte(source) | rom.ReadByte(source + 1) << 8);
            AssertEqual(expectedPointers[selection], nativePointer,
                $"normal suit pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer, SamusPaletteRomData.Common.NormalSuitPalettePointer(offset),
                $"compiled normal suit pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 2,
                SuitColors = suitColors,
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
}
