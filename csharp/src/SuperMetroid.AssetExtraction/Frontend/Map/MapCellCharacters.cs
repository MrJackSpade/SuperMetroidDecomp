using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The import's translation between the cartridge's area-map words and the cells the game shows (#1513), by
/// <see cref="MapCellCharacterDefinitions"/>: a map word decodes to the cell its character and mirroring picture, and a
/// cell is pictured from the first character and mirroring that pictures it, in the colour row of the shade it is shown
/// in. The game never sees a character, a mirroring or a colour row.
/// </summary>
internal static class MapCellCharacters
{
    /// <summary>Each classified character and mirroring's cell.</summary>
    private static readonly Dictionary<MapCellSource, MapCell> CellsBySource = IndexSources();

    /// <summary>Each cell's first classified character and mirroring, which the import pictures it from.</summary>
    private static readonly Dictionary<MapCell, MapCellSource> SourcesByCell = IndexCells();

    /// <summary>The shade each shade's colour row shows a cell in.</summary>
    private static readonly Dictionary<int, MapCellShade> ShadesByRow =
        MapCellCharacterDefinitions.ShadeRows.ToDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The colour row characters are painted in to compare their pixels: any one row shows the same colour numbers.</summary>
    private const int ComparedRow = 0;

    /// <summary>Colours that leave every pixel's colour number readable, for comparing what characters paint.</summary>
    private static readonly RgbColor[] Unshaded = new RgbColor[ImageColourGroup.HalfColours];

    /// <summary>
    /// The cell a retail area-map word shows: the cell its character and mirroring picture, after requiring the word to
    /// colour it as the retail maps do (any row for the empty cell, which paints nothing; the unexplored row, or for a cell
    /// the maps store explored, the explored row, for any other) and to leave the priority clear.
    /// </summary>
    /// <exception cref="InvalidDataException">The word names an unclassified character or mirroring, a colour row the maps never store the cell in, or the priority.</exception>
    internal static MapCell Decode(MapTileWord word)
    {
        if (word.HasPriority)
            throw new InvalidDataException($"Area-map word {word.Raw:x4} sets the priority, which no retail map cell does.");
        MapCell cell = Picture(word.CharacterIndex, word.Mirroring);
        if (cell.Kind == MapCellKind.Empty)
            return cell;
        MapCellShade? stored = ShadeIn(word.PaletteIndex);
        if (stored == MapCellShade.Unexplored)
        {
            if (ShadeIn(word.AsExplored().PaletteIndex) != MapCellShade.Explored)
                throw new InvalidDataException($"Area-map word {word.Raw:x4} does not show its cell in the explored row once explored.");
            return cell;
        }
        if (stored == MapCellShade.Explored && MapCellCharacterDefinitions.StoredExplored.Contains(cell))
            return cell;
        throw new InvalidDataException($"Area-map word {word.Raw:x4} colours {cell.Name} in row {word.PaletteIndex}, which no retail map stores it in.");
    }

    /// <summary>The cell map character <paramref name="character"/> pictures, mirrored by <paramref name="mirroring"/>.</summary>
    /// <exception cref="InvalidDataException">The character, so mirrored, is not one the retail maps draw a cell with.</exception>
    internal static MapCell Picture(int character, PictureFlip mirroring) =>
        CellsBySource.TryGetValue(new MapCellSource(character, mirroring), out MapCell cell)
            ? cell
            : throw new InvalidDataException($"Map character {character} mirrored {mirroring} is not one the retail maps draw a cell with.");

    /// <summary>The character and mirroring the import pictures <paramref name="cell"/> from.</summary>
    /// <exception cref="InvalidDataException">No map character pictures the cell.</exception>
    internal static MapCellSource Source(MapCell cell) => SourcesByCell.TryGetValue(cell, out MapCellSource source)
        ? source
        : throw new InvalidDataException($"No map character pictures the map cell {cell.Name}.");

    /// <summary>The colour row the map screens show a cell in under <paramref name="shade"/>.</summary>
    internal static int Row(MapCellShade shade) => MapCellCharacterDefinitions.ShadeRows.TryGetValue(shade, out int row)
        ? row
        : throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined map cell shade.");

    /// <summary>The shade colour row <paramref name="row"/> shows a cell in, or none for a row no shade shows.</summary>
    private static MapCellShade? ShadeIn(int row) => ShadesByRow.TryGetValue(row, out MapCellShade shade) ? shade : null;

    /// <summary>The tile the map screens' sheets picture <paramref name="shown"/> from: its cell's source in its shade's row.</summary>
    /// <exception cref="InvalidDataException">No map character pictures the cell.</exception>
    internal static BackgroundTile Tile(ShownMapCell shown)
    {
        MapCellSource source = Source(shown.Cell);
        return new BackgroundTile(source.Character, Row(shown.Shade), highPriority: false, source.Mirroring);
    }

    /// <summary>
    /// Requires every character and mirroring that pictures one cell to paint the same pixels over
    /// <paramref name="characters"/>, so picturing a cell once, from its first source, shows every map word that names it
    /// as that word draws (a character the artwork does not place paints nothing, as the sheets paint it).
    /// </summary>
    /// <exception cref="InvalidDataException">Two sources of one cell paint different pixels.</exception>
    internal static void RequireOnePicturePerCell(ScreenCharacters characters)
    {
        Ensure.NotNull(characters);
        var painted = new Dictionary<MapCell, byte[]>();
        foreach (MapCellCharacter entry in MapCellCharacterDefinitions.Characters)
        {
            byte[] pixels = Paint(characters, entry.Source, ComparedRow, Unshaded).PaletteIndices;
            if (!painted.TryAdd(entry.Cell, pixels) && !painted[entry.Cell].SequenceEqual(pixels))
                throw new InvalidDataException(
                    $"Map character {entry.Source.Character} mirrored {entry.Source.Mirroring} paints other pixels than the first source of the map cell {entry.Cell.Name}, so the cell is two pictures.");
        }
    }

    /// <summary>
    /// Requires each cell a retail map stores in the explored row (<see cref="MapCellCharacterDefinitions.StoredExplored"/>)
    /// to show the same in either row of <paramref name="colours"/>, so showing it by its exploration changes nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">Such a cell shows differently in the two rows.</exception>
    internal static void RequireStoredExploredCellsShowAlike(ScreenCharacters characters, ReadOnlySpan<RgbColor> colours)
    {
        Ensure.NotNull(characters);
        RgbColor[] palette = colours.ToArray();
        foreach (MapCell cell in MapCellCharacterDefinitions.StoredExplored)
        {
            MapCellSource source = Source(cell);
            RgbaPixel[] unexplored = Paint(characters, source, Row(MapCellShade.Unexplored), palette).Pixels;
            RgbaPixel[] explored = Paint(characters, source, Row(MapCellShade.Explored), palette).Pixels;
            if (!unexplored.SequenceEqual(explored))
                throw new InvalidDataException($"The map cell {cell.Name}, which a retail map stores explored, shows differently explored and unexplored.");
        }
    }

    /// <summary>One cell's canvas with <paramref name="source"/> painted over <paramref name="characters"/> in <paramref name="row"/>.</summary>
    private static RoomTileCanvas Paint(ScreenCharacters characters, MapCellSource source, int row, RgbColor[] colours)
    {
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(RoomTileCanvas.CharacterPixels, RoomTileCanvas.CharacterPixels));
        if (!characters.TryCharacter(source.Character, out ReadOnlySpan<byte> character))
            return canvas;
        var tile = new BackgroundTile(source.Character, row, highPriority: false, source.Mirroring);
        if (characters.Depth == TileInkDepth.SixteenColours)
            canvas.PaintTile(0, 0, tile, character, colours);
        else
            canvas.PaintTwoBppTile(0, 0, tile, character, colours);
        return canvas;
    }

    /// <summary>Indexes each classified source's cell, failing on a source classified twice.</summary>
    private static Dictionary<MapCellSource, MapCell> IndexSources()
    {
        var cells = new Dictionary<MapCellSource, MapCell>();
        foreach (MapCellCharacter entry in MapCellCharacterDefinitions.Characters)
        {
            if (!cells.TryAdd(entry.Source, entry.Cell))
                throw new InvalidDataException($"Map character {entry.Source.Character} mirrored {entry.Source.Mirroring} is classified twice.");
        }
        return cells;
    }

    /// <summary>Indexes each cell's first classified source.</summary>
    private static Dictionary<MapCell, MapCellSource> IndexCells()
    {
        var sources = new Dictionary<MapCell, MapCellSource>();
        foreach (MapCellCharacter entry in MapCellCharacterDefinitions.Characters)
            sources.TryAdd(entry.Cell, entry.Source);
        return sources;
    }
}
