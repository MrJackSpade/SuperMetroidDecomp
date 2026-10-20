namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Typed view of the Ridley palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Ridley palettes")]
public readonly struct RidleyPalettes(IPaletteColors cgram)
{
    /// <summary>Ceres door/container and Baby palettes.</summary>
    public CgramPalette CeresStart => new(cgram, RidleyCgramLayout.CeresStart);
    /// <summary>Ceres eye colours.</summary>
    public CgramPalette CeresEyeFade => new(cgram, RidleyCgramLayout.CeresEyeFade);
    /// <summary>Ceres body fade, OBJ palette 1 copy.</summary>
    public CgramPalette CeresBodyFadeShadow => new(cgram, RidleyCgramLayout.CeresBodyFadeShadow);
    /// <summary>Ceres body fade, OBJ palette 7.</summary>
    public CgramPalette CeresBodyFade => new(cgram, RidleyCgramLayout.CeresBodyFade);
    /// <summary>Shared health shade.</summary>
    public CgramPalette Health => new(cgram, RidleyCgramLayout.Health);
    /// <summary>Ceres alarm text.</summary>
    public CgramPalette CeresAlarm => new(cgram, RidleyCgramLayout.CeresAlarm);
    /// <summary>Ceres getaway backdrop.</summary>
    public CgramPalette CeresRetreatBackground => new(cgram, RidleyCgramLayout.CeresRetreatBackground);
    /// <summary>Ceres shared retreat shade, BG copy.</summary>
    public CgramPalette CeresRetreatSharedBackground => new(cgram, RidleyCgramLayout.CeresRetreatSharedBackground);
    /// <summary>Ceres shared retreat shade, OBJ copy.</summary>
    public CgramPalette CeresRetreatSharedObject => new(cgram, RidleyCgramLayout.CeresRetreatSharedObject);
    /// <summary>Ceres Baby shade.</summary>
    public CgramPalette CeresBaby => new(cgram, RidleyCgramLayout.CeresBaby);
    /// <summary>Ceres Mode 7 zoom shade.</summary>
    public CgramPalette CeresMode7Zoom => new(cgram, RidleyCgramLayout.CeresMode7Zoom);
    /// <summary>Norfair initial palettes.</summary>
    public CgramPalette NorfairInitial => new(cgram, RidleyCgramLayout.NorfairInitial);
    /// <summary>Norfair reveal fade.</summary>
    public CgramPalette NorfairReveal => new(cgram, RidleyCgramLayout.NorfairReveal);
}
