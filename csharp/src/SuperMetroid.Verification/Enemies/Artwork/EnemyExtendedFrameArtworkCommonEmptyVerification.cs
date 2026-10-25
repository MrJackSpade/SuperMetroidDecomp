using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkVerification;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

/// <summary>Checks bank-local shared empty enemy frames across ordinary draw, extended draw, and collision.</summary>
internal static class EnemyExtendedFrameArtworkCommonEmptyVerification
{
    /// <summary>Compares the 12 native empty-frame records and blocks cartridge reads during installed draw and callback selection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifySharedEmptyExtendedFrames(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        var blockedRom = NewFrontendCartridgeReadGuard(rom);
        foreach (byte bank in CommonEnemyEmptyExtendedFrameDefinitionsTooling.SupportedBanks)
        {
            int emptyOam = ((int)new SnesAddress(bank, CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap));
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord(emptyOam),
                $"bank ${bank:X2} shared ordinary empty OAM part count");
            // Exercise the ordinary draw entry point too: unlike the extended
            // $804F root below, enemies can select $804D directly during their
            // initialization. A full-ROM read guard proves this is compiled.
            var ordinaryEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas };
            var emptyDraw = new OamBuffer();
            ordinaryEnemies.DrawEnemySpritemap(emptyDraw, new NativeEnemySpritemapDraw { Bank = bank, Pointer = CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap, Origin = new Position16<SpritePixels>(0x0080, 0x0080), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            AssertEqual(0, emptyDraw.NextByteOffset,
                $"bank ${bank:X2} ordinary empty frame emits no OAM entries");
            int root = ((int)new SnesAddress(bank, CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame));
            AssertEqual((byte)1, rom.ReadCartridgeByte(root),
                $"bank ${bank:X2} shared empty extended component count");
            AssertEqual((byte)0, rom.ReadCartridgeByte(root + 1),
                $"bank ${bank:X2} shared empty extended header high byte");
            for (int offset = 2; offset < 6; offset++)
                AssertEqual((byte)0, rom.ReadCartridgeByte(root + offset),
                    $"bank ${bank:X2} shared empty component offset byte {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 6)),
                $"bank ${bank:X2} shared empty OAM pointer");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 8)),
                $"bank ${bank:X2} shared empty hitbox pointer");
            int hitbox = ((int)new SnesAddress(bank, CommonEnemyEmptyExtendedFrameDefinitions.PointHitboxList));
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord(hitbox),
                $"bank ${bank:X2} shared point-hitbox count");
            for (int offset = 2; offset < 10; offset += 2)
                AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + offset)),
                    $"bank ${bank:X2} shared point bound {offset}");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitionsConstants.Callback(false),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 10)),
                $"bank ${bank:X2} shared touch callback");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitionsConstants.Callback(true),
                CartridgeImportSource.Require(rom).ReadCartridgeWord((hitbox + 12)),
                $"bank ${bank:X2} shared shot callback");

            var visualGuard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> visualGuardBlockFrame);
            visualGuardBlockFrame(bank,
                CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame);
            OamBuffer native = DrawReferenceExtendedFrame(rom, bank,
                CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame, new(0x0080, 0x0080));
            OamBuffer installed = DrawExtendedForBank(stock, bank,
                CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame, new(0x0080, 0x0080));
            AssertTrue(native.ImageEquals(installed) &&
                native.NextByteOffset == installed.NextByteOffset,
                $"bank ${bank:X2} shared empty frame draws without ROM art reads");
            // Bank $A0 holds the shared records but no enemy, so no slot selects its frames.
            if (!RoomEnemyDefinitionNative.HoldsEnemy(bank))
                continue;
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitionsConstants.Callback(false),
                FindSharedEmptyCallback(bank, selectShot: false),
                $"bank ${bank:X2} shared point selects compiled touch callback");
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitionsConstants.Callback(true),
                FindSharedEmptyCallback(bank, selectShot: true),
                $"bank ${bank:X2} shared point selects compiled shot callback");
        }
        AssertTrue(!CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(
                0xb4, CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap),
            "non-enemy bank does not inherit the shared empty OAM definition");
        Console.WriteLine("  Shared empty frames: 12 bank-local ordinary and extended records match the ROM; ordinary draw and extended draw/collision forbid cartridge reads.");
    }

    /// <summary>Runs the extended hitbox walker at the point origin to select its touch or shot callback.</summary>
    private static ushort FindSharedEmptyCallback(
        byte bank, bool selectShot)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        // The frame pointer names a frame of the bank the slot's enemy lives in.
        slot.Bind(RoomEnemyDefinitionNative.FirstInBank(bank), default(RoomEnemyDefinition));
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        object?[] arguments =
            [slot, new Position16<RoomPixels>(0x0080, 0x0080), new Extent16<RoomPixels>(2, 2), selectShot, null];
        bool found = ExtendedHitboxWalker.Find(enemies, arguments);
        AssertTrue(found, "shared empty extended point overlaps its origin");
        return (ushort)(arguments[4] ?? throw new InvalidOperationException("Verification requires arguments[4]."));
    }
}
