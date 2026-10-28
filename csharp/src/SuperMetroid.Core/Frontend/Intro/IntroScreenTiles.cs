using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What the opening cinematic's layers show (#1392, #1404): the scene characters, and the narration font with, once the
/// first illustrated page blanks the Japanese glyphs, the font's blank glyph repeated over them, as their pre-coloured tiles
/// under the cinematic's palette, at the tile numbers each layer reads.
/// </summary>
internal static class IntroScreenTiles
{
    /// <summary>The scene planes: the scene characters.</summary>
    internal static ScreenLayerPictures Scene(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenTileSet(TileInkDepth.SixteenColours)
            .Place(ScreenTileSheets.Of(sheets, SpriteSheetId.IntroSceneTiles), IntroTileSetDefinitions.SceneFirstTile, 0,
                IntroCinematicDefinitions.Artwork.BackgroundCharacterBytes / (ScreenTileSheet.CharacterWordsOf(TileInkDepth.SixteenColours) * sizeof(ushort)))
            .Pictures();
    }

    /// <summary>The narration plane, with the Japanese glyphs blanked when <paramref name="japaneseGlyphsBlanked"/>.</summary>
    internal static ScreenLayerPictures Narration(InstalledSpriteSheets sheets, bool japaneseGlyphsBlanked)
    {
        Ensure.NotNull(sheets);
        ScreenTileSheet font = ScreenTileSheets.Of(sheets, SpriteSheetId.IntroFontTiles);
        var tiles = new ScreenTileSet(TileInkDepth.FourColours)
            .Place(font, IntroTileSetDefinitions.FontFirstTile, 0, IntroFontAtlasFormat.ByteCount / FontCharacterBytes);
        // BlankOut_JapanText_Tiles repeats the font's own blank glyph over the Japanese glyphs.
        if (japaneseGlyphsBlanked)
            tiles.PlaceRepeated(font, IntroTileSetDefinitions.JapaneseBlankFirstTile, IntroCinematicDefinitions.Text.JapaneseBlankSourceOffset / FontCharacterBytes,
                IntroCinematicDefinitions.Artwork.JapaneseBlankCharactersByteCount / FontCharacterBytes);
        return tiles.Pictures();
    }

    /// <summary>Bytes of one 2-bpp font character.</summary>
    private static int FontCharacterBytes => ScreenTileSheet.CharacterWordsOf(TileInkDepth.FourColours) * sizeof(ushort);
}
