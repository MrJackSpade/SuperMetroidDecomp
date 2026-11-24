using Engine.Content;

namespace Engine.World;

/// <summary>
/// How each screen of the room being played lets the view in: what the camera reads, and what a room, its camera
/// zones and anything else that opens or closes a screen writes. One for a session; a game may supply its own
/// (<see cref="Engine.Core.GameDefinition.CreateScrollCells"/>).
/// </summary>
public interface IScrollCells
{
    /// <summary>
    /// Takes up a room as it is set up: how many screens it is across and down, and each screen's state, row by row.
    /// </summary>
    /// <param name="columns">How many screens across the room is.</param>
    /// <param name="rows">How many screens down.</param>
    /// <param name="screens">Each screen's state, row by row and each row from the left.</param>
    /// <param name="layout">The name the room's version gives its scroll layout, or empty when it gives none.</param>
    /// <param name="content">The game's content, for one that keeps more about a layout than the room does.</param>
    void Load(int columns, int rows, IReadOnlyList<ScreenScroll> screens, string layout, ContentLibrary content);

    /// <summary>How the screen at a column and row lets the view in. The camera also asks about screens beside and below the room's own.</summary>
    ScreenScroll Read(int column, int row);

    /// <summary>Sets how the screen at a column and row lets the view in.</summary>
    void Write(int column, int row, ScreenScroll state);
}
