using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyXrayRevealTable()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int matches = 0;
        var commands = new HashSet<ushort>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            // Independent inventory transcribed from $91:D2D6-D4DE. All other
            // combinations retain their BG1 art, rather than fabricating a symbol.
            bool expected = type switch
            {
                RoomCollisionType.Air or RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension => true,
                RoomCollisionType.SpecialAir => bts == 0x46,
                RoomCollisionType.SpikeBlock => bts == 0x0e,
                RoomCollisionType.SpecialBlock => bts < 16 || bts is >= 0x82 and <= 0x85,
                RoomCollisionType.ShootableBlock => bts < 16,
                RoomCollisionType.GrappleBlock => bts < 3,
                RoomCollisionType.BombableBlock => bts < 8,
                _ => false,
            };
            XrayRevealDefinition? found = XrayRevealTable.Find(bus, type, (byte)bts);
            AssertEqual(expected, found.HasValue, $"retail reveal admission {type}/BTS {bts:X2}");
            if (found is { } command) { matches++; commands.Add(command.Command); }
        }
        AssertEqual(817, matches, "retail reveal lookup matches all wildcard and explicit entries");
        AssertEqual(7, commands.Count, "all seven reveal commands are decoded");
        AssertEqual(new XrayRevealDefinition(XrayRevealCodePointers.CopyTall, 0x98, 0, 0xb8, 0),
            XrayRevealTable.Find(bus, RoomCollisionType.ShootableBlock, 2)!.Value,
            "tall shot-block reveal advances to a distinct lower operand at $91:CF67");
        AssertEqual(new XrayRevealDefinition(XrayRevealCodePointers.CopySquare, 0x99, 0x9a, 0xb9, 0xba),
            XrayRevealTable.Find(bus, RoomCollisionType.ShootableBlock, 3)!.Value,
            "square shot-block reveal preserves all four row-major metatiles");
        Console.WriteLine("  X-ray reveal tables: 4096 type/BTS combinations, 817 matches, all seven commands and distinct multi-block art agree.");
    }
}
