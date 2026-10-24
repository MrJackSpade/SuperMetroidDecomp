using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.ResourceAudit.PlmPrograms;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>
/// Finite metadata mirror of the actual runtime dispatch. The routing, admission
/// and importer sources are guarded independently; defining a page alone does not
/// imply it is reachable through IInstalledArtworkTransferSource.
/// </summary>
internal static class VramDmaOwnership
{
    /// <summary>Recognizes a source-address and byte-count pair only when it belongs to a reviewed native actor, PLM, room FX, or sky transfer.</summary>
    internal static bool OwnsNative(int source, int count)
    {
        if ((source == EscapeTimerTileRomData.FirstSourceAddress && count == EscapeTimerTileAtlasFormat.FirstByteCount) ||
            (source == EscapeTimerTileRomData.SecondSourceAddress && count == EscapeTimerTileAtlasFormat.SecondByteCount)) return true;
        if (GrappleTileDefinitions.Transfers.Any(frame => frame.SourceAddress == source && frame.ByteCount == count)) return true;
        if ((SnesAddressMath.BankBase(source)) == SamusRenderingRomData.Banks.CharacterData &&
            SamusArmCannonArtworkFormat.CoverTileDmaSources.IndexOf((ushort)source) >= 0 &&
            count == SamusRenderingRomData.ArmCannon.TileUploadByteCount) return true;
        if (GunshipLiftoffTransferDefinitions.Frames.Any(frame => frame.SourceAddress == source &&
                count == GunshipLiftoffTransferDefinitions.ByteCount)) return true;
        if (PlmVramArtworkAudit.OwnsInstalledTransfer(source, count)) return true;
        if (RoomFxAnimatedTileAtlasFormat.Segments.Any(frame => frame.IsFrame && frame.SourceAddress == source && frame.ByteCount == count)) return true;
        if (TourianStatueAnimatedTileMechanicsDefinitions.All.Any(definition =>
            definition.FrameByteCount == count && TourianStatueTileStripDefinitions.Pictures.Any(picture =>
                TourianStatueTileStripDefinitions.SourceAddress(definition.AnimatedObject, picture) == source))) return true;
        int offset = source - RoomSkyTilemapFormat.FirstSourceAddress;
        return offset >= 0 && offset + count <= RoomSkyTilemapFormat.TotalByteCount &&
            offset % RoomSkyTilemapFormat.PageByteCount == 0 && count == RoomSkyTilemapFormat.PageByteCount;
    }

    /// <summary>Recognizes a typed installed VRAM asset only with its reviewed transfer length, including separate ice/wave and missile trail ranges.</summary>
    internal static bool OwnsTyped(VramAssetId asset, int count)
    {
        if (asset == VramAssetId.StandardHudTiles) return count == HudTileAtlasFormat.TransferByteCount;
        if (asset == VramAssetId.EscapeTimerFirstTiles) return count == EscapeTimerTileAtlasFormat.FirstByteCount;
        if (asset == VramAssetId.EscapeTimerSecondTiles) return count == EscapeTimerTileAtlasFormat.SecondByteCount;
        if (asset is >= VramAssetId.BeamPowerTiles and <= VramAssetId.BeamPlasmaIceWaveTiles or VramAssetId.BeamChainsawTiles or VramAssetId.BeamSpacetimeTiles)
            return count == BeamTileAtlasDefinitions.ByteCount;
        if (asset == VramAssetId.ProjectileIceWaveTrailTiles) return count == ProjectileTrailAtlasDefinitions.IceWaveByteCount;
        if (asset == VramAssetId.ProjectileMissileTrailTiles) return count == ProjectileTrailAtlasDefinitions.MissileByteCount;
        if (GrappleTileDefinitions.Transfers.Any(frame => frame.Asset == asset && frame.ByteCount == count)) return true;
        if (asset is >= VramAssetId.KraidBg3RestoreQuarter0 and <= VramAssetId.KraidBg3RestoreQuarter3)
            return count == KraidBackgroundRomData.StandardBg3TransferBytes;
        return GunshipLiftoffTransferDefinitions.Frames.Any(frame => frame.Asset == asset && count == GunshipLiftoffTransferDefinitions.ByteCount);
    }
}
