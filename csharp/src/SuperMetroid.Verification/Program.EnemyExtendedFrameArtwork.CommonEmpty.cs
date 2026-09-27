using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySharedEmptyExtendedFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var blockedRom = new FrontendCartridgeReadGuard(rom);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitions.SupportedBanks)
        {
            int root = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.Frame;
            AssertEqual((byte)1, rom.ReadByte(root),
                $"bank ${bank:X2} shared empty extended component count");
            AssertEqual((byte)0, rom.ReadByte(root + 1),
                $"bank ${bank:X2} shared empty extended header high byte");
            for (int offset = 2; offset < 6; offset++)
                AssertEqual((byte)0, rom.ReadByte(root + offset),
                    $"bank ${bank:X2} shared empty component offset byte {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap,
                ReadWord(root + 6),
                $"bank ${bank:X2} shared empty OAM pointer");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList,
                ReadWord(root + 8),
                $"bank ${bank:X2} shared empty hitbox pointer");
            int hitbox = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList;
            AssertEqual((ushort)1, ReadWord(hitbox),
                $"bank ${bank:X2} shared point-hitbox count");
            for (int offset = 2; offset < 10; offset += 2)
                AssertEqual((ushort)0, ReadWord(hitbox + offset),
                    $"bank ${bank:X2} shared point bound {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(false),
                ReadWord(hitbox + 10),
                $"bank ${bank:X2} shared touch callback");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(true),
                ReadWord(hitbox + 12),
                $"bank ${bank:X2} shared shot callback");

            var visualGuard = new ExtendedVisualReadGuard(rom);
            visualGuard.BlockFrame(bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame);
            OamBuffer native = DrawExtendedForBank(null, rom, bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame, 0x0080, 0x0080);
            OamBuffer installed = DrawExtendedForBank(stock, visualGuard, bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame, 0x0080, 0x0080);
            AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                native.HighTable.SequenceEqual(installed.HighTable) &&
                native.NextByteOffset == installed.NextByteOffset,
                $"bank ${bank:X2} shared empty frame draws without ROM art reads");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(false),
                FindSharedEmptyCallback(blockedRom, bank, selectShot: false),
                $"bank ${bank:X2} shared point selects compiled touch callback");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(true),
                FindSharedEmptyCallback(blockedRom, bank, selectShot: true),
                $"bank ${bank:X2} shared point selects compiled shot callback");
        }
        Console.WriteLine("  Shared empty extended frame: 12 bank-local records match the ROM and draw/collide with cartridge reads forbidden.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private static ushort FindSharedEmptyCallback(
        ISnesAddressSpace bus, byte bank, bool selectShot)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Definition = default(RoomEnemyDefinition) with { Bank = bank };
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.Frame;
        slot.XPosition = 0x0080;
        slot.YPosition = 0x0080;
        object?[] arguments =
            [slot, (ushort)0x0080, (ushort)0x0080,
                (ushort)2, (ushort)2, selectShot, null];
        bool found = (bool)typeof(RoomEnemySystem)
            .GetMethod("TryFindExtendedHitboxCallback", flags)!
            .Invoke(enemies, arguments)!;
        AssertTrue(found, "shared empty extended point overlaps its origin");
        return (ushort)arguments[6]!;
    }
}
