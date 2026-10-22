using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks installed Ridley wing and tail sprites against native OAM without runtime ROM reads.</summary>
internal static class RidleySupplementalArtworkVerification
{
    /// <summary>Compares all wing phases and tail directions with native selectors and rendered OAM.</summary>
    internal static void VerifyInstalledRidleySupplementalArtwork(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        AssertEqual(20, RidleySupplementalVisualDefinitionsConstants.WingPointerCount,
            "Ridley wing table retains twenty animation selections");
        AssertEqual(16, RidleySupplementalVisualDefinitionsConstants.TailTipPointerCount,
            "Ridley tail tip retains sixteen direction selections");
        for (int index = 0; index < RidleySupplementalVisualDefinitionsConstants.WingPointerCount;
             index++)
        {
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ceres.WingSpritemapPointers + index * 2));
            AssertEqual(native, RidleySupplementalVisualDefinitionsConstants.WingFrameAt(index),
                $"Ridley wing animation table entry {index} matches the pinned ROM");

            RidleyFacing facing = index < 10 ? RidleyFacing.Left : RidleyFacing.Right;
            ushort wingFrame = (ushort)(index % 10);
            OamBuffer actual = DrawInstalledRidleySupplement(
                stock, facing, wingFrame, tailDirection: null);
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = RidleySupplementalVisualDefinitionsConstants.Bank, Pointer = native, Origin = new(200, 100), PaletteBits = 0x0200, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = true });
            AssertRidleySupplementOam(expected, actual,
                $"Ridley wing selection {index} uses installed OAM without table or frame reads");
        }

        for (int direction = 0;
             direction < RidleySupplementalVisualDefinitionsConstants.TailTipPointerCount;
             direction++)
        {
            ushort nativeTip = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointers.Ceres.TailTipSpritemapPointers + direction * 2));
            AssertEqual(nativeTip, RidleySupplementalVisualDefinitionsConstants.TailTipFrameAt(direction),
                $"Ridley tail-tip direction {direction} matches the pinned ROM");
            OamBuffer actual = DrawInstalledRidleySupplement(
                stock, facing: RidleyFacing.Turning, wingFrame: 0, tailDirection: direction);
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = RidleySupplementalVisualDefinitionsConstants.Bank, Pointer = nativeTip, Origin = new(140, 138), PaletteBits = 0x0200, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = true });
            for (int segment = 5; segment >= 0; segment--)
                DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = RidleySupplementalVisualDefinitionsConstants.Bank, Pointer = RidleySupplementalVisualDefinitionsConstants.SegmentFrameAt(segment), Origin = new(unchecked((ushort)(80 + segment * 10)), unchecked((ushort)(120 + segment * 3))), PaletteBits = 0x0200, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = true });
            AssertRidleySupplementOam(expected, actual,
                $"Ridley tail direction {direction} retains tip-first reverse-segment OAM");
        }

        AssertThrows<InvalidDataException>(
            () => RidleySupplementalVisualDefinitionsConstants.WingFrameAt(20),
            "Ridley rejects adjacent wing-table code as a pose");
        AssertThrows<InvalidDataException>(
            () => RidleySupplementalVisualDefinitionsConstants.TailTipFrameAt(16),
            "Ridley rejects adjacent tail-tip OAM as a pointer-table entry");
    }

    /// <summary>Constructs a Ridley state and draws its installed wing or articulated-tail supplement.</summary>
    internal static OamBuffer DrawInstalledRidleySupplement(
        InstalledEnemyArtwork artwork, RidleyFacing facing, ushort wingFrame, int? tailDirection)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        slot.Position = slot.Position.WithWhole(new(200, 100));
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        var state = new RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = facing,
            WingFrame = wingFrame,
            SpritemapPaletteIndex = SnesObjPalettes.Palette(1),
            TailSegments = tailDirection is null ? [] :
                [.. Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                {
                    Position = new Position16<RoomPixels>(
                        unchecked((ushort)(80 + index * 10)),
                        unchecked((ushort)(120 + index * 3))),
                    Angle = index == 6 ? unchecked((ushort)(tailDirection.Value << 4)) : (ushort)0,
                })],
        };
        enemies.RidleyEncounter.Ridley = state;
        var oam = new OamBuffer();
        oam.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawRidleySupplementalSprites(
            frame, slot, Position16<RoomPixels>.Zero));
        return oam;
    }

    /// <summary>Checks exact OAM image content and byte count for a supplemental draw.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void AssertRidleySupplementOam(
        OamBuffer expected, OamBuffer actual, string description) =>
        AssertTrue(expected.ImageEquals(actual) &&
                   expected.NextByteOffset == actual.NextByteOffset, description);

}
