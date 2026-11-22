using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>The installed options-menu layout document (#1638): the heading and cursor anchors and the cursor's timing the game binds.</summary>
public sealed partial record GameOptionsPlacementDocument
{
    /// <summary>Layout revision; must equal <see cref="GameOptionsLayoutFile.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Each menu's heading origin in screen pixels, by menu name.</summary>
    public required Dictionary<string, Offset16<ScreenPixels>> HeadingAnchors { get; init; }
    /// <summary>Each menu's cursor origins in screen pixels, keyed by the row's name, by menu name.</summary>
    public required Dictionary<string, Dictionary<string, Offset16<ScreenPixels>>> CursorAnchors { get; init; }
    /// <summary>Duration in menu updates, at least one, of each looping cursor frame.</summary>
    public required int CursorFrameDuration { get; init; }
}
