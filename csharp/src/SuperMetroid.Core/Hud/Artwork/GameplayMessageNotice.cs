using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Defines one gameplay message's tilemap template and editable text regions.</summary>
public sealed record GameplayMessageNotice
{
    /// <summary>Gets the number of content rows, excluding the shared borders.</summary>
    public required int RowCount { get; init; }
    /// <summary>Gets the row-major 32-cell-wide content template.</summary>
    public required BackgroundTile[] Template { get; init; }
    /// <summary>Gets the text regions overlaid on the template.</summary>
    public required GameplayMessageTextRegion[] Text { get; init; }
    /// <summary>Gets the optional selected-YES replacement row.</summary>
    public BackgroundTile[]? YesSelection { get; init; }
    /// <summary>Gets the optional selected-NO replacement row.</summary>
    public BackgroundTile[]? NoSelection { get; init; }
}
