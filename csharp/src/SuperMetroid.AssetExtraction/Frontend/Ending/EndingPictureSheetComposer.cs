using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Composes the ending's background picture sheets (#1467): the import executes the ending's character and map loads once
/// and paints each picture the ending's scenes select, the credits roll's staff lines, the waiting backdrop, the logo, the
/// result and copyright panels, the typed glyphs, the wavy Samus and the explosion's maps, each pre-coloured under the
/// palette its scene loads and each pixel in the priority tier its map word gave it.
/// </summary>
internal static class EndingPictureSheetComposer
{
    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>Characters across a screen and its maps.</summary>
    private const int ScreenCells = EndingObjectArtworkFormat.MapCellCount / EndingTextDefinitions.TilemapWidth;

    /// <summary>The pixels of one screen.</summary>
    private static readonly Extent16<ImagePixels> ScreenSize = new(ScreenCells * CharacterPixels, ScreenCells * CharacterPixels);

    /// <summary>The character numbers a background's map words name.</summary>
    private const int CharacterNumbers = ScreenLayerPictures.CharacterIndices;

    /// <summary>The font's character every page clears to: its blank glyph, which must show nothing.</summary>
    private const int BlankGlyph = 0x07f;

    /// <summary>The ending's picture sheets.</summary>
    internal static IReadOnlyList<SpriteSheetId> Sheets { get; } =
        [SpriteSheetId.EndingCreditsRows, SpriteSheetId.EndingPostCreditsPictures, SpriteSheetId.EndingWavySamusPicture, SpriteSheetId.EndingExplosionPictures];

    /// <summary>Composes ending picture sheet <paramref name="sheet"/> from the installed artwork <paramref name="sources"/> read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is not an ending picture sheet.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetId sheet, ScreenTileSources sources)
    {
        Ensure.NotNull(sources);
        return sheet switch
        {
            SpriteSheetId.EndingCreditsRows => ComposeCredits(sources),
            SpriteSheetId.EndingPostCreditsPictures => ComposePostCredits(sources),
            SpriteSheetId.EndingWavySamusPicture => ComposeWavySamus(sources),
            SpriteSheetId.EndingExplosionPictures => ComposeExplosion(sources),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending picture sheet."),
        };
    }

