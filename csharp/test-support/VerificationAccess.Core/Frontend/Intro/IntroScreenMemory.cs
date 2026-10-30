using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The memory image the cartridge's opening cinematic holds (#1404): the scene characters, the narration font with the
/// Japanese glyphs blanked once the first illustrated page begins, the scene background pages, the intro and cinematic
/// OBJ sheets, and the portrait and narration tilemaps, at the words `$8B:A469-$A529` upload them to. The cinematic keeps
/// no such image; verification composes it to compare against native VRAM.
/// </summary>
internal static class IntroScreenMemory
{
    /// <summary>The character image of <paramref name="artwork"/> and <paramref name="installedFont"/>, as `$8B:A469-$A529` upload them.</summary>
    [WholeImageFixture("the intro's character image")]
    internal static byte[] Characters(IntroCinematicArtwork artwork, IntroFontAtlas installedFont, bool japaneseGlyphsBlanked)
    {
        IntroCinematicState.RequireFont(installedFont);
        IntroCinematicSourceArtwork sources = IntroCinematicSourceArtwork.Of(artwork);
        ReadOnlySpan<byte> font = IntroFontCharacters.Of(installedFont).Characters.Span;
        var vram = new SnesVram();
        var intro = new IntroVram(vram);
        intro.BackgroundCharacters.Load(sources.BackgroundCharacters.Characters.Span[..IntroCinematicDefinitions.Artwork.BackgroundCharacterBytes]);
        intro.FontOne.Load(font);
        if (japaneseGlyphsBlanked)
        {
            // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times and DMA copies the $600 bytes
            // to VMADD $4180. BTS[$1E8E-$1E9D] aliases the resident font, so the repeated character is the font's own.
            ReadOnlySpan<byte> blank = font.Slice(IntroCinematicDefinitions.Text.JapaneseBlankSourceOffset,
                IntroJapaneseBlankGlyph.ByteCount);
            var blanked = new byte[IntroCinematicDefinitions.Artwork.JapaneseBlankCharactersByteCount];
            for (int offset = 0; offset < blanked.Length; offset += blank.Length)
                blank.CopyTo(blanked.AsSpan(offset));
            intro.JapaneseBlankCharacters.Load(blanked);
        }
        intro.BackgroundPages.Load(BackgroundTileWordBytes.Of(sources.BackgroundPages.Span[..IntroCinematicDefinitionsArtworkConstants.BackgroundPageTilemapCells]));
        intro.IntroObjectCharacters.Load(sources.IntroObjectCharacters.Characters.Span);
        intro.CinematicObjectCharacters.Load(sources.CinematicObjectCharacters.Characters.Span[..IntroCinematicArtworkConstants.ObjectCharacterBytes]);
        return vram.CaptureImage();
    }

    /// <summary>The memory image <paramref name="intro"/> shows: its bound characters with its portrait and narration tilemaps over them.</summary>
    [WholeImageFixture("the intro's memory image")]
    internal static byte[] Compose(IntroCinematicState intro)
    {
        IntroCinematicArtwork artwork = intro.CharacterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic rendering requires installed artwork.");
        IntroFontAtlas font = intro.IntroFont ?? throw new InvalidOperationException(
            "Opening cinematic rendering requires the installed font atlas.");
        return Compose(Characters(artwork, font, !intro.narrationText.FirstCardShown), PortraitTiles(artwork, intro.CurrentObjects?.EyeFrame),
            NarrationTiles(intro.narrationText, artwork));
    }

    /// <summary>
    /// The narration tilemap the cartridge holds for <paramref name="text"/> (#1467): the installed first card while it is
    /// shown; otherwise the cleared staging map's border and blank text rows, the installed final line and its blanked
    /// neighbours, with each shown text cell's letter, or the blank cell, in its colour variant.
    /// </summary>
    internal static BackgroundTile[] NarrationTiles(IntroNarrationText text, IntroCinematicArtwork artwork)
    {
        var tiles = new BackgroundTile[IntroCinematicDefinitionsPlanesConstants.TextTilemapWordCount];
        if (text.FirstCardShown)
        {
            IntroCinematicSourceArtwork.Of(artwork).InitialNarrationTilemap.Span[..IntroCinematicDefinitions.Artwork.NarrationTilemapCells].CopyTo(tiles);
            return tiles;
        }
        Array.Fill(tiles, IntroNarrationTilemapConstants.Blank);
        for (int index = 0; index < IntroCinematicDefinitions.Text.GameplayBlankStartIndex; index++)
        {
            tiles[index] = IntroFinalLineTilemapFormat.MarginTile;
            tiles[index + IntroNarrationTilemapConstants.JapaneseBlankBottomDelta] = IntroFinalLineTilemapFormat.MarginTile;
        }
        IntroCinematicSourceArtwork.Of(artwork).FinalLine.Tiles.Span[..IntroCinematicDefinitions.Text.FinalLineWordCount].CopyTo(tiles.AsSpan(IntroCinematicDefinitions.Text.FinalLineFirstCell));
        tiles[IntroNarrationTilemapConstants.FinalBlankLeftIndex] = IntroNarrationTilemapConstants.FinalBlank;
        tiles[IntroNarrationTilemapConstants.FinalBlankRightIndex] = IntroNarrationTilemapConstants.FinalBlank;
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
            {
                IntroTextCell cell = text.ShownAt(column, row);
                BackgroundTile shown = cell.Letter is { } letter ? IntroNarrationGlyphTiles.CompileGlyph(letter) : IntroNarrationTilemapConstants.Blank;
                tiles[row * IntroNarrationText.RowCells + column] = shown with { ColourVariant = cell.ColourVariant };
            }
        }
        return tiles;
    }

    /// <summary>
    /// The portrait tilemap the cartridge holds for the eye frame <paramref name="eye"/> the portrait shows (#1467): the
    /// installed portrait with the frame's rectangle over its eye, or the portrait alone before the eye's first frame.
    /// </summary>
    internal static BackgroundTile[] PortraitTiles(IntroCinematicArtwork artwork, IntroEyeFrame? eye)
    {
        IntroCinematicSourceArtwork sources = IntroCinematicSourceArtwork.Of(artwork);
        BackgroundTile[] portrait = sources.PortraitTilemap.Span[..IntroCinematicDefinitionsArtworkConstants.SamusHeadTilemapCells].ToArray();
        if (eye is not { } frame)
            return portrait;
        ReadOnlySpan<BackgroundTile> rectangle = sources.EyeFrames.FrameTiles((int)frame);
        Position16<TilemapCells> corner = IntroEyeAnimationDefinitions.PortraitCell;
        Extent16<TilemapCells> size = IntroEyeAnimationDefinitions.FrameSize;
        for (int cell = 0; cell < rectangle.Length; cell++)
            portrait[(corner.Y + cell / size.X) * IntroCinematicDefinitions.Planes.Width + corner.X + cell % size.X] = rectangle[cell];
        return portrait;
    }

    /// <summary>The memory image of <paramref name="characters"/> with <paramref name="portrait"/> and <paramref name="narration"/> over them.</summary>
    [WholeImageFixture("the intro's memory image")]
    internal static byte[] Compose(ReadOnlySpan<byte> characters, BackgroundTile[] portrait, BackgroundTile[] narration)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, characters);
        var view = new IntroVram(vram);
        view.PortraitTilemap.Write(BackgroundTileWordBytes.Words(portrait));
        view.NarrationTilemap.Write(BackgroundTileWordBytes.Words(narration));
        return vram.CaptureImage();
    }
}
