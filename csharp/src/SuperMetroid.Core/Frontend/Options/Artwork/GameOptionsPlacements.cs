namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Where the options screen places each menu's heading and cursor rows and how long the cursor holds each frame: the
/// part of the installed options presentation the game reads (#1502).
/// </summary>
internal sealed record GameOptionsPlacements
{
    /// <summary>Each menu's heading origin on the screen, in pixels; drawing subtracts the vertical scroll from Y.</summary>
    public required IReadOnlyDictionary<GameOptionsPage, Offset16<ScreenPixels>> HeadingAnchors { get; init; }
    /// <summary>Each menu's cursor origins on the screen, in pixels, in selection-row order.</summary>
    public required IReadOnlyDictionary<GameOptionsPage, Offset16<ScreenPixels>[]> CursorAnchors { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
