using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Correlated compiled DMA operands, not arbitrary Cartesian products of available artwork.</summary>
internal static class VramDmaDomains
{
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
                return Typed(GrappleTileDefinitions.Transfers.ToArray().Where(frame =>
                    (frame.AtlasOffset < GrappleTileDefinitions.TransferFor(VramAssetId.GrappleHorizontalSegmentTiles).AtlasOffset) ==
                    (family == "grapple-point")).Select(frame => (frame.Asset, (int)frame.ByteCount)));
            case "grapple-native":
                // Legal saved ordinals are the four endpoint frames. Malformed old
                // snapshot counters are not silently claimed as valid table indices.
                return Native(GrappleTileDefinitions.Transfers.ToArray().Select(frame => (frame.SourceAddress, (int)frame.ByteCount)));
            case "trails":
                return Typed([(VramAssetId.ProjectileIceWaveTrailTiles, ProjectileTrailAtlasDefinitions.IceWaveByteCount),
                    (VramAssetId.ProjectileMissileTrailTiles, ProjectileTrailAtlasDefinitions.MissileByteCount)]);
            case "emergency":
                var page = CeresEscapeOverlayTilemapDefinitions.Emergency;
                return Native([(page.SourceAddress, page.WordCount * sizeof(ushort))]);
            case "ceres":
                return Native(CeresEscapeVramTransferDefinitionsTooling.All.ToArray().Select(frame => (frame.SourceAddress, (int)frame.ByteCount)));
            case "ceres-japanese":
                return Native(CeresEscapeVramTransferDefinitionsTooling.All.ToArray().Where(frame =>
                    frame.Pointer is >= CeresEscapeVramTransferDefinitions.JapaneseOverlay and
                    < CeresEscapeVramTransferDefinitions.TimerSprites).Select(frame => (frame.SourceAddress, (int)frame.ByteCount)));
            case "corpse":
                return Native(DeadMonsterRottingDefinitionsTooling.AllTransfers.Select(frame => (frame.SourceAddress, (int)frame.SizeInBytes)));
            case "dead-torizo":
                return Native(DeadTorizoVramTransferDefinitions.ForPhase(0).ToArray().Concat(
                    DeadTorizoVramTransferDefinitions.ForPhase(1).ToArray()).Select(frame => (frame.SourceAddress, (int)frame.SizeInBytes)));
            case "kraid":
                return Typed(Enumerable.Range(0, KraidBackgroundRomData.StandardBg3TransferCount).Select(index =>
                    (KraidBackgroundRomData.StandardBg3AssetForQuarter(index), (int)KraidBackgroundRomData.StandardBg3TransferBytes)));
            case "gunship":
                return Native(GunshipLiftoffTransferDefinitions.Frames.ToArray().Select(frame => (frame.SourceAddress, (int)GunshipLiftoffTransferDefinitions.ByteCount)))
                    .Concat(Typed(GunshipLiftoffTransferDefinitions.Frames.ToArray().Select(frame => (frame.Asset, (int)GunshipLiftoffTransferDefinitions.ByteCount))));
            case "enemies":
                return Native(RoomEnemyGraphicsSetDefinitions.Pointers.SelectMany(pointer =>
                    RoomEnemyGraphicsSetDefinitions.Get(pointer).Records.ToArray()).Select(entry =>
                    (RoomEnemyDefinitionCatalog.Get(entry.DefinitionPointer).TileDataAddress,
                     (int)RoomEnemyDefinitionCatalog.Get(entry.DefinitionPointer).TileDataSize & VramDmaDomainGeometry.EnemyTileByteCountMask)).Distinct());
            case "fx":
                return Native(RoomFxAnimatedTileMechanicsDefinitions.All.SelectMany(definition => definition.Frames.Select(frame =>
                    (RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame.InstructionPointer), (int)definition.TransferByteCount))));
            case "arm":
                return Native(SamusArmCannonArtworkFormat.TileSourcePointers.Select(pointer =>
                    (SamusRenderingRomData.Banks.CharacterData | pointer, (int)SamusRenderingRomData.ArmCannon.TileUploadByteCount)));
            case "death":
                return Native(SamusSpecialSequenceRomData.Death.TileSegments.ToArray().Select(segment =>
                    (segment.SourceAddress, (int)SamusSpecialSequenceRomData.Death.TileSegmentByteCount)));
            case "beam":
                return Typed(Enumerable.Range(0, BeamTileAtlasDefinitions.ArtworkCount).Select(BeamTileAtlasDefinitions.SelectionAt).Select(selection =>
                    (BeamTileCatalog.AssetFor(selection), BeamTileAtlasDefinitions.ByteCount)));
            case "statues":
                return Native(TourianStatueAnimatedTileMechanicsDefinitions.All.SelectMany(definition =>
                    definition.SourceOperandPointers.Select(operand =>
                        (TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition, operand), (int)definition.TransferByteCount))));
            case "treadmill":
                return Native(Enumerable.Range(0, RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount).Select(index =>
                    (WreckedShipTreadmillRomData.FrameSource(index), (int)WreckedShipTreadmillRomData.TransferByteCount)));
            case "landing":
                return Native(LibraryBackgroundProgramDefinitionsTooling.Get(unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress)).Instructions
                    .Where(instruction => instruction.Command == LibraryBackgroundCommand.TransferForDoor)
                    .Select(instruction => (instruction.SourceAddress, (int)instruction.ByteCount)));
            case "sky":
                return Native(SkyRows());
            case "plm":
                // The descriptor walker follows byte widths and source/count
                // correlation in all compiled PLM lists, including disconnected tails.
                return [];
            default: throw new InvalidDataException("Unregistered DMA producer domain " + family);
        }
    }

    private static IEnumerable<(int, int)> SkyRows()
    {
        // Finite source arithmetic for the rooms selecting this callback, not
        // gameplay execution. Using the entire $07F8 mask incorrectly invents
        // index 6..8 ocean rows below the six-screen room. Last-row blue scrolls
        // align at zero; nonblue rows permit the camera's additional $1F.
        foreach (var room in RoomHeaderDefinitions.All)
        foreach (var state in RoomStateSelectionDefinitions.GetStatePointers(room.Pointer).Select(RoomStateDefinitions.Get)
                     .Where(state => ScrollingSkyState.IsScrollingSkyRoomMain(state.MainCallback)))
        {
            int table = state.MainCallback == RoomMainCallback.ScrollingSkyOcean
                ? RoomFxRomData.ScrollingSky.OceanChunkPointerTableAddress
                : RoomFxRomData.ScrollingSky.LandChunkPointerTableAddress;
            int logicalCount = room.WidthInScreens * room.HeightInScreens;
            bool blueBottom = unchecked((short)state.ScrollPointer) >= 0
                ? (byte)(state.ScrollPointer + 1) == (byte)RoomScrollState.Blue
                : RoomScrollDefinitions.Get(state.ScrollPointer).Storage.Span
                    .Slice(logicalCount - room.WidthInScreens, room.WidthInScreens).ToArray()
                    .All(cell => cell == (byte)RoomScrollState.Blue);
            int maximum = ((room.HeightInScreens - 1) << VramDmaDomainGeometry.ScreenPixelShift) +
                (blueBottom ? 0 : VramDmaDomainGeometry.NonBlueLowerAlignment);
            for (int camera = 0; camera <= maximum; camera += VramDmaDomainGeometry.TileRowPixels)
            foreach (int offset in new[] { -(int)RoomFxRomData.ScrollingSky.UpperRowCameraOffset,
                         (int)RoomFxRomData.ScrollingSky.LowerRowCameraOffset })
            {
                ushort position = unchecked((ushort)((camera & RoomFxRomData.ScrollingSky.SourcePositionMask) + offset));
                int source = RoomFxRomData.Banks.Tilemaps | unchecked((ushort)(
                    ScrollingSkyChunkPointerDefinitions.Get(table, position >> VramDmaDomainGeometry.ScreenPixelShift) +
                    (position & VramDmaDomainGeometry.WithinScreenMask) * VramDmaDomainGeometry.TileRowPixels));
                yield return (source, RoomFxRomData.ScrollingSky.TilemapRowByteCount);
                yield return (source + RoomFxRomData.ScrollingSky.TilemapRowByteCount, RoomFxRomData.ScrollingSky.TilemapRowByteCount);
            }
        }
    }
}
