using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNormalSuitPalettePointers()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guarded = new NormalSuitPointerReadGuard(rom);
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

            var nativeSamus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 2,
            };
            var guardedSamus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 2,
            };
            var nativeColors = new SnesCgram();
            var installedColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(rom, nativeColors, nativeSamus),
                "native special palette restores normal suit");
            AssertTrue(SamusSpecialSuperPalette.Update(guarded, installedColors, guardedSamus),
                "compiled special palette restores normal suit without pointer-table reads");
            AssertTrue(nativeColors.Colors.SequenceEqual(installedColors.Colors),
                $"normal suit CGRAM selection {selection} remains byte-exact");
            AssertEqual(nativeSamus.SpecialSuperPaletteFlags, guardedSamus.SpecialSuperPaletteFlags,
                "special palette counter parity");
        }
        Console.WriteLine("Normal suit palettes: four equipment cases match ROM and live CGRAM without pointer-table reads.");
    }

    private sealed class NormalSuitPointerReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((uint)(address - SamusPaletteRomData.Common.NormalSuitPointers) < 6)
                throw new InvalidOperationException($"Runtime read of compiled normal suit pointer ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
