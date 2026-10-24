using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Correlated compiled DMA operands, not arbitrary Cartesian products of available artwork.</summary>
internal static class VramDmaDomains
{
    /// <summary>Enumerates source/length pairs or typed assets admitted by one compiled DMA producer family.</summary>
    internal static IEnumerable<VramDmaTransfer> ForFamily(string family, string owner)
    {
        IEnumerable<VramDmaTransfer> Native(IEnumerable<NativeTransfer> pairs) =>
            pairs.Select(pair => new VramDmaTransfer(owner, pair.Source, pair.Count));
        IEnumerable<VramDmaTransfer> Typed(IEnumerable<AssetTransfer> pairs) =>
            pairs.Select(pair => new VramDmaTransfer(owner, 0, pair.Count, pair.Asset));
        switch (family)
        {
            case "grapple-point":
            case "grapple-segment":
                return Typed(GrappleTileDefinitions.Transfers.Where(frame =>
                    (frame.AtlasOffset < GrappleTileDefinitions.TransferFor(VramAssetId.GrappleHorizontalSegmentTiles).AtlasOffset) ==
                    (family == "grapple-point")).Select(frame => new AssetTransfer(frame.Asset, frame.ByteCount)));
            case "grapple-native":
                // Legal saved ordinals are the four endpoint frames. Malformed old
                // snapshot counters are not silently claimed as valid table indices.
                return Native(GrappleTileDefinitions.Transfers.Select(frame => new NativeTransfer(frame.SourceAddress, frame.ByteCount)));
            case "trails":
                return Typed([new AssetTransfer(VramAssetId.ProjectileIceWaveTrailTiles, ProjectileTrailAtlasDefinitions.IceWaveByteCount),
                    new AssetTransfer(VramAssetId.ProjectileMissileTrailTiles, ProjectileTrailAtlasDefinitions.MissileByteCount)]);
            case "kraid":
                return Typed(Enumerable.Range(0, KraidBackgroundRomData.StandardBg3TransferCount).Select(index =>
                    new AssetTransfer(KraidBackgroundRomData.StandardBg3AssetForQuarter(index), KraidBackgroundRomData.StandardBg3TransferBytes)));
            case "gunship":
                return Native(GunshipLiftoffTransferDefinitions.Frames.Select(frame => new NativeTransfer(frame.SourceAddress, GunshipLiftoffTransferDefinitions.ByteCount)))
                    .Concat(Typed(GunshipLiftoffTransferDefinitions.Frames.Select(frame => new AssetTransfer(frame.Asset, GunshipLiftoffTransferDefinitions.ByteCount))));
            case "enemies":
                return Native(RoomEnemyGraphicsSetDefinitions.All.SelectMany(set =>
                    set.Records.ToArray()).Select(entry =>
                    new NativeTransfer(RoomEnemyDefinitionCatalog.Get(entry.DefinitionId).TileDataAddress,
                     RoomEnemyDefinitionCatalog.Get(entry.DefinitionId).TileDataSize & VramDmaDomainGeometry.EnemyTileByteCountMask)).Distinct());
            case "fx":
                return Native(RoomFxAnimatedTileMechanicsDefinitions.All.SelectMany(definition => definition.Frames.Select(frame =>
                    new NativeTransfer(RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame), definition.FrameByteCount))));
            case "arm":
                return Native(SamusArmCannonArtworkFormat.CoverTileDmaSources.Select(pointer =>
                    new NativeTransfer(SamusRenderingRomData.Banks.CharacterData | pointer, SamusRenderingRomData.ArmCannon.TileUploadByteCount)));
            case "beam":
                return Typed(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt).Select(selection =>
                    new AssetTransfer(BeamTileCatalog.AssetFor(selection), BeamTileAtlasDefinitions.ByteCount)));
            case "statues":
                return Native(TourianStatueAnimatedTileMechanicsDefinitions.All.SelectMany(definition =>
                    TourianStatueTileStripDefinitions.Pictures.Select(picture =>
                        new NativeTransfer(TourianStatueTileStripDefinitions.SourceAddress(definition.AnimatedObject, picture), definition.FrameByteCount))));
            case "treadmill":
                return Native(Enumerable.Range(0, RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount).Select(index =>
                    new NativeTransfer(WreckedShipTreadmillRomData.FrameSource(index), WreckedShipTreadmillRomData.FrameByteCount)));
            case "landing":
                return Native(LibraryBackgroundSources.Program(unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)).Instructions
                    .Where(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor)
                    .Select(instruction => new NativeTransfer(instruction.SourceAddress, instruction.ByteCount)));
            case "plm":
                // The descriptor walker follows byte widths and source/count
                // correlation in all compiled PLM lists, including disconnected tails.
                return [];
            default: throw new InvalidDataException("Unregistered DMA producer domain " + family);
        }
    }
}
