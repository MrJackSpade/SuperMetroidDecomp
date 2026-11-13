namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Editable whole-pixel spawn position for one persistent Ceres destruction actor; its animation and motion identity remain fixed.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as signed x/y screen pixels.")]
public sealed record CeresDestructionActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => Position16<ScreenPixels>.Wrapped(X, Y);

    /// <summary>Case-sensitive persistent actor-role key required to match its ordered slot, not an editable behavior selector.</summary>
    public required string Id { get; init; }
    /// <summary>Initial whole-pixel screen X, -32768 through 32767: negative lies left of the screen (#1627); the actor's compiled wrap policy remains unchanged.</summary>
    public required int X { get; init; }
    /// <summary>Initial whole-pixel screen Y, -32768 through 32767: negative lies above the screen (#1627), without a visible-viewport restriction.</summary>
    public required int Y { get; init; }
}
