namespace SuperMetroid.Core.Assets;

/// <summary>Defines one arrow's screen anchor and per-phase durations.</summary>
[SpatialComponents("External format: map-arrows.json stores each arrow anchor as flat x/y fields.")]
public sealed record MapArrowEntry
{
    /// <summary>The authored screen-pixel anchor.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<OamPixels> Position => new(checked((ushort)X), checked((ushort)Y));
    /// <summary>Gets the horizontal screen-pixel anchor from zero through 255.</summary>
    public required int X { get; init; }
    /// <summary>Gets the vertical screen-pixel anchor from zero through 223.</summary>
    public required int Y { get; init; }
    /// <summary>Gets one positive update-tick duration per cyclic animation phase.</summary>
    public required int[] DurationTicks { get; init; }
}
