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
            XrayRevealDefinition? expected = ReadNativeXrayRevealDefinition(
                bus, type, unchecked((byte)bts));
            XrayRevealDefinition? found = XrayRevealTable.Find(type, unchecked((byte)bts));
            AssertEqual(expected, found, $"retail reveal definition {type}/BTS {bts:X2}");
            if (found is { } command) { matches++; commands.Add(command.Command); }
        }
        AssertEqual(817, matches, "retail reveal lookup matches all wildcard and explicit entries");
        AssertEqual(7, commands.Count, "all seven reveal commands are decoded");
        AssertEqual(new XrayRevealDefinition(XrayRevealCodePointers.CopyTall, 0x98, 0, 0xb8, 0),
            XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 2)!.Value,
            "tall shot-block reveal advances to a distinct lower operand at $91:CF67");
        AssertEqual(new XrayRevealDefinition(XrayRevealCodePointers.CopySquare, 0x99, 0x9a, 0xb9, 0xba),
            XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 3)!.Value,
            "square shot-block reveal preserves all four row-major metatiles");
        Console.WriteLine("  X-ray reveal tables: all 4096 type/BTS definitions and operands match the cartridge; production lookup is ROM-independent.");
    }

    private static XrayRevealDefinition? ReadNativeXrayRevealDefinition(
        ISnesAddressSpace bus,
        RoomCollisionType type,
        byte bts)
    {
        int typeWord = (int)type << 12;
        for (int entry = XrayRevealCodePointers.BlockTypeTable; ; entry += 4)
        {
            ushort key = ReadNativeXrayRevealWord(bus, entry);
            if (key == XrayRevealCodePointers.End) return null;
            if (key != typeWord) continue;
            for (int match = ReadNativeXrayRevealWord(bus, entry + 2); ; match += 4)
            {
                ushort value = ReadNativeXrayRevealWord(bus, match);
                if (value == XrayRevealCodePointers.End) return null;
                if (value != XrayRevealCodePointers.AnyBts && value != bts) continue;
                int commandPointer = ReadNativeXrayRevealWord(bus, match + 2);
                ushort command = ReadNativeXrayRevealWord(bus, commandPointer);
                return command switch
                {
                    XrayRevealCodePointers.HorizontalExtension or
                        XrayRevealCodePointers.VerticalExtension =>
                        new(command, 0, 0, 0, 0),
                    XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyBrinstar =>
                        new(command, ReadNativeXrayRevealWord(bus, commandPointer + 2), 0, 0, 0),
                    XrayRevealCodePointers.CopyWide =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0, 0),
                    XrayRevealCodePointers.CopyTall =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2), 0,
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0),
                    XrayRevealCodePointers.CopySquare =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4),
                            ReadNativeXrayRevealWord(bus, commandPointer + 6),
                            ReadNativeXrayRevealWord(bus, commandPointer + 8)),
                    _ => throw new InvalidDataException(
                        $"Native X-ray reveal command $91:{command:X4} at ${commandPointer:X4} is unknown."),
                };
            }
        }
    }

    private static ushort ReadNativeXrayRevealWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer < 0x8000 || pointer >= ushort.MaxValue)
            throw new InvalidDataException(
                $"Native X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return unchecked((ushort)(
            bus.ReadByte(XrayRevealCodePointers.Bank | pointer) |
            bus.ReadByte(XrayRevealCodePointers.Bank | (pointer + 1)) << 8));
    }
}
