using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The pause screen's picture sheet (#1467), over the map characters and the pause interface characters after them under
/// the pause palette: every map cell picture any area map shows, each area's backdrop without the button rows, the button
/// rows without their labels, each label lit and dim, and the equipment page's template and dynamic parts in each state
/// (<see cref="PauseEquipmentPageLayout"/>). The game places them by frame name.
/// </summary>
internal static class PausePictureSheet
{
    /// <summary>Cells across the pause frame.</summary>
    private const int Columns = PauseBackdropDefinitions.Columns;

    /// <summary>Every picture the sheet holds.</summary>
    internal static IEnumerable<ScreenPictureSubject> Subjects(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        foreach (ScreenPictureSubject cell in MapPictureSheets.CellSubjects(maps))
            yield return cell;
        int[] buttonCells = [.. ButtonRowCells()];
        foreach (AreaId area in AreaIds.All)
        {
            var backdrop = new BackgroundTile[PauseBackdropDefinitions.Cells];
            maps.Pause.Backdrops.LoadTo(backdrop, area);
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
        foreach (ScreenPictureSubject equipment in PauseEquipmentPageLayout.Subjects(maps))
            yield return equipment;
    }

    /// <summary>The frame's cells the button rows replace.</summary>
    internal static IEnumerable<int> ButtonRowCells() =>
        Enumerable.Range(PauseFrameDefinitions.ButtonRowsFirstRow * Columns, PauseFrameDefinitions.ButtonRowCount * Columns);

    /// <summary>The frame's cells <paramref name="label"/> covers.</summary>
    internal static IEnumerable<int> LabelCells(PauseButtonLabel label) =>
        Enumerable.Range(0, PauseFrameDefinitions.ButtonRowCount).Select(row => PauseMenuButtonLabels.ButtonLabelSpan(label, row))
            .SelectMany(span => Enumerable.Range(span.Word, span.Count));

    /// <summary>The button page's two shown rows laid where the frame shows them, on an otherwise empty frame.</summary>
    internal static BackgroundTile[] ButtonRows(ImportAreaMapPresentation maps)
    {
        BackgroundTile[] page = maps.Pause.Backdrops.CreateButtonTilemap();
        var rows = new BackgroundTile[PauseBackdropDefinitions.Cells];
        page.AsSpan(PauseFrameDefinitions.ButtonSourceFirstRow * Columns, PauseFrameDefinitions.ButtonRowCount * Columns)
            .CopyTo(rows.AsSpan(PauseFrameDefinitions.ButtonRowsFirstRow * Columns));
        return rows;
    }

    /// <summary>The colour variant of <paramref name="shade"/>.</summary>
    internal static int PaletteOf(PauseButtonLabelShade shade) => shade switch
    {
        PauseButtonLabelShade.Lit => PauseFrameDefinitions.LitPalette,
        PauseButtonLabelShade.Dim => PauseFrameDefinitions.DimPalette,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined pause button shade."),
    };

    /// <summary>The map characters and the pause interface characters after them.</summary>
    internal static ScreenCharacters Characters(ImportAreaMapPresentation maps)
    {
        MapCharacterArtwork characters = Ensure.NotNull(maps).Characters;
        int bytes = MapScreenDefinitions.MapTileCount * RoomTileCanvas.CharacterBytes;
        return new ScreenCharacters(TileInkDepth.SixteenColours)
            .Place(PauseTileSetDefinitions.MapFirstTile, characters.Map.Characters[..bytes].ToArray())
            .Place(PauseTileSetDefinitions.InterfaceFirstTile, characters.Pause.Characters[..bytes].ToArray());
    }

    /// <summary>Composes the sheet.</summary>
    /// <remarks>
    /// The reserve arrow's pictures are pre-coloured in its solid bevel colours and in each phase of its pulse (#1512); the
    /// pause screen names the shade its arrow shows in. A picture whose cells the page data leaves clear, such as a blank
    /// label or a map cell's empty shape, shows nothing and is held without a picture (#1513).
    /// </remarks>
    internal static ComposedSpriteSheet Compose(ImportAreaMapPresentation maps)
    {
        MapPictureSheets.RequireCellsPicturedOnce(maps, maps.Palettes.Pause);
        ScreenPictureSubject[] subjects = [.. Subjects(maps)];
        if (!subjects.Select(subject => subject.Name).SequenceEqual(FrameNames(maps)))
            throw new InvalidDataException("The pause sheet's pictures are not the frames the game names it by.");
        ComposedSpriteSheet sheet = ScreenPictureSheetComposer.ComposeHoldingBlanks(subjects, Characters(maps), maps.Palettes.Pause);
        PauseReserveArrowColours arrow = maps.Pause.ReserveUi.ArrowColours;
        return sheet with { Manifest = sheet.Manifest with { Shades = SheetShade.Documents([.. arrow.Shades()]) } };
    }

    /// <summary>
    /// The frames the sheet must hold, in <see cref="Subjects"/> order: the map cells' names from the installed maps, and the
    /// frame's and the equipment page's own names, as their artwork is import data the installation never holds (#1638).
    /// </summary>
    internal static IEnumerable<string> FrameNames(InstalledAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        foreach (ScreenPictureSubject cell in MapPictureSheets.CellSubjects(maps))
            yield return cell.Name;
        for (int index = 0; index < AreaIds.RetailCount; index++)
            yield return PauseFramePictures.BackdropFrame(AreaIds.At(index));
        yield return PauseFramePictures.ButtonRowsFrame;
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
        foreach (PauseButtonLabelShade shade in Enum.GetValues<PauseButtonLabelShade>())
            yield return PauseFramePictures.ButtonFrame(label, shade);
        foreach (string equipment in PauseEquipmentPageLayout.SubjectNames())
            yield return equipment;
    }
}
