using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The cells of the ending's floor (#1404): each escape and explosion scene's map, the flyaway's padded map lane and the
/// reward icon's chunks, as the uploads that lay them out were made.
/// </summary>
internal static class EndingScreenFloor
{
    /// <summary>The floor's cells once <paramref name="uploads"/> have been made, in order.</summary>
    /// <exception cref="InvalidOperationException">An upload names installed artwork that is not bound.</exception>
    internal static byte[] Cells(IReadOnlyList<EndingUpload> uploads, EndingFloorArtwork? floor, CeresFlightArtwork? flight)
    {
        var cells = new byte[AffineFloor.CellCount];
        int chunkBytes = EndingArtworkExtentDefinitions.FlyawayChunkBytes;
        int chunksPerLane = EndingArtworkExtentDefinitions.FloorBytes / chunkBytes;
        foreach (EndingUpload upload in uploads)
        {
            switch (upload.Kind)
            {
                case EndingUploadKind.FloorSceneTiles:
                    // A scene's map is laid out twice, one copy after the other.
                    ReadOnlySpan<byte> map = Floor(floor)[upload.As<EndingFloorSceneUpload>().Scene].Map.Span;
                    map.CopyTo(cells);
                    map.CopyTo(cells.AsSpan(EndingFloorArtworkFormat.MapByteCount));
                    break;
                case EndingUploadKind.FlyawayChunk when upload.As<EndingChunkUpload>().Chunk >= chunksPerLane:
                    // The flyaway's second lane lays out the flight's map, padded with the blank tile to the whole floor.
                    int offset = upload.As<EndingChunkUpload>().Chunk % chunksPerLane * chunkBytes;
                    FlyawayMap(flight).AsSpan(offset, chunkBytes).CopyTo(cells.AsSpan(offset));
                    break;
                case EndingUploadKind.RewardChunk:
                    // Each reward chunk uploads words of a map cell and a tile pixel; it lays out the cells of its words.
                    int chunkCells = EndingRewardGraphicsUploadDefinitions.ChunkBytes / sizeof(ushort);
                    int source = EndingRewardGraphicsUploads.ChunkOffset(upload.As<EndingChunkUpload>().Chunk) / sizeof(ushort);
                    Floor(floor).RewardIcon.Map.Span.Slice(source, chunkCells).CopyTo(cells.AsSpan(upload.As<EndingChunkUpload>().Chunk * chunkCells));
                    break;
                case EndingUploadKind.FlyawayChunk:
                case EndingUploadKind.CreditsCharacterArt:
                    // The flyaway's first lane uploads characters; the post-credits art lays out no floor cells.
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploads), upload.Kind, "Undefined ending upload.");
            }
        }
        return cells;
    }

    /// <summary>The flight's map data padded with the blank tile to the whole floor, as the flyaway uploads it.</summary>
    private static byte[] FlyawayMap(CeresFlightArtwork? flight)
    {
        CeresFlightArtwork content = flight ?? throw new InvalidOperationException("Ending flyaway requires installed flight artwork.");
        var map = new byte[EndingArtworkExtentDefinitions.FloorBytes];
        content.FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(map);
        map.AsSpan(EndingArtworkExtentDefinitions.FlyawayMapDataBytes).Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        return map;
    }

    /// <summary>The bound floor artwork, which the scene and reward uploads require.</summary>
    private static EndingFloorArtwork Floor(EndingFloorArtwork? floor) =>
        floor ?? throw new InvalidOperationException("Ending Mode-7 scenes require installed artwork.");
}
