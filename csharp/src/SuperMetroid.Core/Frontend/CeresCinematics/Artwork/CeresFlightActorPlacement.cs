namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One editable initial screen placement for a fixed rear-view Ceres actor; coordinates are signed screen pixels, not motion parameters.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as signed x/y screen pixels.")]
public sealed record CeresFlightActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Offset16<ScreenPixels> Place => Offset16<ScreenPixels>.Of(X, Y);

    /// <summary>The actor the placement places, by what it shows (#1768): its installed key, not an editable animation or behavior selector.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(CeresRearViewActorRoleJsonConverter))]
    public required CeresRearViewActorRole Role { get; init; }
    /// <summary>The actor's starting screen X, in whole pixels: negative lies left of the screen (#1627).</summary>
    public required int X { get; init; }
    /// <summary>The actor's starting screen Y, in whole pixels: negative lies above the screen (#1627).</summary>
    public required int Y { get; init; }
}
