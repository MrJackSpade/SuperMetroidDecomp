using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The pause screen's picture sheet (#1467), over the map characters and the pause interface characters after them under
/// the pause palette: every map cell picture any area map shows, each area's backdrop without the button rows, the button
/// rows without their labels, and each label lit and dim. The game places them by frame name.
/// </summary>
internal static class PausePictureSheet
{
    /// <summary>Cells across the pause frame.</summary>
    private const int Columns = PauseBackdropDefinitions.Columns;

    /// <summary>Every picture the sheet holds.</summary>
    internal static IEnumerable<ScreenPictureSubject> Subjects(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        foreach (ScreenPictureSubject cell in MapPictureSheets.CellSubjects(maps))
            yield return cell;
        int[] buttonCells = [.. ButtonRowCells()];
        for (int index = 0; index < AreaIds.RetailCount; index++)
        {
            AreaId area = AreaIds.At(index);
            var backdrop = new BackgroundTile[PauseBackdropDefinitions.Cells];
            maps.PauseBackdrops.LoadTo(backdrop, area);
            yield return ScreenPictureSubject.Page(PauseFramePictures.BackdropFrame(area), backdrop, Columns, buttonCells);
        }
        BackgroundTile[] buttons = ButtonRows(maps);
        int[] labelCells = [.. PauseMenuButtonLabels.ButtonLabelSpans.SelectMany(span => Enumerable.Range(span.Word, span.Count))];
        yield return ScreenPictureSubject.Cells(PauseFramePictures.ButtonRowsFrame, buttons, Columns, [.. buttonCells.Except(labelCells)]);
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
        foreach (PauseButtonLabelShade shade in Enum.GetValues<PauseButtonLabelShade>())
        {
            BackgroundTile[] shaded = ButtonRows(maps);
            int[] cells = [.. LabelCells(label)];
            foreach (int cell in cells)
                shaded[cell] = shaded[cell] with { ColourVariant = PaletteOf(shade) };
            yield return ScreenPictureSubject.Cells(PauseFramePictures.ButtonFrame(label, shade), shaded, Columns, cells);
        }
    }

    /// <summary>
    /// The pause frame <paramref name="area"/> shows under <paramref name="highlight"/>, as the cartridge's tiles: the backdrop
    /// with the button page's two shown rows over its button rows, each label in its shade, for verification.
    /// </summary>
    internal static BackgroundTile[] Frame(AreaMapPresentation maps, AreaId area, PauseButtonHighlight highlight)
    {
        Ensure.NotNull(maps);
        var frame = new BackgroundTile[PauseBackdropDefinitions.Cells];
        maps.PauseBackdrops.LoadTo(frame, area);
        BackgroundTile[] buttons = ButtonRows(maps);
        foreach (int cell in ButtonRowCells())
            frame[cell] = buttons[cell];
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
        {
            int palette = PaletteOf(PauseFramePictures.ShadeOf(highlight, label));
            foreach (int cell in LabelCells(label))
                frame[cell] = frame[cell] with { ColourVariant = palette };
        }
        return frame;
    }

    /// <summary>The frame's cells the button rows replace.</summary>
    private static IEnumerable<int> ButtonRowCells() =>
        Enumerable.Range(PauseFrameDefinitions.ButtonRowsFirstRow * Columns, PauseFrameDefinitions.ButtonRowCount * Columns);

    /// <summary>The frame's cells <paramref name="label"/> covers.</summary>
    private static IEnumerable<int> LabelCells(PauseButtonLabel label) =>
        Enumerable.Range(0, PauseFrameDefinitions.ButtonRowCount).Select(row => PauseMenuButtonLabels.ButtonLabelSpan(label, row))
            .SelectMany(span => Enumerable.Range(span.Word, span.Count));

    /// <summary>The button page's two shown rows laid where the frame shows them, on an otherwise empty frame.</summary>
    private static BackgroundTile[] ButtonRows(AreaMapPresentation maps)
    {
        BackgroundTile[] page = maps.PauseBackdrops.CreateButtonTilemap();
        var rows = new BackgroundTile[PauseBackdropDefinitions.Cells];
        page.AsSpan(PauseFrameDefinitions.ButtonSourceFirstRow * Columns, PauseFrameDefinitions.ButtonRowCount * Columns)
            .CopyTo(rows.AsSpan(PauseFrameDefinitions.ButtonRowsFirstRow * Columns));
        return rows;
    }

    /// <summary>The colour variant of <paramref name="shade"/>.</summary>
    private static int PaletteOf(PauseButtonLabelShade shade) => shade switch
    {
        PauseButtonLabelShade.Lit => PauseFrameDefinitions.LitPalette,
        PauseButtonLabelShade.Dim => PauseFrameDefinitions.DimPalette,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined pause button shade."),
    };

    /// <summary>The map characters and the pause interface characters after them.</summary>
    internal static ScreenCharacters Characters(AreaMapPresentation maps)
    {
        MapCharacterArtwork characters = MapCharacterArtwork.Of(Ensure.NotNull(maps));
        int bytes = MapScreenDefinitions.MapTileCount * ScreenTileSheet.CharacterWordsOf(TileInkDepth.SixteenColours) * sizeof(ushort);
        return new ScreenCharacters(TileInkDepth.SixteenColours)
            .Place(PauseTileSetDefinitions.MapFirstTile, characters.Map.Characters[..bytes].ToArray())
            .Place(PauseTileSetDefinitions.InterfaceFirstTile, characters.Pause.Characters[..bytes].ToArray());
    }

    /// <summary>Composes the sheet.</summary>
    internal static ComposedSpriteSheet Compose(AreaMapPresentation maps) =>
        ScreenPictureSheetComposer.Compose([.. Subjects(maps)], Characters(maps), ScreenColourImage.Encode(Ensure.NotNull(maps).Palettes.Pause));

    /// <summary>The frames the sheet must hold.</summary>
    internal static IEnumerable<string> FrameNames(AreaMapPresentation maps) => Subjects(maps).Select(subject => subject.Name);
}
