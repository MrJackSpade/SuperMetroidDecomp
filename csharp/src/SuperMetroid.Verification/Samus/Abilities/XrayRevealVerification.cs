using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks the compiled X-ray reveal commands against the native bank $91 lookup tables.</summary>
internal static class XrayRevealVerification
{
    /// <summary>
    /// Compares the command of every collision type and BTS byte, including wildcard and explicit entries. The blocks each
    /// copy shows are installed pictures (#1596), which the installation verification compares with the same tables.
    /// </summary>
    internal static void VerifyXrayRevealTable()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int matches = 0;
        var commands = new HashSet<XrayRevealShape>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? expected = ReadNativeXrayRevealDefinition(
                bus, type, unchecked((byte)bts));
            XrayRevealShape? found = XrayRevealTable.Find(type, RoomBlockBehaviorCodec.Decode(bts));
            AssertEqual(expected?.Shape, found, $"retail reveal command {type}/BTS {bts:X2}");
            if (found is { } command) { matches++; commands.Add(command); }
        }
        AssertEqual(817, matches, "retail reveal lookup matches all wildcard and explicit entries");
        AssertEqual(7, commands.Count, "all seven reveal commands are decoded");
        Console.WriteLine("  X-ray reveal tables: all 4096 type/BTS commands match the cartridge; production lookup is ROM-independent.");
    }

    /// <summary>Walks the original block-type, BTS, and command-pointer chains for one block.</summary>
    internal static XrayRevealDefinition? ReadNativeXrayRevealDefinition(
        ISnesAddressSpace bus,
        RoomCollisionType type,
        byte bts)
    {
        for (int entry = XrayRevealTableWords.BlockTypeTable; ; entry += 4)
        {
            ushort key = ReadNativeXrayRevealWord(bus, entry);
            if (key == XrayRevealTableWords.End) return null;
            // A collision-kind key is a level word holding only the kind's collision bits.
            if (RoomLevelWordCodec.Decode(key).Collision != type) continue;
            for (int match = ReadNativeXrayRevealWord(bus, entry + 2); ; match += 4)
            {
                ushort value = ReadNativeXrayRevealWord(bus, match);
                if (value == XrayRevealTableWords.End) return null;
                if (value != XrayRevealTableWords.AnyBts && value != bts) continue;
                int commandPointer = ReadNativeXrayRevealWord(bus, match + 2);
                XrayRevealShape command = SamusIdentityWords.XrayRevealShapes.Decode(
                    ReadNativeXrayRevealWord(bus, commandPointer), "native X-ray reveal command");
                return command switch
                {
                    XrayRevealShape.HorizontalExtension or
                        XrayRevealShape.VerticalExtension =>
                        new(command, 0, 0, 0, 0),
                    XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar =>
                        new(command, ReadNativeXrayRevealWord(bus, commandPointer + 2), 0, 0, 0),
                    XrayRevealShape.CopyWide =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0, 0),
                    XrayRevealShape.CopyTall =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2), 0,
                            ReadNativeXrayRevealWord(bus, commandPointer + 4), 0),
                    XrayRevealShape.CopySquare =>
                        new(command,
                            ReadNativeXrayRevealWord(bus, commandPointer + 2),
                            ReadNativeXrayRevealWord(bus, commandPointer + 4),
                            ReadNativeXrayRevealWord(bus, commandPointer + 6),
                            ReadNativeXrayRevealWord(bus, commandPointer + 8)),
                    _ => throw new InvalidOperationException($"Undefined {nameof(XrayRevealShape)} {command}."),
                };
            }
        }
    }

    /// <summary>Reads a fixed-bank native table word, rejecting pointers outside bank $91's mapped range.</summary>
    private static ushort ReadNativeXrayRevealWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer is < 0x8000 or >= ushort.MaxValue)
            throw new InvalidDataException(
                $"Native X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(XrayRevealTableWords.Bank | pointer);
    }
}
