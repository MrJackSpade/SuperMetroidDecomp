namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One named cinematic actor's initial whole-pixel screen position; animation, palette, sliding, and deletion remain compiled mechanics.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as x/y screen words.")]
public sealed record CeresRevealActorPlacement
{
    /// <summary>The validated spawn position as one screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => new(X, Y);

    /// <summary>Case-sensitive presentation role, required to match its fixed index among the installed reveal layout's actors.</summary>
    public required string Id { get; init; }
    /// <summary>Initial horizontal screen-pixel coordinate, a wrapping position of 0..65535; not a room-world or tile coordinate.</summary>
    public required int X { get; init; }
    /// <summary>Initial vertical screen-pixel coordinate, a wrapping position of 0..65535; increases downward.</summary>
    public required int Y { get; init; }
}
