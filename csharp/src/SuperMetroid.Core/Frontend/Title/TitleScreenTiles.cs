using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// What the title's Mode 7 map shows (#1392): its Mode 7 characters, with the Baby Metroid's current animation page over
/// the tiles it animates, as their pre-coloured characters under the title's palette.
/// </summary>
[PpuLayoutBoundary("codec: the title's Mode 7 characters and Baby Metroid page at their native tiles")]
internal static class TitleScreenTiles
{
    /// <summary>The title's tiles with Baby Metroid page <paramref name="babySourcePage"/> loaded.</summary>
    internal static Mode7TilePictures Of(InstalledSpriteSheets sheets, int babySourcePage)
    {
        Ensure.NotNull(sheets);
        int pageTiles = TitleSequenceRomData.Transfers.BabyCharacterPageByteCount / Mode7CharacterSheet.TileBytes;
        return new FloorCharacters()
            .Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.TitleMode7Tiles), 0, 0,
                TitleSequenceRomData.Transfers.FloorCharacterByteCount / Mode7CharacterSheet.TileBytes)
            .Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.TitleBabyTiles), TitleVramLayout.BabyMetroidCharacters.Offset / Mode7CharacterSheet.TileBytes,
                babySourcePage * pageTiles, pageTiles)
            .Pictures();
    }
}
