namespace SuperMetroid.Core.Assets;

/// <summary>Editable ten-row full-body Hyper Beam palette schema, separate from the gameplay-owned cycle clock and acquisition state.</summary>
public sealed record SamusHyperBeamColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusHyperBeamColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ten ordered rows of sixteen nonnull RGB5 colors, corresponding to $91:D99E's descending sources $9B:A360..A240; includes color zero's copied payload even though OBJ index-zero pixels remain transparent.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
