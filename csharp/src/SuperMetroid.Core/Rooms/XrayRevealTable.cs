using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Owned native reveal command and its row-major metatile arguments; extensions have no arguments.</summary>
public readonly record struct XrayRevealDefinition(ushort Command, ushort TopLeft,
    ushort TopRight, ushort BottomLeft, ushort BottomRight);

/// <summary>Ports the two-stage block-type/BTS lookup at $91:CDD6 without assigning invented reveal graphics.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(ISnesAddressSpace bus, RoomCollisionType type, byte bts)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int typeWord = (int)type << 12;
        for (int entry = XrayRevealCodePointers.BlockTypeTable; ; entry += 4)
        {
            ushort key = ReadWord(bus, entry);
            if (key == XrayRevealCodePointers.End) return null;
            if (key != typeWord) continue;
            for (int match = ReadWord(bus, entry + 2); ; match += 4)
            {
                ushort value = ReadWord(bus, match);
                if (value == XrayRevealCodePointers.End) return null;
                if (value != XrayRevealCodePointers.AnyBts && value != bts) continue;
                int commandPointer = ReadWord(bus, match + 2);
                ushort command = ReadWord(bus, commandPointer);
                return command switch
                {
                    XrayRevealCodePointers.HorizontalExtension or XrayRevealCodePointers.VerticalExtension =>
                        new(command, 0, 0, 0, 0),
                    XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyBrinstar =>
                        new(command, ReadWord(bus, commandPointer + 2), 0, 0, 0),
                    XrayRevealCodePointers.CopyWide =>
                        new(command, ReadWord(bus, commandPointer + 2), ReadWord(bus, commandPointer + 4), 0, 0),
                    // $91:CF67 increments Y twice before copying the lower metatile.
                    // It is a distinct operand, not a repeated copy of the upper one.
                    XrayRevealCodePointers.CopyTall =>
                        new(command, ReadWord(bus, commandPointer + 2), 0, ReadWord(bus, commandPointer + 4), 0),
                    XrayRevealCodePointers.CopySquare =>
                        new(command, ReadWord(bus, commandPointer + 2), ReadWord(bus, commandPointer + 4),
                            ReadWord(bus, commandPointer + 6), ReadWord(bus, commandPointer + 8)),
                    _ => throw new InvalidDataException($"Untranslated X-ray reveal command $91:{command:X4} at ${commandPointer:X4}."),
                };
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer < 0x8000 || pointer >= ushort.MaxValue)
            throw new InvalidDataException($"X-ray reveal table crossed its ROM bank at ${pointer:X}.");
        return (ushort)(bus.ReadByte(XrayRevealCodePointers.Bank | pointer) |
            bus.ReadByte(XrayRevealCodePointers.Bank | (pointer + 1)) << 8);
    }
}
