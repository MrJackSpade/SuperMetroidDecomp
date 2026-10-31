namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One editable initial screen placement for a fixed rear-view Ceres actor; coordinates preserve native sixteen-bit encoding rather than motion parameters.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as x/y screen words.")]
public sealed record CeresFlightActorPlacement
{
    /// <summary>The validated spawn position as one native screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => new(X, Y);

    /// <summary>Case-sensitive actor-role identity required to match its native spawn-order slot, not an editable instruction or behavior selector.</summary>
    public required string Id { get; init; }
    /// <summary>Initial whole-pixel screen X encoded in 0..65535; negative off-screen origins retain two's-complement encoding, such as 65504 for -32.</summary>
    public required int X { get; init; }
    /// <summary>Initial whole-pixel screen Y encoded in 0..65535, without restricting the actor to the visible viewport.</summary>
    public required int Y { get; init; }
}
