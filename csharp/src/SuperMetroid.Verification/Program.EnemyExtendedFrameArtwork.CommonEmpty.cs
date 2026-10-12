using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for shared empty extended frames")]
    private static void VerifySharedEmptyExtendedFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        var blockedRom = NewFrontendCartridgeReadGuard(rom);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            int emptyOam = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord(emptyOam),
                $"bank ${bank:X2} shared ordinary empty OAM part count");
            // Exercise the ordinary draw entry point too: unlike the extended
            // $804F root below, enemies can select $804D directly during their
            // initialization. A full-ROM read guard proves this is compiled.
            var ordinaryEnemies = new RoomEnemySystem { TileArtwork = stock };
            typeof(RoomEnemySystem).GetField("_bus",
                BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(
                    ordinaryEnemies, NewFrontendCartridgeReadGuard(rom));
            var emptyDraw = new OamBuffer();
            typeof(RoomEnemySystem).GetMethod("DrawEnemySpritemap",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(
                    ordinaryEnemies,
                    [emptyDraw, new EnemySpritemapDraw { Bank = (byte)bank, Pointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap, Origin = new Position16<OamPixels>((ushort)0x0080, (ushort)0x0080), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = false, OriginYIsOnScreen = true }]);
            AssertEqual(0, emptyDraw.NextByteOffset,
                $"bank ${bank:X2} ordinary empty frame emits no OAM entries");
            int root = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.Frame;
            AssertEqual((byte)1, rom.ReadCartridgeByte(root),
                $"bank ${bank:X2} shared empty extended component count");
            AssertEqual((byte)0, rom.ReadCartridgeByte(root + 1),
                $"bank ${bank:X2} shared empty extended header high byte");
            for (int offset = 2; offset < 6; offset++)
                AssertEqual((byte)0, rom.ReadCartridgeByte(root + offset),
                    $"bank ${bank:X2} shared empty component offset byte {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 6)),
                $"bank ${bank:X2} shared empty OAM pointer");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 8)),
                $"bank ${bank:X2} shared empty hitbox pointer");
            int hitbox = (bank << 16) |
                CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList;
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord(hitbox),
                $"bank ${bank:X2} shared point-hitbox count");
            for (int offset = 2; offset < 10; offset += 2)
                AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + offset)),
                    $"bank ${bank:X2} shared point bound {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(false),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 10)),
                $"bank ${bank:X2} shared touch callback");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.Callback(true),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 12)),
                $"bank ${bank:X2} shared shot callback");

            var visualGuard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> visualGuardBlockFrame);
            visualGuardBlockFrame(bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame);
            OamBuffer native = DrawReferenceExtendedFrame(rom, bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame, new(0x0080, 0x0080));
            OamBuffer installed = DrawExtendedForBank(stock, visualGuard, bank,
                CommonEnemyEmptyExtendedFrameDefinitions.Frame, new(0x0080, 0x0080));
            AssertTrue(native.ImageEquals(installed) &&
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
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        object?[] arguments =
            [slot, new Position16<RoomPixels>((ushort)0x0080, (ushort)0x0080), new Extent16<RoomPixels>((ushort)2, (ushort)2), selectShot, null];
        bool found = (bool)typeof(RoomEnemySystem)
            .GetMethod("TryFindExtendedHitboxCallback", flags)!
            .Invoke(enemies, arguments)!;
        AssertTrue(found, "shared empty extended point overlaps its origin");
        return (ushort)arguments[4]!;
    }
}
