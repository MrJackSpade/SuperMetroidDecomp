using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>One warning line's first cell and selected text; this value record does not check that the text fits.</summary>
/// <param name="Cell">
/// The overlay cell the line's first character is typed over (#1406), counted row by row across the overlay's two
/// screens; loaded programs require the complete line to fit the overlay.
/// </param>
/// <param name="Text">One through 32 supported characters in a loaded program. Spaces advance the cell without writing a tile; other characters select the caller's glyph base.</param>
public sealed record EscapeTypewriterLine(int Cell, string Text)
{
    /// <summary>The overlay cell the line's first character is typed over.</summary>
    public int Cell { get; init; } = Ensure.BetweenInclusive(0, RoomWrittenOverlay.CellCount - 1, Cell);
}
