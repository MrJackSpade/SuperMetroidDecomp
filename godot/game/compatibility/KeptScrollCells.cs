using Engine.Content;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's scroll cells: one set of fifty for every room, which outlive each room. A room whose layout is an
/// explicit table loads all fifty, its own screens and what the table's load writes after them; a room with none
/// writes only its own, so the cells past them keep what an earlier room, its triggers or its doors left. The camera
/// reads past a room's own screens at its bottom and right edges, so a cell an earlier room left can decide whether
/// the view goes on following Samus there.
/// </summary>
/// <remarks>
/// A cell is found by its row times the room's width plus its column, with no regard to the room's height. A read
/// past the fifty cells is, on the cartridge, a read of the stored explored maps; that is not built, and such a read
/// fails here rather than answer wrongly.
/// </remarks>
public sealed class KeptScrollCells : IScrollCells
{
    /// <summary>The data file of what each explicit layout's load writes after the room's own screens, by the layout's name.</summary>
    private const string LayoutsFile = "rooms/scroll_layouts.json";

    /// <summary>How many cells the cartridge keeps.</summary>
    private const int Count = 50;

    private readonly ScreenScroll[] _cells = new ScreenScroll[Count];
    private int _columns = 1;

    /// <inheritdoc/>
    public void Load(int columns, int rows, IReadOnlyList<ScreenScroll> screens, string layout, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(screens);
        ArgumentNullException.ThrowIfNull(content);
        if (screens.Count != columns * rows || screens.Count > Count)
            throw new ArgumentException($"A room of {columns} by {rows} screens was given {screens.Count} scroll states; the game keeps {Count}.", nameof(screens));
        _columns = columns;
        for (int cell = 0; cell < screens.Count; cell++)
            _cells[cell] = screens[cell];

        // Only an explicit table writes past the room's own screens: every cell to the fiftieth.
        DataNode layouts = content.ReadData(LayoutsFile);
        if (layout.Length == 0 || !layouts.Has(layout))
            return;
        List<ScreenScroll> past = [.. layouts.List(layout).Select(state => state.AsEnum<ScreenScroll>())];
        if (screens.Count + past.Count != Count)
            throw new InvalidDataException($"The scroll layout \"{layout}\" gives {past.Count} cells after a room of {screens.Count} screens; together they must be {Count}.");
        for (int cell = 0; cell < past.Count; cell++)
            _cells[screens.Count + cell] = past[cell];
    }

    /// <inheritdoc/>
    public ScreenScroll Read(int column, int row)
    {
        // A screen's column and row are each a byte, as the cartridge takes them from the view's place.
        int cell = (row & 0xFF) * _columns + (column & 0xFF);
        return cell < Count
            ? _cells[cell]
            : throw new InvalidOperationException(
                $"The view asked about the screen at column {column}, row {row}, past the {Count} scroll cells: the cartridge reads its stored maps there, which is not built.");
    }

    /// <inheritdoc/>
    public void Write(int column, int row, ScreenScroll state)
    {
        if (column < 0 || column >= _columns)
            throw new ArgumentOutOfRangeException(nameof(column), column, "The screen's column lies outside the room.");
        int cell = row * _columns + column;
        if (row < 0 || cell >= Count)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The screen lies outside the {Count} scroll cells the game keeps.");
        _cells[cell] = state;
    }
}
