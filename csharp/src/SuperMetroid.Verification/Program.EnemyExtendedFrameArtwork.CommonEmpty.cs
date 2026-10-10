using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies shared empty ordinary and extended enemy frames across supported banks, including installed-artwork ROM isolation and point-hitbox callback selection.</summary>
    /// <param name="rom">Retail address space used as the byte-level reference for each bank-local shared definition.</param>
    /// <param name="stock">Installed enemy artwork catalog used to confirm the extended empty frame can draw without reading its art from the cartridge.</param>
    private static void VerifySharedEmptyExtendedFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var blockedRom = new FrontendCartridgeReadGuard(rom);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            int emptyOam = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            AssertEqual((ushort)0, ReadWord(emptyOam),
                $"bank ${bank:X2} shared ordinary empty OAM part count");
            // Exercise the ordinary draw entry point too: unlike the extended
            // $804F root below, enemies can select $804D directly during their
            // initialization. A full-ROM read guard proves this is compiled.
            var ordinaryEnemies = new RoomEnemySystem { TileArtwork = stock };
            typeof(RoomEnemySystem).GetField("_bus",
                BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(
                    ordinaryEnemies, new FrontendCartridgeReadGuard(rom));
            var emptyDraw = new OamBuffer();
            typeof(RoomEnemySystem).GetMethod("DrawEnemySpritemap",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(
                    ordinaryEnemies,
                    [emptyDraw, bank, CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap,
                        (ushort)0x0080, (ushort)0x0080, (ushort)0, (ushort)0,
                        false, true]);
            AssertEqual(0, emptyDraw.NextByteOffset,
                $"bank ${bank:X2} ordinary empty frame emits no OAM entries");
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
            OamBuffer native = DrawReferenceExtendedFrame(rom, bank,
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
        AssertTrue(!CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(
                0xb4, CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap),
            "non-enemy bank does not inherit the shared empty OAM definition");
        Console.WriteLine("  Shared empty frames: 12 bank-local ordinary and extended records match the ROM; ordinary draw and extended draw/collision forbid cartridge reads.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    /// <summary>Finds the callback selected by the shared point hitbox when an enemy query overlaps its origin.</summary>
    /// <param name="bus">Address space supplying the compiled enemy frame and hitbox definitions.</param>
    /// <param name="bank">Enemy bank whose shared empty frame is installed on the query fixture.</param>
    /// <param name="selectShot"><see langword="true"/> selects the hitbox's shot callback; <see langword="false"/> selects its touch callback.</param>
    /// <returns>The callback address selected by the enemy hitbox query.</returns>
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
