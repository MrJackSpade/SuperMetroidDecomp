using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Correlated compiled DMA operands, not arbitrary Cartesian products of available artwork.</summary>
internal static class VramDmaDomains
{
    /// <summary>Enumerates source/length pairs or typed assets admitted by one compiled DMA producer family.</summary>
    internal static IEnumerable<VramDmaTransfer> ForFamily(string family, string owner)
    {
        IEnumerable<VramDmaTransfer> Native(IEnumerable<(int Source, int Count)> pairs) =>
            pairs.Select(pair => new VramDmaTransfer(owner, pair.Source, pair.Count));
        IEnumerable<VramDmaTransfer> Typed(IEnumerable<(VramAssetId Asset, int Count)> pairs) =>
            pairs.Select(pair => new VramDmaTransfer(owner, 0, pair.Count, pair.Asset));
        switch (family)
        {
            case "timer":
                return Typed([(VramAssetId.EscapeTimerFirstTiles, EscapeTimerTileAtlasFormat.FirstByteCount),
                    (VramAssetId.EscapeTimerSecondTiles, EscapeTimerTileAtlasFormat.SecondByteCount)]);
            case "grapple-point":
            case "grapple-segment":
                return Typed(GrappleTileDefinitions.Transfers.Where(frame =>
                    (frame.AtlasOffset < GrappleTileDefinitions.TransferFor(VramAssetId.GrappleHorizontalSegmentTiles).AtlasOffset) ==
                    (family == "grapple-point")).Select(frame => (frame.Asset, (int)frame.ByteCount)));
            case "grapple-native":
                // Legal saved ordinals are the four endpoint frames. Malformed old
                // snapshot counters are not silently claimed as valid table indices.
                return Native(GrappleTileDefinitions.Transfers.Select(frame => (frame.SourceAddress, (int)frame.ByteCount)));
            case "trails":
                return Typed([(VramAssetId.ProjectileIceWaveTrailTiles, ProjectileTrailAtlasDefinitions.IceWaveByteCount),
                    (VramAssetId.ProjectileMissileTrailTiles, ProjectileTrailAtlasDefinitions.MissileByteCount)]);
            case "ceres":
                return Native(CeresEscapeVramTransferDefinitionsTooling.All.Select(frame => (frame.SourceAddress, (int)frame.ByteCount)));
            case "kraid":
                return Typed(Enumerable.Range(0, KraidBackgroundRomData.StandardBg3TransferCount).Select(index =>
                    (KraidBackgroundRomData.StandardBg3AssetForQuarter(index), (int)KraidBackgroundRomData.StandardBg3TransferBytes)));
            case "gunship":
                return Native(GunshipLiftoffTransferDefinitions.Frames.Select(frame => (frame.SourceAddress, (int)GunshipLiftoffTransferDefinitions.ByteCount)))
                    .Concat(Typed(GunshipLiftoffTransferDefinitions.Frames.Select(frame => (frame.Asset, (int)GunshipLiftoffTransferDefinitions.ByteCount))));
            case "enemies":
                return Native(RoomEnemyGraphicsSetDefinitions.All.SelectMany(set =>
                    set.Records.ToArray()).Select(entry =>
                    (RoomEnemyDefinitionCatalog.Get(entry.DefinitionPointer).TileDataAddress,
                     RoomEnemyDefinitionCatalog.Get(entry.DefinitionPointer).TileDataSize & VramDmaDomainGeometry.EnemyTileByteCountMask)).Distinct());
            case "fx":
                return Native(RoomFxAnimatedTileMechanicsDefinitions.All.SelectMany(definition => definition.Frames.Select(frame =>
                    (RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame.InstructionPointer), (int)definition.TransferByteCount))));
            case "arm":
                return Native(SamusArmCannonArtworkFormat.TileSourcePointers.Select(pointer =>
                    (SamusRenderingRomData.Banks.CharacterData | pointer, (int)SamusRenderingRomData.ArmCannon.TileUploadByteCount)));
            case "death":
                return Native(SamusSpecialSequenceRomData.Death.TileSegments.Select(segment =>
                    (segment.SourceAddress, (int)SamusSpecialSequenceRomData.Death.TileSegmentByteCount)));
            case "beam":
                return Typed(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt).Select(selection =>
                    (BeamTileCatalog.AssetFor(selection), BeamTileAtlasDefinitions.ByteCount)));
            case "statues":
                return Native(TourianStatueAnimatedTileMechanicsDefinitions.All.SelectMany(definition =>
                    TourianStatueAnimatedTileProgramDefinition.Pictures.Select(picture =>
                        (TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition, picture), definition.TransferByteCount))));
            case "treadmill":
                return Native(Enumerable.Range(0, RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount).Select(index =>
                    (WreckedShipTreadmillRomData.FrameSource(index), (int)WreckedShipTreadmillRomData.TransferByteCount)));
            case "landing":
                return Native(LibraryBackgroundSources.Program(unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)).Instructions
                    .Where(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor)
                    .Select(instruction => (instruction.SourceAddress, (int)instruction.ByteCount)));
            case "plm":
                // The descriptor walker follows byte widths and source/count
                // correlation in all compiled PLM lists, including disconnected tails.
                return [];
            default: throw new InvalidDataException("Unregistered DMA producer domain " + family);
        }
    }
}
