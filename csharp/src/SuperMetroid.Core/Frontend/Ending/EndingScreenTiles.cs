using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's backgrounds show (#1392, #1404): the characters its uploads leave, as their pre-coloured tiles under
/// the palette each of its scenes loads: the credits' font under the credits palette; the post-credits text, subtitle,
/// logo, waiting backdrop and wavy Samus under the post-credits palette; and the explosion under the explosion palette.
/// </summary>
internal static class EndingScreenTiles
{
    /// <summary>The tiles <paramref name="layer"/> shows once <paramref name="uploads"/> have been made, in order.</summary>
    internal static ScreenLayerPictures Layer(InstalledSpriteSheets sheets, EndingTileLayer layer, TileInkDepth depth, IEnumerable<EndingUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new ScreenTileSet(depth);
        foreach (EndingUpload upload in Ensure.NotNull(uploads))
        {
            switch (layer)
            {
                case EndingTileLayer.Credits:
                    if (upload.Kind == EndingUploadKind.CreditsFont)
                        Place(SpriteSheetId.EndingCreditsFontTiles, EndingTileSetDefinitions.FontFirstTile, 0, EndingArtworkExtentDefinitions.FontCharacterBytes);
                    break;
                case EndingTileLayer.PostCreditsText:
                    if (upload.Kind == EndingUploadKind.CreditsFont)
                        Place(SpriteSheetId.EndingTextFontTiles, EndingTileSetDefinitions.FontFirstTile, 0, EndingArtworkExtentDefinitions.FontCharacterBytes);
                    else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Place(SpriteSheetId.EndingWaitingTiles, EndingTileSetDefinitions.TextWaitingFirstTile, 0, EndingArtworkExtentDefinitions.WaitingCharacterBytes);
                    else if (upload.Kind == EndingUploadKind.PostShotTransfer)
                        PlacePostShot(EndingPostShotUploads.Get(upload.As<EndingChunkUpload>().Chunk), EndingTileSetDefinitions.TextLogoFirstTile);
                    break;
                case EndingTileLayer.Waiting:
                    if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Place(SpriteSheetId.EndingWaitingTiles, EndingTileSetDefinitions.WaitingFirstTile, 0, EndingArtworkExtentDefinitions.WaitingCharacterBytes);
                    else if (upload.Kind == EndingUploadKind.PostShotTransfer)
                        PlacePostShot(EndingPostShotUploads.Get(upload.As<EndingChunkUpload>().Chunk), EndingTileSetDefinitions.WaitingLogoFirstTile);
                    break;
                case EndingTileLayer.Logo:
                    if (upload.Kind == EndingUploadKind.PostShotTransfer)
                        PlacePostShot(EndingPostShotUploads.Get(upload.As<EndingChunkUpload>().Chunk), EndingTileSetDefinitions.LogoFirstTile);
                    break;
                case EndingTileLayer.WavySamus:
                    if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Place(SpriteSheetId.EndingWavySamusTiles, EndingTileSetDefinitions.WavySamusFirstTile, 0, EndingArtworkExtentDefinitions.PostCreditsFragmentABytes);
                    break;
                case EndingTileLayer.Explosion:
                    if (upload.Kind == EndingUploadKind.ExplosionObjects)
                        Place(SpriteSheetId.EndingExplosionTiles, EndingTileSetDefinitions.ExplosionFirstTile, 0, EndingArtworkExtentDefinitions.ExplosionObjectBytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined ending tile layer.");
            }
        }
        return tiles.Pictures();

        // Places a post-shot transfer's subtitle font, on the text layer, or its logo characters, whose chunks follow one
        // another from the logo's first tile; the transfer's tilemap is not characters.
        void PlacePostShot(EndingPostShotUploadDefinition transfer, int logoFirstTile)
        {
            if (transfer.Source == EndingPostShotUploadSource.Subtitle && layer == EndingTileLayer.PostCreditsText)
                Place(SpriteSheetId.EndingSubtitleTiles, EndingTileSetDefinitions.SubtitleFirstTile, 0, transfer.Length);
            else if (transfer.Source == EndingPostShotUploadSource.LogoTiles)
            {
                int offset = transfer.Offset;
                Place(SpriteSheetId.EndingLogoTiles, logoFirstTile, offset, transfer.Length, offset);
            }
        }

        // Places `bytes` of the sheet's artwork from byte `offset` at `firstTile`, plus `tileOffset` bytes' worth of
        // tiles; a layer reading characters of another depth shows none of them.
        void Place(SpriteSheetId id, int firstTile, int offset, int bytes, int tileOffset = 0)
        {
            ScreenTileSheet sheet = ScreenTileSheets.Of(sheets, id);
            if (sheet.Depth != depth)
                return;
            int characterBytes = ScreenTileSheet.CharacterWordsOf(sheet.Depth) * sizeof(ushort);
            tiles.Place(sheet, firstTile + tileOffset / characterBytes, offset / characterBytes, bytes / characterBytes);
        }
    }
}
