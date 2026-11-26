using Engine.Content;
using Engine.World;

namespace Engine.Tests.Core;

/// <summary>
/// The plain keeper of a room's screens: it holds the states of the room's own screens and nothing else, and every
/// screen outside the room is closed.
/// </summary>
public sealed class RoomScrollCells : IScrollCells
{
    private ScreenScroll[] _screens = [ScreenScroll.Open];
    private int _columns = 1;
    private int _rows = 1;

    /// <inheritdoc/>
    public void Load(int columns, int rows, IReadOnlyList<ScreenScroll> screens, string layout, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(screens);
        if (screens.Count != columns * rows)
            throw new ArgumentException($"A room of {columns} by {rows} screens was given {screens.Count} scroll states.", nameof(screens));
        (_columns, _rows, _screens) = (columns, rows, [.. screens]);
    }

    /// <inheritdoc/>
    public ScreenScroll Read(int column, int row) =>
        column < 0 || column >= _columns || row < 0 || row >= _rows ? ScreenScroll.Closed : _screens[row * _columns + column];

    /// <inheritdoc/>
    public void Write(int column, int row, ScreenScroll state)
    {
        if (column < 0 || column >= _columns || row < 0 || row >= _rows)
            throw new ArgumentOutOfRangeException(nameof(column), $"The screen at column {column}, row {row} is outside a room of {_columns} by {_rows} screens.");
        _screens[row * _columns + column] = state;
    }
}
