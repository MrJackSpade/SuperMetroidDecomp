namespace SuperMetroid.Core.Assets;

/// <summary>Editable map-highlight cycle schema, containing only authored durations and palette rows rather than an executable palette program.</summary>
public sealed record MapPaletteCycleDocument
{
    /// <summary>Schema revision; loading requires <see cref="MapPaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable array of one through 255 frames in playback order; copied into compiled values when loaded.</summary>
    public required MapPaletteCycleFrame[] Frames { get; init; }
}
