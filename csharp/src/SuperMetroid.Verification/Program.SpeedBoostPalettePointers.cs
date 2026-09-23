using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpeedBoostPalettePointers()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guarded = new SpeedBoostPointerReadGuard(rom);
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

            var nativeSamus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 1,
            };
            var guardedSamus = new SamusState
            {
                EquippedItems = equipment[selection],
                SpecialSuperPaletteFlags = 1,
            };
            var nativeColors = new SnesCgram();
            var installedColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(rom, nativeColors, nativeSamus),
                "native special palette loads Speed Booster shade");
            AssertTrue(SamusSpecialSuperPalette.Update(guarded, installedColors, guardedSamus),
                "compiled special palette loads Speed Booster shade without pointer-table reads");
            AssertTrue(nativeColors.Colors.SequenceEqual(installedColors.Colors),
                $"Speed Booster CGRAM selection {selection} remains byte-exact");
            AssertEqual(nativeSamus.SpecialSuperPaletteFlags, guardedSamus.SpecialSuperPaletteFlags,
                "special palette counter parity");
        }
        Console.WriteLine("Speed Booster palettes: four equipment cases match ROM and live CGRAM without pointer-table reads.");
    }

    private sealed class SpeedBoostPointerReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((uint)(address - SamusPaletteRomData.FullBodyCycles.SpeedBoostPointers) < 6)
                throw new InvalidOperationException($"Runtime read of compiled Speed Booster pointer ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
