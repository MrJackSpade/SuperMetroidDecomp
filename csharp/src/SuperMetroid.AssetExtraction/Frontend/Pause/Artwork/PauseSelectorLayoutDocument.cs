using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector layout document (#1638): the anchors, the initial dwell and the animation the game binds,
/// each phase naming the frame each selector shows.
/// </summary>
public sealed record PauseSelectorLayoutDocument
{
    /// <summary>Gets the layout revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the initial selector dwell time in update ticks.</summary>
    public required int InitialDurationTicks { get; init; }
    /// <summary>Gets all sixteen named selector anchors in screen pixels.</summary>
    public required Dictionary<string, Position16<SpritePixels>> Anchors { get; init; }
    /// <summary>Gets the ordered cyclic animation phases.</summary>
    public required PauseSelectorPhase[] Animation { get; init; }
}
