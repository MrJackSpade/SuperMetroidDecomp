using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// What the title's Mode 7 map shows (#1392): its Mode 7 characters, with the Baby Metroid's current animation page over
/// the tiles it animates, as their pre-coloured characters under the title's palette.
/// </summary>
internal static class TitleScreenTiles
{
    /// <summary>The title's tiles with Baby Metroid page <paramref name="babySourcePage"/> loaded.</summary>
    internal static FloorTilePictures Of(InstalledSpriteSheets sheets, int babySourcePage)
    {
        Ensure.NotNull(sheets);
        int pageTiles = TitleSequenceDefinitions.Graphics.BabyCharacterPageByteCount / FloorCharacterSheet.TileBytes;
        return new FloorCharacters()
            .Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.TitleFloorTiles), TitleTileSetDefinitions.FloorFirstTile, 0,
                TitleSequenceDefinitions.Graphics.FloorCharacterByteCount / FloorCharacterSheet.TileBytes)
            .Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.TitleBabyTiles), TitleTileSetDefinitions.BabyFirstTile,
                babySourcePage * pageTiles, pageTiles)
            .Pictures();
    }
}
