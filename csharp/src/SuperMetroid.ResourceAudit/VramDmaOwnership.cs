using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Finite metadata mirror of the actual runtime dispatch. The routing, admission
/// and importer sources are guarded independently; defining a page alone does not
/// imply it is reachable through IInstalledArtworkTransferSource.
/// </summary>
internal static class VramDmaOwnership
{
    /// <summary>Checks whether a native source address and byte count belong to an installed transfer.</summary>
    internal static bool OwnsNative(int source, int count)
    {
        if (source == HudTileAtlasFormat.SourceAddress && count == HudTileAtlasFormat.TransferByteCount) return true;
        if ((source == EscapeTimerTileRomData.FirstSourceAddress && count == EscapeTimerTileAtlasFormat.FirstByteCount) ||
            (source == EscapeTimerTileRomData.SecondSourceAddress && count == EscapeTimerTileAtlasFormat.SecondByteCount)) return true;
        if (GrappleTileDefinitions.Transfers.ToArray().Any(frame => frame.SourceAddress == source && frame.ByteCount == count)) return true;
        if (source == GameplayHudDefinitions.TopRowAddress && count == GameplayHudDefinitions.TopRowByteCount) return true;
        if (source == StandardObjectArtworkAddresses.Source && count == StandardObjectArtworkFormat.TransferByteCount) return true;
        if (SamusSpecialSequenceRomData.Death.TileSegments.ToArray().Any(segment => segment.SourceAddress == source &&
                count == SamusSpecialSequenceRomData.Death.TileSegmentByteCount)) return true;
        if ((source & 0xff0000) == SamusRenderingRomData.Banks.CharacterData &&
            SamusArmCannonArtworkFormat.TileSourcePointers.Contains((ushort)source) &&
            count == SamusRenderingRomData.ArmCannon.TileUploadByteCount) return true;
        if (GunshipLiftoffTransferDefinitions.Frames.ToArray().Any(frame => frame.SourceAddress == source &&
                count == GunshipLiftoffTransferDefinitions.ByteCount)) return true;
        if (PlmVramArtworkAudit.OwnsInstalledTransfer(source, count)) return true;
        if (RoomFxAnimatedTileAtlasFormat.Segments.Any(frame => frame.IsFrame && frame.SourceAddress == source && frame.ByteCount == count)) return true;
        if (TourianStatueAnimatedTileMechanicsDefinitions.All.Any(definition =>
            definition.TransferByteCount == count && definition.SourceOperandPointers.Any(operand =>
                TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition, operand) == source))) return true;
        int offset = source - RoomSkyTilemapFormat.FirstSourceAddress;
        return offset >= 0 && offset + count <= RoomSkyTilemapFormat.TotalByteCount &&
            ((offset % RoomSkyTilemapFormat.PageByteCount == 0 && count == RoomSkyTilemapFormat.PageByteCount) ||
             ((source & 1) == 0 && count == RoomFxRomData.ScrollingSky.TilemapRowByteCount));
    }

    /// <summary>Checks whether a typed VRAM asset and byte count match an admitted transfer.</summary>
    internal static bool OwnsTyped(VramAssetId asset, int count)
    {
        if (asset == VramAssetId.StandardHudTiles) return count == HudTileAtlasFormat.TransferByteCount;
        if (asset == VramAssetId.EscapeTimerFirstTiles) return count == EscapeTimerTileAtlasFormat.FirstByteCount;
        if (asset == VramAssetId.EscapeTimerSecondTiles) return count == EscapeTimerTileAtlasFormat.SecondByteCount;
        if (asset is >= VramAssetId.BeamPowerTiles and <= VramAssetId.BeamPlasmaIceWaveTiles or VramAssetId.BeamChainsawTiles or VramAssetId.BeamSpacetimeTiles)
            return count == BeamTileAtlasDefinitions.ByteCount;
        if (asset == VramAssetId.ProjectileIceWaveTrailTiles) return count == ProjectileTrailAtlasDefinitions.IceWaveByteCount;
        if (asset == VramAssetId.ProjectileMissileTrailTiles) return count == ProjectileTrailAtlasDefinitions.MissileByteCount;
        if (GrappleTileDefinitions.Transfers.ToArray().Any(frame => frame.Asset == asset && frame.ByteCount == count)) return true;
        if (asset is >= VramAssetId.KraidBg3RestoreQuarter0 and <= VramAssetId.KraidBg3RestoreQuarter3)
            return count == KraidBackgroundRomData.StandardBg3TransferBytes;
        return GunshipLiftoffTransferDefinitions.Frames.ToArray().Any(frame => frame.Asset == asset && count == GunshipLiftoffTransferDefinitions.ByteCount);
    }
}
