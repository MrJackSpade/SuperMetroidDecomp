using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks the shape X-ray reveals for every block against the native bank $91 lookup tables.</summary>
internal static class XrayRevealVerification
{
    /// <summary>
    /// Compares the reveal shape of every collision type and BTS byte, including wildcard and explicit entries. The blocks each
    /// copy shows are installed pictures (#1596), which the installation verification compares with the same tables.
    /// </summary>
    internal static void VerifyXrayRevealTable()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int matches = 0;
        var shapes = new HashSet<XrayRevealShape>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? expected = ReadNativeXrayRevealDefinition(
                bus, type, unchecked((byte)bts));
            XrayRevealShape? found = XrayRevealTable.Find(type, RoomBlockBehaviorCodec.Decode(bts));
            AssertEqual(expected?.Shape, found, $"retail reveal shape {type}/BTS {bts:X2}");
            if (found is { } shape) { matches++; shapes.Add(shape); }
        }
        AssertEqual(817, matches, "retail reveal lookup matches all wildcard and explicit entries");
        AssertEqual(7, shapes.Count, "all seven reveal shapes are decoded");
        Console.WriteLine("  X-ray reveal tables: all 4096 type/BTS shapes match the cartridge; production lookup is ROM-independent.");
    }

    /// <summary>Walks the original block-type, BTS, and shape-pointer chains for one block.</summary>
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
                int shapePointer = ReadNativeXrayRevealWord(bus, match + 2);
                XrayRevealShape shape = SamusIdentityWords.XrayRevealShapes.Decode(
                    ReadNativeXrayRevealWord(bus, shapePointer), "native X-ray reveal shape");
                return shape switch
                {
                    XrayRevealShape.HorizontalExtension or
                        XrayRevealShape.VerticalExtension =>
                        new(shape, 0, 0, 0, 0),
                    XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar =>
                        new(shape, ReadNativeXrayRevealWord(bus, shapePointer + 2), 0, 0, 0),
                    XrayRevealShape.CopyWide =>
                        new(shape,
                            ReadNativeXrayRevealWord(bus, shapePointer + 2),
                            ReadNativeXrayRevealWord(bus, shapePointer + 4), 0, 0),
                    XrayRevealShape.CopyTall =>
                        new(shape,
                            ReadNativeXrayRevealWord(bus, shapePointer + 2), 0,
                            ReadNativeXrayRevealWord(bus, shapePointer + 4), 0),
                    XrayRevealShape.CopySquare =>
                        new(shape,
                            ReadNativeXrayRevealWord(bus, shapePointer + 2),
                            ReadNativeXrayRevealWord(bus, shapePointer + 4),
                            ReadNativeXrayRevealWord(bus, shapePointer + 6),
                            ReadNativeXrayRevealWord(bus, shapePointer + 8)),
                    _ => throw new InvalidOperationException($"Undefined {nameof(XrayRevealShape)} {shape}."),
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
