namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One editable initial screen placement for a fixed rear-view Ceres actor; coordinates are signed screen pixels, not motion parameters.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as signed x/y screen pixels.")]
public sealed record CeresFlightActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => Position16<ScreenPixels>.Wrapped(X, Y);

    /// <summary>Case-sensitive actor-role identity required to match its spawn-order slot, not an editable animation or behavior selector.</summary>
    public required string Id { get; init; }
    /// <summary>Initial whole-pixel screen X, -32768 through 32767: negative lies left of the screen, such as -32 (#1627).</summary>
    public required int X { get; init; }
    /// <summary>Initial whole-pixel screen Y, -32768 through 32767: negative lies above the screen (#1627), without restricting the actor to the visible viewport.</summary>
    public required int Y { get; init; }
}
