namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for Zebetite's two barrier-core inks across eight pulse steps; shared animation-counter ownership, update conditions, health, and collision remain engine behavior.</summary>
public sealed record ZebetiteColorDocument
{
    /// <summary>Schema revision, which must equal <see cref="ZebetiteColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Eight ordered rows of two RGB5 colors, corresponding to native words at <c>$A6:FD87 + 4 * row</c>; row entries paint OBJ palette 2 colors 12 and 13 in that order.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}
