namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One named cinematic actor's initial whole-pixel screen position; animation, palette, sliding, and deletion remain compiled mechanics.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as x/y screen words.")]
public sealed record CeresRevealActorPlacement
{
    /// <summary>The validated spawn position as one native screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => new(checked((ushort)X), checked((ushort)Y));

    /// <summary>Case-sensitive native presentation role, required to match its fixed index in <see cref="CeresRevealActorLayoutDocument.Actors"/>.</summary>
    public required string Id { get; init; }
    /// <summary>Initial horizontal screen-pixel coordinate encoded as a native unsigned 16-bit position word, 0..65535; not a room-world or tile coordinate.</summary>
    public required int X { get; init; }
    /// <summary>Initial vertical screen-pixel coordinate encoded as a native unsigned 16-bit position word, 0..65535; increases downward.</summary>
    public required int Y { get; init; }
}
