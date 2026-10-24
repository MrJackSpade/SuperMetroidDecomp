using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks compiled X-ray block reveals against the native bank $91 lookup tables.</summary>
internal static class XrayRevealVerification
{
    /// <summary>Compares all collision types and BTS bytes, including wildcard and explicit entries.</summary>
    internal static void VerifyXrayRevealTable()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int matches = 0;
        var commands = new HashSet<XrayRevealCommand>();
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
        AssertEqual(new XrayRevealDefinition(XrayRevealCommand.CopyTall, 0x98, 0, 0xb8, 0),
            (XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 2) ?? throw new InvalidOperationException("Verification requires XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 2).")),
            "tall shot-block reveal advances to a distinct lower operand at $91:CF67");
        AssertEqual(new XrayRevealDefinition(XrayRevealCommand.CopySquare, 0x99, 0x9a, 0xb9, 0xba),
            (XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 3) ?? throw new InvalidOperationException("Verification requires XrayRevealTable.Find(RoomCollisionType.ShootableBlock, 3).")),
            "square shot-block reveal preserves all four row-major metatiles");
        Console.WriteLine("  X-ray reveal tables: all 4096 type/BTS definitions and operands match the cartridge; production lookup is ROM-independent.");
    }

    /// <summary>Walks the original block-type, BTS, and command-pointer chains for one block.</summary>
    private static XrayRevealDefinition? ReadNativeXrayRevealDefinition(
        ISnesAddressSpace bus,
        RoomCollisionType type,
        byte bts)
    {
        int typeWord = (int)type << 12;
        for (int entry = XrayRevealTableWords.BlockTypeTable; ; entry += 4)
        {
            ushort key = ReadNativeXrayRevealWord(bus, entry);
            if (key == XrayRevealTableWords.End) return null;
            if (key != typeWord) continue;
            for (int match = ReadNativeXrayRevealWord(bus, entry + 2); ; match += 4)
            {
                ushort value = ReadNativeXrayRevealWord(bus, match);
                if (value == XrayRevealTableWords.End) return null;
                if (value != XrayRevealTableWords.AnyBts && value != bts) continue;
                int commandPointer = ReadNativeXrayRevealWord(bus, match + 2);
                XrayRevealCommand command = SamusIdentityWords.XrayRevealCommands.Decode(
                    ReadNativeXrayRevealWord(bus, commandPointer), "native X-ray reveal command");
                return command switch
                {
                    XrayRevealCommand.HorizontalExtension or
                        XrayRevealCommand.VerticalExtension =>
                        new(command, 0, 0, 0, 0),
                    XrayRevealCommand.CopyOne or XrayRevealCommand.CopyBrinstar =>
                        new(command, ReadNativeXrayRevealWord(bus, commandPointer + 2), 0, 0, 0),
                    XrayRevealCommand.CopyWide =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0, 0),
                    XrayRevealCommand.CopyTall =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2), 0,
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0),
                    XrayRevealCommand.CopySquare =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4),
                            ReadNativeXrayRevealWord(bus, commandPointer + 6),
                            ReadNativeXrayRevealWord(bus, commandPointer + 8)),
                    _ => throw new InvalidOperationException($"Undefined {nameof(XrayRevealCommand)} {command}."),
                };
            }
        }
    }

    /// <summary>Reads a fixed-bank native table word, rejecting pointers outside bank $91's mapped range.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for native X-ray reveal words")]
    private static ushort ReadNativeXrayRevealWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer is < 0x8000 or >= ushort.MaxValue)
            throw new InvalidDataException(
                $"Native X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(XrayRevealTableWords.Bank | pointer);
    }
}
