namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable whole-pixel spawn position for one persistent Ceres destruction actor; its native instruction and motion identity remain fixed.</summary>
[SpatialComponents("External format: the actor layout JSON stores each spawn position as x/y screen words.")]
public sealed record CeresDestructionActorPlacement
{
    /// <summary>The validated spawn position as one native screen-pixel value.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<ScreenPixels> Position => new(checked((ushort)X), checked((ushort)Y));

    /// <summary>Case-sensitive persistent actor-role key required to match its ordered slot, not an editable behavior selector.</summary>
    public required string Id { get; init; }
    /// <summary>Initial screen X word encoded as an integer in 0..65535; negative off-screen origins use native two's-complement encoding, and the actor's compiled wrap policy remains unchanged.</summary>
    public required int X { get; init; }
    /// <summary>Initial whole-pixel screen Y word encoded as an integer in 0..65535, without a visible-viewport restriction.</summary>
    public required int Y { get; init; }
}
