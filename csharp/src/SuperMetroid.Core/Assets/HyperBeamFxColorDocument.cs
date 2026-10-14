namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for the ten Hyper Beam projectile palette rows; the separate native palette program retains frame durations, destination selection, and loop control.</summary>
public sealed record HyperBeamFxColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="HyperBeamFxColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of eight RGB5 colors corresponding to payloads at <c>$8D:D906 + 20 * row</c>; row colors map to OBJ palette 6 colors 1..8, and every supplied channel remains independently editable.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
