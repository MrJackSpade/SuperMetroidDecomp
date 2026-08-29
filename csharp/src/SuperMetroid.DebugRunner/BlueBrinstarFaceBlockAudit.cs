using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// ROM-backed audit under construction for Blue Brinstar face block definition $A0:EA7F.
/// The first pass inventories every nearby data word so the translation can name literal
/// cartridge behavior instead of tuning animation and palette state from screenshots.
/// </summary>
internal static class BlueBrinstarFaceBlockAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        Console.WriteLine(RoomEnemySystem.ReadDefinition(bus, 0xea7f));
        for (int address = 0xa8e7ac; address < 0xa8e99a; address += 16)
        {
            ushort[] words = Enumerable.Range(0, Math.Min(8, (0xa8e99a - address) / 2))
                .Select(index => ReadWord(bus, address + index * 2))
                .ToArray();
            Console.WriteLine(
                $"${address >> 16:X2}:{address & 0xffff:X4}  " +
                string.Join(' ', words.Select(word => $"{word:X4}")));
        }
        return 0;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
