namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One named cinematic actor's initial whole-pixel screen position; animation, palette, sliding, and deletion remain compiled mechanics.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as signed x/y screen pixels.")]
public sealed record CeresRevealActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => Position16<ScreenPixels>.Wrapped(X, Y);

    /// <summary>The actor the placement places, by what it shows (#1768): its installed key.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<ZebesRevealActorRole>))]
    public required ZebesRevealActorRole Role { get; init; }
    /// <summary>Initial horizontal screen-pixel coordinate, -32768 through 32767, negative left of the screen (#1627); not a room-world or block coordinate.</summary>
    public required int X { get; init; }
    /// <summary>Initial vertical screen-pixel coordinate, -32768 through 32767, negative above the screen (#1627); increases downward.</summary>
    public required int Y { get; init; }
}
