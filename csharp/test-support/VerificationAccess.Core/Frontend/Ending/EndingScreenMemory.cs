using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen as checks read it (#1358, #1467): the floor's loads the screen's state says were made, executed in
/// the order the ending makes them (the scene's floor, the flyaway's loads, the post-credits art, the reward icon's loads), then
/// the explosion's and the post-shot's loads the screen's scene state says were made. The game draws pictures selected by
/// that state; this image is what the loads would have left, for checks that compare installed artwork byte for byte.
/// </summary>
internal static class EndingScreenMemory
{
    /// <summary>The image <paramref name="screen"/>'s loads leave over the bound artwork.</summary>
    [WholeImageFixture("the ending's VRAM image")]
    internal static byte[] Compose(EndingScreen screen, EndingFloorArtwork? floor, CeresFlightArtwork? flight, EndingObjectArtwork? objects,
        EndingFontAtlas? font, EndingReward reward)
    {
        ArgumentNullException.ThrowIfNull(screen);
        var vram = new SnesVram();
        var artwork = new EndingTransferArtwork(objects, font, reward);
        if (screen.FloorScene is { } loaded)
        {
            EndingFloorSceneArtwork scene = Required(floor)[loaded];
            var mode7 = new Mode7Vram(vram);
            mode7.LoadMap(scene.Map.Span);
            mode7.LoadMap(scene.Map.Span, cell: EndingFloorArtworkFormat.MapByteCount);
            mode7.LoadCharacters(EndingFloorSceneCharacters.Of(scene).Characters.Span);
        }
        for (int chunk = 0; chunk < screen.FlyawayChunksLoaded; chunk++)
            Flyaway(vram, chunk, flight);
        if (screen.PostCreditsArtLoaded)
            EndingScreenImage.Execute(vram, new EndingTransfer(EndingTransferKind.CreditsCharacterArt), artwork);
        for (int chunk = 0; chunk < screen.RewardChunksLoaded; chunk++)
            vram.LoadBytes(EndingVramDestinations.RewardChunkWordOf(chunk) * sizeof(ushort),
                EndingRewardIconCharacters.Of(Required(floor).RewardIcon).Bytes.Span.Slice(EndingRewardGraphicsUploads.ChunkOffset(chunk),
                    EndingRewardGraphicsUploadDefinitions.ChunkBytes));
        if (screen.ExplosionMapsLoaded)
        {
            EndingScreenImage.Execute(vram, new EndingTransfer(EndingTransferKind.ExplosionObjects), artwork);
            foreach (EndingObjectFragmentId map in Enum.GetValues<EndingObjectFragmentId>())
                EndingScreenImage.Execute(vram, new EndingTransfer(EndingTransferKind.ExplosionMap, Fragment: map), artwork);
            EndingScreenImage.Execute(vram, new EndingTransfer(EndingTransferKind.EndingFont), artwork);
        }
        for (int part = 0; part < screen.PostShotPartsShown; part++)
            EndingScreenImage.Execute(vram, new EndingTransfer(EndingTransferKind.PostShotPart, part), artwork);
        return vram.CaptureImage();
    }

    /// <summary>One of the sixteen flyaway chunks: the first half load floor characters, the rest the padded map.</summary>
    private static void Flyaway(SnesVram vram, int index, CeresFlightArtwork? flight)
    {
        CeresFlightArtwork content = flight ?? throw new InvalidOperationException("Ending flyaway requires installed flight artwork.");
        int chunkBytes = EndingArtworkExtentDefinitions.FlyawayChunkBytes;
        int chunksPerLane = EndingArtworkExtentDefinitions.FloorBytes / chunkBytes;
        int offset = index % chunksPerLane * chunkBytes;
        var mode7 = new Mode7Vram(vram);
        if (index < chunksPerLane)
        {
            mode7.LoadCharacters(CeresFlightCharacters.Of(content).FloorCharacters.Span.Slice(offset, chunkBytes), offset);
            return;
        }
        var map = new byte[EndingArtworkExtentDefinitions.FloorBytes];
        content.FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(map);
        map.AsSpan(EndingArtworkExtentDefinitions.FlyawayMapDataBytes).Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        mode7.LoadMap(map.AsSpan(offset, chunkBytes), offset);
    }

    /// <summary>The bound floor artwork.</summary>
    private static EndingFloorArtwork Required(EndingFloorArtwork? floor) =>
        floor ?? throw new InvalidOperationException("Ending Mode-7 scenes require installed artwork.");
}
