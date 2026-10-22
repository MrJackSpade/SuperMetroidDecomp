using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// What the opening cinematic's background characters show (#1392): the scene characters and the narration font it loads,
/// and, once the first illustrated page blanks the Japanese glyphs, the font's blank glyph repeated over them, as their
/// pre-coloured tiles under the cinematic's palette.
/// </summary>
[PpuLayoutBoundary("codec: the opening cinematic's background characters at their native words")]
internal static class IntroScreenTiles
{
    /// <summary>The cinematic's characters, with the Japanese glyphs blanked when <paramref name="japaneseGlyphsBlanked"/>.</summary>
    internal static ScreenCharacters Of(InstalledSpriteSheets sheets, bool japaneseGlyphsBlanked)
    {
        Ensure.NotNull(sheets);
        ScreenTileSheet font = ScreenTileSheets.Of(sheets, SpriteSheetId.IntroFontTiles);
        var tiles = new ScreenCharacters()
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.IntroSceneTiles), IntroVramLayout.BackgroundCharacters.Offset, 0,
                IntroCinematicRomData.Vram.BackgroundCharacterBytes / (ScreenTileSheet.CharacterWordsOf(BgBitDepth.FourBpp) * sizeof(ushort)))
            .Show(font, IntroVramLayout.FontOne.Offset, 0, IntroFontAtlasFormat.ByteCount / FontCharacterBytes);
        // BlankOut_JapanText_Tiles repeats the font's own blank glyph over the Japanese glyphs.
        return japaneseGlyphsBlanked
            ? tiles.ShowRepeated(font, IntroVramLayout.JapaneseBlankCharacters.Offset, IntroCinematicRomData.Text.JapaneseBlankSourceOffset / FontCharacterBytes,
                IntroCinematicRomData.Vram.JapaneseBlankCharactersByteCount / FontCharacterBytes)
            : tiles;
    }

    /// <summary>Bytes of one 2-bpp font character.</summary>
    private static int FontCharacterBytes => ScreenTileSheet.CharacterWordsOf(BgBitDepth.TwoBpp) * sizeof(ushort);
}
