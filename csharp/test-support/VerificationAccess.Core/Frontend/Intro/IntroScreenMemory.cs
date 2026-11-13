using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
    internal static byte[] Characters(ImportIntroCinematicArtwork artwork, IntroFontCharacters installedFont, bool japaneseGlyphsBlanked)
    {
        ImportIntroCinematicArtwork sources = artwork;
        ReadOnlySpan<byte> font = Ensure.NotNull(installedFont).Characters.Span;
        var vram = new SnesVram();
        var intro = new IntroVram(vram);
        intro.BackgroundCharacters.Load(sources.BackgroundCharacters.Characters.Span[..IntroArtworkExtentDefinitions.BackgroundCharacterBytes]);
        intro.FontOne.Load(font);
        if (japaneseGlyphsBlanked)
        {
            // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times and DMA copies the $600 bytes
            // to VMADD $4180. BTS[$1E8E-$1E9D] aliases the resident font, so the repeated character is the font's own.
            ReadOnlySpan<byte> blank = font.Slice(IntroArtworkExtentDefinitions.JapaneseBlankSourceOffset,
                IntroJapaneseBlankGlyph.ByteCount);
            var blanked = new byte[IntroArtworkExtentDefinitions.JapaneseBlankCharactersByteCount];
            for (int offset = 0; offset < blanked.Length; offset += blank.Length)
                blank.CopyTo(blanked.AsSpan(offset));
            intro.JapaneseBlankCharacters.Load(blanked);
        }
        intro.BackgroundPages.Load(BackgroundTileWordBytes.Of(sources.BackgroundPages.Span[..IntroCinematicDefinitionsArtworkConstants.BackgroundPageTilemapCells]));
        intro.IntroObjectCharacters.Load(sources.IntroObjectCharacters.Characters.Span);
        intro.CinematicObjectCharacters.Load(sources.CinematicObjectCharacters.Characters.Span[..IntroCinematicArtworkConstants.ObjectCharacterBytes]);
        return vram.CaptureImage();
    }

    /// <summary>
    /// The memory image <paramref name="intro"/> shows: the characters of the import's artwork its bound catalog came from
    /// (#1616) and the installation's opening font, which the intro's installed narration pictures are composed from (#1583),
    /// with its portrait and narration tilemaps over them.
    /// </summary>
    [WholeImageFixture("the intro's memory image")]
    internal static byte[] Compose(IntroCinematicState intro)
    {
        ImportIntroCinematicArtwork artwork = (intro.CharacterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic rendering requires installed artwork.")).ImportSource;
        IntroFontCharacters font = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.IntroFont;
        return Compose(Characters(artwork, font, intro.CurrentObjects is not null), PortraitTiles(artwork, intro.CurrentObjects?.EyeFrame),
            NarrationTiles(intro.narrationText, intro.CurrentObjects is null, artwork));
    }

    /// <summary>
    /// The narration tilemap the cartridge holds for <paramref name="text"/> (#1467): the installed first card while it is
    /// shown; otherwise the cleared staging map's border and blank text rows, the installed final line and its blanked
    /// neighbours, with each shown text cell's letter, or the blank cell, in its colour variant.
    /// </summary>
    internal static BackgroundTile[] NarrationTiles(IntroNarrationText text, bool firstCardShown, ImportIntroCinematicArtwork artwork)
    {
        var tiles = new BackgroundTile[IntroCinematicDefinitionsPlanesConstants.TextTilemapWordCount];
        if (firstCardShown)
        {
            artwork.InitialNarrationTilemap.Span[..IntroArtworkExtentDefinitions.NarrationTilemapCells].CopyTo(tiles);
            return tiles;
        }
        Array.Fill(tiles, IntroNarrationTilemapConstants.Blank);
        for (int index = 0; index < IntroNarrationTilemapConstants.GameplayBlankStartIndex; index++)
        {
            tiles[index] = IntroNarrationTilemapConstants.Margin;
            tiles[index + IntroNarrationTilemapConstants.JapaneseBlankBottomDelta] = IntroNarrationTilemapConstants.Margin;
        }
        artwork.FinalLine.Tiles.Span[..IntroArtworkExtentDefinitions.FinalLineWordCount].CopyTo(tiles.AsSpan(IntroNarrationTilemapConstants.FinalLineFirstCell));
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
    internal static BackgroundTile[] PortraitTiles(ImportIntroCinematicArtwork artwork, IntroEyeFrame? eye)
    {
        ImportIntroCinematicArtwork sources = artwork;
        BackgroundTile[] portrait = sources.PortraitTilemap.Span[..IntroCinematicDefinitionsArtworkConstants.SamusHeadTilemapCells].ToArray();
        if (eye is not { } frame)
            return portrait;
        ReadOnlySpan<BackgroundTile> rectangle = sources.EyeFrames.FrameTiles((int)frame);
        var corner = new Position16<CharacterCells>(IntroEyeAnimationDefinitions.PortraitEyeCorner.X / CharacterCellConversions.CellPixels,
            IntroEyeAnimationDefinitions.PortraitEyeCorner.Y / CharacterCellConversions.CellPixels);
        Extent16<CharacterCells> size = IntroArtworkExtentDefinitions.EyeFrameSize;
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
