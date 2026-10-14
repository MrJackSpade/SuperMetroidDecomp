namespace SuperMetroid.Core.Assets;

/// <summary>Editable Spore Spawn RGB5 images for spores, health stages, and three death-scene layers; every row retains all sixteen native palette slots.</summary>
public sealed record SporeSpawnColorDocument
{
    /// <summary>Color schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen spore colors corresponding to $A5:E359, installed in OBJ palette 7 at CGRAM 240–255; stock values share the healthy boss row.</summary>
    public required PaletteRgb5[] Spores { get; init; }
    /// <summary>Four ordered sixteen-color boss rows from healthy to critical, corresponding to $A5:E379–$E3F8; health thresholds remain compiled AI behavior.</summary>
    public required PaletteRgb5[][] Health { get; init; }
    /// <summary>Eight ordered sixteen-color OBJ-palette-1 death phases corresponding to $A5:E3F9–$E4F8, beginning with the critical-health row in stock content.</summary>
    public required PaletteRgb5[][] DeathSprite { get; init; }
    /// <summary>Seven ordered sixteen-color level-graphics BG-palette-4 death phases corresponding to $A5:E4F9–$E5D8.</summary>
    public required PaletteRgb5[][] DeathLevel { get; init; }
    /// <summary>Seven ordered sixteen-color background BG-palette-7 death phases corresponding to $A5:E5D9–$E6B8, independent of the level palette.</summary>
    public required PaletteRgb5[][] DeathBackground { get; init; }
}
