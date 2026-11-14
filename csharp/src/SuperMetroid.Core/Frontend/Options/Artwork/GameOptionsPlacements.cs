namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Where the options screen places each menu's heading and cursor rows and how long the cursor holds each frame: the
/// part of the installed options presentation the game reads (#1502).
/// </summary>
internal sealed record GameOptionsPlacements
{
    /// <summary>Each menu's heading origin in screen pixels, X=0-255 and Y=0-255; drawing subtracts vertical scroll from Y.</summary>
    public required IReadOnlyDictionary<GameOptionsPage, Position16<SpritePixels>> HeadingAnchors { get; init; }
    /// <summary>Each menu's cursor origins in selection-row order, in screen pixels X=0-255 and Y=0-255.</summary>
    public required IReadOnlyDictionary<GameOptionsPage, Position16<SpritePixels>[]> CursorAnchors { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
