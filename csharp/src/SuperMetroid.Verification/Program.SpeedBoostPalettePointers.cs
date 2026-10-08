using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySpeedBoostPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusFullBodyCycleColorCatalog cycleColors = SamusFullBodyCycleColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.SamusFullBodyCycleColorExtractor.Extract(rom)));
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
            ushort nativePointer = (ushort)(rom.ReadByte(source) | rom.ReadByte(source + 1) << 8);
            AssertEqual(expectedPointers[selection], nativePointer,
                $"Speed Booster pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer,
                SamusPaletteRomData.FullBodyCycles.SpeedBoostPalettePointer(offset),
                $"compiled Speed Booster pointer selection {selection} matches retail ROM");

            var samus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 1,
                FullBodyCycleColors = cycleColors,
            };
            var installedColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColors, samus),
                "installed special palette loads Speed Booster shade without a CPU bus");
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual((ushort)(RomDataReader.ReadWordFixedBank(rom,
                        SamusPaletteRomData.Banks.Palette | (nativePointer + color * sizeof(ushort))) & 0x7fff),
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
}
