namespace SuperMetroid.Core.Assets;

/// <summary>Versioned editable RGB5 pulse and initialization-target images for the live Shitroid encounter, excluding timer, fade-destination, and sound behavior.</summary>
public sealed record ShitroidColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Eight ordered phases of four innard inks each, corresponding to $A9:F6D1; all RGB5 channels must be from 0 through 31.</summary>
    public required PaletteRgb5[][] Normal { get; init; }
    /// <summary>Sixteen ordered live-victim colors corresponding to $A9:F8C6, including transparent palette slot zero.</summary>
    public required PaletteRgb5[] Sidehopper { get; init; }
    /// <summary>Sixteen ordered giant Baby Metroid colors corresponding to $A9:F8E6; slot zero is preserved as data even though OBJ transparency gives it no visible hue.</summary>
    public required PaletteRgb5[] Shitroid { get; init; }
    /// <summary>Sixteen ordered drained-victim colors corresponding to $A9:F8A6, including transparent palette slot zero.</summary>
    public required PaletteRgb5[] DeadSidehopper { get; init; }
}
