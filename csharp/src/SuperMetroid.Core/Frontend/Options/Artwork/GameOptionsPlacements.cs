namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Where the options screen places each menu's heading and cursor rows and how long the cursor holds each frame: the
/// part of the installed options presentation the game reads (#1502).
/// </summary>
internal sealed partial record GameOptionsPlacements
{
    /// <summary>Each menu's heading origin on the screen, in pixels; drawing subtracts the vertical scroll from Y.</summary>
    public required IReadOnlyDictionary<GameOptionsPage, Offset16<ScreenPixels>> HeadingAnchors { get; init; }
    /// <summary>The missile cursor's origin at each row of the three pages, in screen pixels (#2415).</summary>
    public required IReadOnlyDictionary<GameOptionsRow, Offset16<ScreenPixels>> CursorAnchors { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
