using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's floor characters show (#1392, #1404): the pre-coloured tiles its floor loads leave.
/// </summary>
internal static class EndingScreenTiles
{
    /// <summary>
    /// The floor characters <paramref name="uploads"/> leave: each escape and explosion scene's, the flyaway gunship's
    /// chunks and the reward icon's; an upload of other characters over the floor's leaves those tiles unpictured.
    /// </summary>
    internal static FloorTilePictures Floor(InstalledSpriteSheets sheets, IEnumerable<EndingUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new FloorCharacters();
        int chunkTiles = EndingArtworkExtentDefinitions.FlyawayChunkBytes / FloorCharacterSheet.TileBytes;
        int chunksPerLane = EndingArtworkExtentDefinitions.FloorBytes / EndingArtworkExtentDefinitions.FlyawayChunkBytes;
        int rewardTiles = EndingRewardGraphicsUploadDefinitions.ChunkBytes / sizeof(ushort) / FloorCharacterSheet.TileBytes;
        foreach (EndingUpload upload in Ensure.NotNull(uploads))
        {
            if (upload.Kind == EndingUploadKind.FloorSceneTiles)
            {
                FloorCharacterSheet scene = ScreenTileSheets.Floor(sheets, ScreenTileSheetDefinitions.EndingFloorScenes[upload.As<EndingFloorSceneUpload>().Scene]);
                tiles.Show(scene, 0, 0, scene.Tiles);
            }
            // The flyaway's first lane uploads the gunship's characters; its second, the map.
            else if (upload.Kind == EndingUploadKind.FlyawayChunk && upload.As<EndingChunkUpload>().Chunk < chunksPerLane)
                tiles.Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.EndingFlyawayFloorTiles), upload.As<EndingChunkUpload>().Chunk * chunkTiles, upload.As<EndingChunkUpload>().Chunk * chunkTiles, chunkTiles);
            // Each reward chunk pictures the next run of the floor's tiles.
            else if (upload.Kind == EndingUploadKind.RewardChunk)
                tiles.Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.EndingRewardIconFloorTiles), upload.As<EndingChunkUpload>().Chunk * rewardTiles, upload.As<EndingChunkUpload>().Chunk * rewardTiles, rewardTiles);
            else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                tiles.Clear(0, EndingArtworkExtentDefinitions.FloorBytes / sizeof(ushort) / FloorCharacterSheet.TileBytes);
        }
        return tiles.Pictures();
    }
}
