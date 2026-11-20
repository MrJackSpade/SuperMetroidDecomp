using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen memory as checks read it (#1358, #1467, #1515): the floor's loads <see cref="EndingScreenLoads"/>
/// names, executed in the order the cartridge makes them (the scene's floor, the flyaway's floor, the post-credits art,
/// the reward icon), then the explosion's and the logo's transfers. The game draws the pictures its phase shows; this
/// image is what the cartridge's loads would have left, for checks that compare installed artwork byte for byte.
/// </summary>
internal static class EndingScreenMemory
{
    /// <summary>The image <paramref name="loads"/> leave over the bound artwork.</summary>
    [WholeImageFixture("the ending's VRAM image")]
    internal static byte[] Compose(EndingScreenLoads loads, EndingFloorCharacters? floor, CeresFlightCharacters? flight, ImportEndingObjectArtwork? objects,
        EndingFontCharacters? font, EndingReward reward)
    {
        ArgumentNullException.ThrowIfNull(loads);
        var vram = new SnesVram();
        // The transfers that read the ending's objects or font read them installed.
        EndingTransferArtwork Artwork() => new(objects ?? throw new InvalidOperationException("The ending's object transfers require installed object artwork."),
            font ?? throw new InvalidOperationException("The ending's font transfer requires installed font artwork."), reward);
        if (loads.FloorScene is { } loaded)
        {
            EndingFloorSceneArtwork scene = Required(floor)[loaded];
            var mode7 = new Mode7Vram(vram);
            mode7.LoadMap(scene.Map.Span);
            mode7.LoadMap(scene.Map.Span, cell: EndingFloorArtworkFormat.MapByteCount);
            mode7.LoadCharacters((scene as EndingFloorSceneCharacters ?? throw new InvalidOperationException($"The ending's {loaded} floor scene is not installed characters.")).Characters.Span);
        }
        if (loads.FlyawayFloor)
            for (int chunk = 0; chunk < FlyawayChunks; chunk++)
                Flyaway(vram, chunk, flight);
        if (loads.PostCreditsArt)
            EndingScreenImage.Execute(vram, new EndingTransfer.CreditsCharacterArt(), Artwork());
        if (loads.RewardIcon)
            for (int chunk = 0; chunk < EndingRewardGraphicsUploadDefinitions.ChunkCount; chunk++)
                vram.LoadBytes(EndingVramDestinationsConstants.RewardChunkWordOf(chunk) * sizeof(ushort),
                    (Required(floor).RewardIcon as EndingRewardIconCharacters ?? throw new InvalidOperationException("The ending's reward icon is not installed characters.")).Bytes.Span.Slice(
                        EndingRewardGraphicsUploads.ChunkOffset(chunk), EndingRewardGraphicsUploadDefinitions.ChunkBytes));
        if (loads.ExplosionMaps)
        {
            EndingScreenImage.Execute(vram, new EndingTransfer.ExplosionObjects(), Artwork());
            foreach (EndingExplosionPicture map in Enum.GetValues<EndingExplosionPicture>())
                EndingScreenImage.Execute(vram, new EndingTransfer.ExplosionMap(map), Artwork());
            EndingScreenImage.Execute(vram, new EndingTransfer.EndingFont(), Artwork());
        }
        for (int part = 0; part < loads.PostShotParts; part++)
            EndingScreenImage.Execute(vram, new EndingTransfer.PostShotPart(part), Artwork());
        return vram.CaptureImage();
    }

    /// <summary>The chunks the cartridge transfers the flyaway's floor in: its characters, then its map, eight each.</summary>
    private const int FlyawayChunks = 16;

    /// <summary>One of the sixteen flyaway chunks: the first half load floor characters, the rest the padded map.</summary>
    private static void Flyaway(SnesVram vram, int index, CeresFlightCharacters? flight)
    {
        CeresFlightCharacters content = flight ?? throw new InvalidOperationException("Ending flyaway requires installed flight artwork.");
        int chunkBytes = EndingArtworkExtentDefinitionsConstants.FlyawayChunkBytes;
        int chunksPerLane = EndingArtworkExtentDefinitions.FloorBytes / chunkBytes;
        int offset = index % chunksPerLane * chunkBytes;
        var mode7 = new Mode7Vram(vram);
        if (index < chunksPerLane)
        {
            mode7.LoadCharacters(content.FloorCharacters.Span.Slice(offset, chunkBytes), offset);
            return;
        }
        var map = new byte[EndingArtworkExtentDefinitions.FloorBytes];
        content.FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(map);
        map.AsSpan(EndingArtworkExtentDefinitions.FlyawayMapDataBytes).Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        mode7.LoadMap(map.AsSpan(offset, chunkBytes), offset);
    }

    /// <summary>The bound floor artwork.</summary>
    private static EndingFloorCharacters Required(EndingFloorCharacters? floor) =>
        floor ?? throw new InvalidOperationException("Ending Mode-7 scenes require installed artwork.");
}
