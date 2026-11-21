using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>
/// The pause screen's picture sheet (#1467), over the map characters and the pause interface characters after them under
/// the pause palette: every map cell picture any area map shows, each area's backdrop without the button rows, the button
/// rows without their labels, each label lit and dim, and the equipment page's template and dynamic parts in each state
/// (<see cref="PauseEquipmentPageLayout"/>). The game places them by frame name.
/// </summary>
internal static partial class PausePictureSheet
{
    /// <summary>Cells across the pause frame.</summary>
    private const int Columns = PauseBackdropDefinitions.Columns;

    /// <summary>
    /// How far each page picture's origin lies below its page's corner (#2207): the screen shows each pause page, frame
    /// and equipment page alike, from the page's second pixel row, while the markers, icons and selectors are placed from
    /// its first, so a page's pictures are installed one row higher than the page lays them out. The map's cell pictures
    /// are drawn in a grid that the screen scrolls with the frame, so their origins keep the page's corner.
    /// </summary>
    private static Offset16<ImagePixels> PageLead => new(0, 1);

    /// <summary>Every picture the sheet holds.</summary>
    internal static IEnumerable<ScreenPictureSubject> Subjects(ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        foreach (ScreenPictureSubject cell in MapPictureSheets.CellSubjects(maps))
            yield return cell;
        foreach (NamedScreenPicture<ImportAreaMapPresentation> picture in FramePictures())
            yield return picture.Picture(maps);
        foreach (ScreenPictureSubject equipment in PauseEquipmentPageLayout.Subjects(maps))
            yield return equipment;
    }

    /// <summary>The frame's own pictures by name, with how each is laid out from the import's artwork, in the order they are placed.</summary>
    private static IEnumerable<NamedScreenPicture<ImportAreaMapPresentation>> FramePictures()
    {
        foreach (AreaId area in AreaIds.All)
        {
            string name = PauseFramePictures.BackdropFrame(area);
            yield return new(name, maps =>
            {
                var backdrop = new BackgroundTile[PauseBackdropDefinitions.Cells];
                maps.Pause.Backdrops.LoadTo(backdrop, area);
                return ScreenPictureSubject.Page(name, backdrop, Columns, [.. ButtonRowCells()]);
            });
        }
        int[] labelCells = [.. PauseMenuButtonLabels.ButtonLabelSpans.SelectMany(span => Enumerable.Range(span.Word, span.Count))];
        yield return new(PauseFramePictures.ButtonRowsFrame, maps =>
            ScreenPictureSubject.Cells(PauseFramePictures.ButtonRowsFrame, ButtonRows(maps), Columns, [.. ButtonRowCells().Except(labelCells)]));
        foreach (PauseButtonLabel label in PauseSheetFrameOrder.ButtonLabels)
        {
            foreach (PauseButtonLabelShade shade in PauseSheetFrameOrder.ButtonLabelShades)
            {
                string name = PauseFramePictures.ButtonFrame(label, shade);
                yield return new(name, maps =>
                {
                    BackgroundTile[] shaded = ButtonRows(maps);
                    int[] cells = [.. LabelCells(label)];
                    foreach (int cell in cells)
                        shaded[cell] = shaded[cell] with { ColourVariant = PaletteOf(shade) };
                    return ScreenPictureSubject.Cells(name, shaded, Columns, cells);
                });
            }
        }
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
        var cells = new HashSet<string>(MapPictureSheets.CellSubjects(maps).Select(cell => cell.Name), StringComparer.Ordinal);
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        foreach ((string name, SpriteSheetFrameDocument frame) in sheet.Manifest.Frames)
            frames.Add(name, cells.Contains(name) ? frame : frame with { Origin = frame.Origin + PageLead });
        return sheet with { Manifest = sheet.Manifest with { Frames = frames, Shades = SheetShade.Documents([.. arrow.Shades()]) } };
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
        foreach (NamedScreenPicture<ImportAreaMapPresentation> picture in FramePictures())
            yield return picture.Name;
        foreach (string equipment in PauseEquipmentPageLayout.SubjectNames())
            yield return equipment;
    }
}
