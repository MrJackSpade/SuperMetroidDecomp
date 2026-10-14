namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for Lower Norfair Ridley's initial OBJ palette and ordered arena-background reveal colors; reveal timing and the terminating step are not asset data.</summary>
public sealed record NorfairRidleyColorDocument
{
    /// <summary>Schema version, which must equal <see cref="NorfairRidleyColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The 32 initial RGB5 colors in native <c>$A6:E1CF-E20E</c> order, including each palette's color-zero entry, copied to CGRAM entries 160..191.</summary>
    public required PaletteRgb5[] Initial { get; init; }
    /// <summary>Fifteen ordered reveal rows, each containing fourteen RGB5 colors for BG palette 7 colors 1..14; the caller applies them at the native three-dispatcher-call cadence.</summary>
    public required PaletteRgb5[][] Reveal { get; init; }
}
