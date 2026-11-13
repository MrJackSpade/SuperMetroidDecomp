using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>The installed options-menu layout document (#1638): the heading and cursor anchors and the cursor's timing the game binds.</summary>
public sealed record GameOptionsPlacementDocument
{
    /// <summary>Layout revision; must equal <see cref="GameOptionsLayoutFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Each menu's heading origin in screen pixels, by menu name.</summary>
    public required Dictionary<string, Position16<SpritePixels>> HeadingAnchors { get; init; }
    /// <summary>Each menu's cursor origins in selection-row order, in screen pixels, by menu name.</summary>
    public required Dictionary<string, Position16<SpritePixels>[]> CursorAnchors { get; init; }
    /// <summary>Cursor origin for phases without a selection table, in screen pixels X=0-511 and Y=0-255.</summary>
    public required Position16<SpritePixels> HiddenCursor { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
