using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainHealthPalette(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var colors = new HashSet<string>();
        foreach (var (health, index) in new (ushort, int)[]
            { (36000, 0), (9000, 0), (8999, 1), (5400, 1), (5399, 2), (1800, 2), (1799, 3), (0, 3) })
        {
            var cgram = new SnesCgram();
            MotherBrainHealthPalette.Apply(bus, cgram, health);
            VerifyCopy(0xade6a2, 65);
            VerifyCopy(0xade6a2, 145);
            VerifyCopy(0xade742, 177);
            colors.Add(string.Join(',', cgram.Colors.Slice(145, 15).ToArray()));
            AssertEqual((ushort)0, cgram.Colors[144], "health palette leaves transparent color untouched");
            AssertEqual((ushort)0, cgram.Colors[192], "health palette cannot overwrite Samus palette");

            void VerifyCopy(int table, int destination)
            {
                int pointer = table + index * 2;
                int source = 0xad0000 | bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
                for (int color = 0; color < 15; color++)
                    AssertEqual((ushort)(bus.ReadByte(source + color * 2) | bus.ReadByte(source + color * 2 + 1) << 8),
                        cgram.Colors[destination + color], $"health {health} palette {index} color {color}");
            }
        }
        AssertEqual(4, colors.Count, "all four native damage palettes are distinct");
        Console.WriteLine("Mother Brain health palettes: all strict thresholds, ROM copies and neighboring colors verified.");
    }
}