    /// <summary>The frames ending picture sheet <paramref name="sheet"/> holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is not an ending picture sheet.</exception>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EndingCreditsRows => CreditsRollRows.Pictures.OfType<string>(),
        SpriteSheetId.EndingPostCreditsPictures => [.. PostCreditsScreens.Select(screen => screen.Name), .. PostCreditsPanels.Select(panel => panel.Name),
            .. Glyphs().Select(glyph => glyph.Name)],
        SpriteSheetId.EndingWavySamusPicture => [EndingPictureNames.WavySamus],
        SpriteSheetId.EndingExplosionPictures => Enum.GetValues<EndingObjectFragmentId>().Select(EndingPictureNames.ExplosionMap),
        _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending picture sheet."),
    };

    /// <summary>The post-credits sheet's whole-screen pictures, in sheet order.</summary>
    private static readonly IReadOnlyList<(string Name, Func<EndingObjectSourceArtwork, RoomBackgroundTilemapAtlas> Map, bool Logo)> PostCreditsScreens =
    [
        (EndingPictureNames.WaitingBackdrop, objects => objects.WaitingTilemap, false),
        (EndingPictureNames.Logo, objects => objects.PostShotLogoMap, true),
    ];

    /// <summary>The post-credits sheet's panels: their rows, and whether they show after the post-shot's loads, in sheet order.</summary>
    private static readonly IReadOnlyList<(string Name, int Rows, bool AfterPostShot)> PostCreditsPanels =
    [
        (EndingPictureNames.ResultPanel, EndingTextDefinitions.ResultPanelRows, false),
        (EndingPictureNames.CopyrightPanel, EndingTextDefinitions.CopyrightRows, false),
        (EndingPictureNames.JapaneseSubtitle, EndingTextDefinitions.JapaneseSubtitleRows, true),
    ];

    /// <summary>
    /// The typed glyphs and the item percentage's digits and percent sign: each glyph's name and its one or two map words,
    /// top first.
    /// </summary>
    private static IEnumerable<(string Name, BackgroundTile[] Words)> Glyphs()
    {
        foreach (EndingTextStyle style in (EndingTextStyle[])[EndingTextStyle.PercentageSmall, EndingTextStyle.FinalLarge])
        {
            for (char letter = 'A'; letter <= 'Z'; letter++)
                yield return (EndingPictureNames.Glyph(style, letter), EndingTextPresentation.IsLarge(style)
                    ? [EndingTextGlyphs.CompileGlyph(letter, style), EndingTextGlyphs.CompileGlyph(letter, style, bottom: true)]
                    : [EndingTextGlyphs.CompileGlyph(letter, style)]);
        }
        for (int digit = 0; digit < EndingPercentageGlyphDefinitions.Digits; digit++)
            yield return (EndingPictureNames.PercentDigit(digit), [Offset(EndingPercentageGlyphDefinitions.DigitTop, digit),
                Offset(EndingPercentageGlyphDefinitions.DigitBottom, digit)]);
        yield return (EndingPictureNames.PercentSign, [EndingPercentageGlyphDefinitions.PercentTop, EndingPercentageGlyphDefinitions.PercentBottom]);

        static BackgroundTile Offset(BackgroundTile zero, int digit) => zero with { Tile = zero.Tile + digit };
    }

    /// <summary>The credits roll's staff lines, one row each, under the credits palette.</summary>
    private static ComposedSpriteSheet ComposeCredits(ScreenTileSources sources)
    {
        AreaMapPresentation maps = sources.Maps;
        CreditsPresentation credits = InstalledAreaMapPresentation.StaffCreditsOf(maps);
        ReadOnlySpan<byte> font = EndingFontCharacters.Of(maps.EndingFont).Characters.Span[..EndingTransferExtentDefinitions.FontCharacterBytes];
        RequireBlank(font);
        var characters = new Characters(RoomTileCanvas.CharacterBytes);
        characters.Load(0, font);
        var rows = CreditsRollRows.Pictures.Select((name, row) => (Name: name, Row: row)).Where(row => row.Name is not null).ToArray();
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(ScreenSize.X, checked((ushort)(rows.Length * CharacterPixels))));
        RgbColor[] colours = Colours(sources, EndingPaletteId.Credits);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < rows.Length; index++)
        {
            ReadOnlySpan<BackgroundTile> row = credits.GetRow(rows[index].Row);
            for (int column = 0; column < row.Length; column++)
                characters.Paint(canvas, column * CharacterPixels, index * CharacterPixels, row[column], colours);
            frames.Add(rows[index].Name!, WrittenPictureSheetComposer.Frame(canvas, index, 1, WrittenPictureLayer.Bg12));
        }
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.SixteenColours));
    }

    /// <summary>
    /// The post-credits pictures under the post-credits palette: the waiting backdrop over the waiting characters, the logo
    /// over its characters, the panels over the text page's characters as they are when each shows, then the typed glyphs.
    /// </summary>
    private static ComposedSpriteSheet ComposePostCredits(ScreenTileSources sources)
    {
        EndingObjectSourceArtwork objects = sources.EndingObjects;
        EndingTextPresentation text = InstalledAreaMapPresentation.EndingTextOf(sources.Maps);
        ReadOnlySpan<byte> font = EndingFontCharacters.Of(sources.Maps.EndingFont).Characters.Span;
        RequireBlank(font);
        // The waiting layer shows the waiting characters; the logo layer, the logo's.
        var waiting = new Characters(RoomTileCanvas.CharacterBytes);
        waiting.Load(EndingTextLayerDefinitions.WaitingFirstCharacter, objects.WaitingSamus.Characters.Span[..EndingTransferExtentDefinitions.WaitingCharacterBytes]);
        var logo = new Characters(RoomTileCanvas.CharacterBytes);
        logo.Load(EndingTextLayerDefinitions.LogoFirstCharacter, objects.PostShotLogoTiles.Characters.Span);
        // The text layer holds the credits font and the waiting characters, then the post-shot's subtitle and logo.
        var textBefore = new Characters(RoomTileCanvas.CharacterBytes);
        textBefore.Load(EndingTextLayerDefinitions.FontFirstCharacter, font[..EndingTransferExtentDefinitions.FontCharacterBytes]);
        textBefore.Load(EndingTextLayerDefinitions.TextWaitingFirstCharacter, objects.WaitingSamus.Characters.Span[..EndingTransferExtentDefinitions.WaitingCharacterBytes]);
        Characters textAfter = textBefore.Copy();
        textAfter.Load(EndingTextLayerDefinitions.SubtitleFirstCharacter,
            font.Slice(EndingPostShotUploadDefinitions.SubtitleFontOffset, EndingPostShotUploadDefinitions.SubtitleLength));
        textAfter.Load(EndingTextLayerDefinitions.TextLogoFirstCharacter, objects.PostShotLogoTiles.Characters.Span);

        BackgroundTile[][] panels = [text.BuildResultPanel(), text.BuildCopyrightPanel(), [.. text.JapaneseSubtitle]];
        (string Name, BackgroundTile[] Words)[] glyphs = [.. Glyphs()];
        int panelRows = PostCreditsPanels.Sum(panel => panel.Rows);
        int glyphRows = EndingTextDefinitions.SmallGlyphHeight + 2 * EndingTextDefinitions.LargeGlyphHeight;
        int height = (PostCreditsScreens.Count * ScreenCells + panelRows + glyphRows) * CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(ScreenSize.X, checked((ushort)height)));
        RgbColor[] colours = Colours(sources, EndingPaletteId.PostCredits);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        int top = 0;
        foreach ((string name, Func<EndingObjectSourceArtwork, RoomBackgroundTilemapAtlas> map, bool isLogo) in PostCreditsScreens)
        {
            PaintMap(canvas, top, map(objects).Tiles.Span, isLogo ? logo : waiting, colours);
            frames.Add(name, WrittenPictureSheetComposer.Frame(canvas, top / CharacterPixels, ScreenCells, WrittenPictureLayer.Bg12));
            top += ScreenSize.Y;
        }
        for (int panel = 0; panel < PostCreditsPanels.Count; panel++)
        {
            (string name, int rows, bool afterPostShot) = PostCreditsPanels[panel];
            if (panels[panel].Length != rows * EndingTextDefinitions.TilemapWidth)
                throw new InvalidDataException($"The ending's {name} is not {rows} whole rows.");
            PaintMap(canvas, top, panels[panel], afterPostShot ? textAfter : textBefore, colours);
            frames.Add(name, WrittenPictureSheetComposer.Frame(canvas, top / CharacterPixels, rows, WrittenPictureLayer.Bg12));
            top += rows * CharacterPixels;
        }
        // The glyphs sit side by side, the small letters on one row, then the large letters, then the digits.
        int left = 0;
        int glyphHeight = EndingTextDefinitions.SmallGlyphHeight;
        foreach ((string name, BackgroundTile[] words) in glyphs)
        {
            if (words.Length != glyphHeight || left + CharacterPixels > ScreenSize.X)
            {
                top += glyphHeight * CharacterPixels;
                glyphHeight = words.Length;
                left = 0;
            }
            for (int row = 0; row < words.Length; row++)
                textAfter.Paint(canvas, left, top + row * CharacterPixels, words[row], colours);
            frames.Add(name, WrittenPictureSheetComposer.Frame(canvas, new Position16<ImagePixels>(checked((ushort)left), checked((ushort)top)),
                new Extent16<ImagePixels>(CharacterPixels, checked((ushort)(words.Length * CharacterPixels))), WrittenPictureLayer.Bg12));
            left += CharacterPixels;
        }
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.SixteenColours));
    }

    /// <summary>The wavy Samus transformation's map over its four-colour characters under the post-credits palette.</summary>
    private static ComposedSpriteSheet ComposeWavySamus(ScreenTileSources sources)
    {
        EndingObjectSourceArtwork objects = sources.EndingObjects;
        var characters = new Characters(RoomTileCanvas.TwoBppCharacterBytes);
        characters.Load(0, objects.PostCreditsFragmentA.Characters.Span[..EndingTransferExtentDefinitions.PostCreditsFragmentABytes]);
        var canvas = new RoomTileCanvas(ScreenSize);
        RgbColor[] colours = Colours(sources, EndingPaletteId.PostCredits);
        PaintMap(canvas, 0, objects.PostCreditsFragmentB.Tiles.Span, characters, colours);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [EndingPictureNames.WavySamus] = WrittenPictureSheetComposer.Frame(canvas, 0, ScreenCells, WrittenPictureLayer.Bg3),
        };
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.FourColours));
    }

    /// <summary>The explosion's four maps over the explosion characters under the explosion palette.</summary>
    private static ComposedSpriteSheet ComposeExplosion(ScreenTileSources sources)
    {
        EndingObjectSourceArtwork objects = sources.EndingObjects;
        var characters = new Characters(RoomTileCanvas.CharacterBytes);
        characters.Load(0, objects.Explosion.Characters.Span[..EndingTransferExtentDefinitions.ExplosionObjectBytes]);
        EndingObjectFragmentId[] maps = Enum.GetValues<EndingObjectFragmentId>();
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(ScreenSize.X, checked((ushort)(maps.Length * ScreenSize.Y))));
        RgbColor[] colours = Colours(sources, EndingPaletteId.Explosion);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int index = 0; index < maps.Length; index++)
        {
            PaintMap(canvas, index * ScreenSize.Y, objects.Fragment(maps[index]).Tiles.Span, characters, colours);
            frames.Add(EndingPictureNames.ExplosionMap(maps[index]), WrittenPictureSheetComposer.Frame(canvas, index * ScreenCells, ScreenCells, WrittenPictureLayer.Bg12));
        }
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.SixteenColours));
    }

    /// <summary>Paints <paramref name="words"/>, rows of a screen's width, from pixel row <paramref name="top"/>.</summary>
    private static void PaintMap(RoomTileCanvas canvas, int top, ReadOnlySpan<BackgroundTile> words, Characters characters, RgbColor[] colours)
    {
        for (int cell = 0; cell < words.Length; cell++)
            characters.Paint(canvas, cell % ScreenCells * CharacterPixels, top + cell / ScreenCells * CharacterPixels, words[cell], colours);
    }

    /// <summary>The ending palette <paramref name="palette"/>'s background colours, as the screen tile sheets are coloured.</summary>
    private static RgbColor[] Colours(ScreenTileSources sources, EndingPaletteId palette) =>
        ScreenTileSheetComposer.EndingColours(sources.EndingPalettes, palette)[..SpriteImage.SpriteColourCount].ToArray();

    /// <summary>Requires the font's blank glyph to show nothing, as the pages leave its cells without a picture.</summary>
    /// <exception cref="InvalidDataException">The blank glyph has a pixel of another colour than colour zero.</exception>
    private static void RequireBlank(ReadOnlySpan<byte> font)
    {
        if (font.Slice(BlankGlyph * RoomTileCanvas.CharacterBytes, RoomTileCanvas.CharacterBytes).ContainsAnyExcept((byte)0))
            throw new InvalidDataException("The ending font's blank glyph shows pixels; the ending's pages leave it unpictured.");
    }

    /// <summary>
    /// A background layer's characters as the ending's loads leave them: each character number's bytes, or none where no
    /// load placed one, which a map word then shows nothing of.
    /// </summary>
    /// <param name="bytes">The bytes of one character.</param>
    private sealed class Characters(int bytes)
    {
        /// <summary>Each character number's bytes.</summary>
        private readonly byte[] data = new byte[CharacterNumbers * bytes];

        /// <summary>Which character numbers a load placed a character at.</summary>
        private readonly bool[] loaded = new bool[CharacterNumbers];

        /// <summary>Loads <paramref name="characters"/> at consecutive numbers from <paramref name="first"/>; those past the last number are not loaded.</summary>
        internal void Load(int first, ReadOnlySpan<byte> characters)
        {
            int count = Math.Min(characters.Length / bytes, CharacterNumbers - first);
            characters[..(count * bytes)].CopyTo(data.AsSpan(first * bytes));
            loaded.AsSpan(first, count).Fill(true);
        }

        /// <summary>A copy, which later loads leave this one unchanged.</summary>
        internal Characters Copy()
        {
            var copy = new Characters(bytes);
            data.CopyTo(copy.data, 0);
            loaded.CopyTo(copy.loaded, 0);
            return copy;
        }

        /// <summary>Paints <paramref name="word"/> at (<paramref name="left"/>, <paramref name="top"/>), or nothing where its character is not loaded.</summary>
        internal void Paint(RoomTileCanvas canvas, int left, int top, BackgroundTile word, RgbColor[] colours)
        {
            if (!loaded[word.Tile])
                return;
            ReadOnlySpan<byte> character = data.AsSpan(word.Tile * bytes, bytes);
            if (bytes == RoomTileCanvas.TwoBppCharacterBytes)
                canvas.PaintTwoBppTile(left, top, word, character, colours);
            else
                canvas.PaintTile(left, top, word, character, colours);
        }
    }
}
