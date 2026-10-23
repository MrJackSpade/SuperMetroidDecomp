using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's Mode 1 backgrounds show (#1392): the characters its uploads leave, as their pre-coloured tiles under
/// the palette each of its scenes loads: the credits' font under the credits palette; the post-credits text, subtitle,
/// logo, waiting backdrop and wavy Samus under the post-credits palette; and the explosion under the explosion palette.
/// </summary>
[PpuLayoutBoundary("codec: the ending's background character uploads at their native words")]
internal static class EndingScreenTiles
{
    /// <summary>
    /// The Mode 7 characters <paramref name="uploads"/> leave: each escape and explosion scene's, the flyaway gunship's
    /// chunks and the reward icon's; an upload of other characters over Mode 7's leaves those tiles unpictured.
    /// </summary>
    internal static Mode7TilePictures Mode7(InstalledSpriteSheets sheets, IEnumerable<EndingUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new FloorCharacters();
        int chunkTiles = EndingCreditsRomData.Rendering.FlyawayUploadBytes / Mode7CharacterSheet.TileBytes;
        int chunksPerLane = EndingCreditsRomData.Rendering.FloorBytes / EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        int rewardTiles = EndingRewardGraphicsUploadDefinitions.ChunkBytes / sizeof(ushort) / Mode7CharacterSheet.TileBytes;
        foreach (EndingUpload upload in Ensure.NotNull(uploads))
        {
            if (upload.Kind == EndingUploadKind.FloorSceneTiles)
            {
                Mode7CharacterSheet scene = ScreenTileSheets.Floor(sheets, ScreenTileSheetDefinitions.EndingFloorScenes[(EndingFloorSceneId)upload.Key]);
                tiles.Show(scene, 0, 0, scene.Tiles);
            }
            // The flyaway's first lane uploads the gunship's characters; its second, the map.
            else if (upload.Kind == EndingUploadKind.FlyawayChunk && upload.Key < chunksPerLane)
                tiles.Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.EndingFlyawayMode7Tiles), upload.Key * chunkTiles, upload.Key * chunkTiles, chunkTiles);
            else if (upload.Kind == EndingUploadKind.RewardChunk)
                tiles.Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.EndingRewardIconMode7Tiles),
                    EndingRewardGraphicsUploadDefinitions.DestinationWord(upload.Key) / Mode7CharacterSheet.TileBytes, upload.Key * rewardTiles, rewardTiles);
            else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                tiles.Clear(0, EndingCreditsRomData.Rendering.FloorBytes / sizeof(ushort) / Mode7CharacterSheet.TileBytes);
        }
        return tiles.Pictures();
    }

    /// <summary>The characters <paramref name="uploads"/> leave, as the scenes coloured by <paramref name="palette"/> show them.</summary>
    internal static ScreenCharacters Of(InstalledSpriteSheets sheets, EndingPaletteId palette, IEnumerable<EndingUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new ScreenCharacters();
        // Uploads of characters no scene the palette colours shows leave nothing it pictures.
        foreach (EndingUpload upload in Ensure.NotNull(uploads))
        {
            if (palette == EndingPaletteId.Credits && upload.Kind == EndingUploadKind.CreditsFont)
                Show(SpriteSheetId.EndingCreditsFontTiles, EndingVramLayout.CreditsFont.Offset, 0, EndingCreditsRomData.Rendering.FontCharacterBytes);
            else if (palette == EndingPaletteId.PostCredits && upload.Kind == EndingUploadKind.CreditsFont)
                Show(SpriteSheetId.EndingTextFontTiles, EndingVramLayout.CreditsFont.Offset, 0, EndingCreditsRomData.Rendering.FontCharacterBytes);
            else if (palette == EndingPaletteId.PostCredits && upload.Kind == EndingUploadKind.CreditsCharacterArt)
            {
                Show(SpriteSheetId.EndingWaitingTiles, EndingVramLayout.WaitingCharacters.Offset, 0, EndingCreditsRomData.Rendering.WaitingCharacterBytes);
                Show(SpriteSheetId.EndingWavySamusTiles, EndingVramLayout.PostCreditsFragmentA.Offset, 0, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes);
            }
            else if (palette == EndingPaletteId.PostCredits && upload.Kind == EndingUploadKind.PostShotTransfer)
                ShowPostShot(EndingPostShotUploadDefinitions.Get(upload.Key));
            else if (palette == EndingPaletteId.Explosion && upload.Kind == EndingUploadKind.ExplosionObjects)
                Show(SpriteSheetId.EndingExplosionTiles, EndingVramLayout.ExplosionObjects.Offset, 0, EndingCreditsRomData.Rendering.ExplosionObjectBytes);
        }
        return tiles;

        // Shows a post-shot transfer's subtitle font or logo characters; its tilemap is not characters.
        void ShowPostShot(EndingPostShotUploadDefinition transfer)
        {
            if (transfer.Source == EndingPostShotUploadSource.Subtitle)
                Show(SpriteSheetId.EndingSubtitleTiles, transfer.DestinationWord, 0, transfer.Length);
            else if (transfer.Source == EndingPostShotUploadSource.LogoTiles)
                Show(SpriteSheetId.EndingLogoTiles, transfer.DestinationWord, transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource, transfer.Length);
        }

        // Shows `bytes` of the sheet's artwork from byte `offset`, loaded at VRAM word `firstWord`.
        void Show(SpriteSheetId id, int firstWord, int offset, int bytes)
        {
            ScreenTileSheet sheet = ScreenTileSheets.Of(sheets, id);
            int characterBytes = ScreenTileSheet.CharacterWordsOf(sheet.Depth) * sizeof(ushort);
            tiles.Show(sheet, firstWord, offset / characterBytes, bytes / characterBytes);
        }
    }
}
