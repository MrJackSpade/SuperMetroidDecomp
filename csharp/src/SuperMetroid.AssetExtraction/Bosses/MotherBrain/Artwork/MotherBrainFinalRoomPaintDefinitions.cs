using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed paints and shade policies for Mother Brain's final room.
/// These specify this room's chosen color composition; all ramps, repeated slots and neutrals calculate.</summary>
internal static class MotherBrainFinalRoomPaintDefinitions
{
    /// <summary>Light fine rear-panel accents, background palette 3 ink 4.</summary>
    internal static readonly RgbColor WallLight = new(22, 16, 18);
    /// <summary>Dark ends of the same rear-panel accents, background palette 3 ink 7.</summary>
    internal static readonly RgbColor WallDark = new(7, 5, 7);
    /// <summary>Recessed pipe/panel-edge light, background palette 3 ink 8.</summary>
    internal static readonly RgbColor RecessedLight = new(5, 7, 8);
    /// <summary>Selected amber, background palette 3 ink 14. Its exact pixel role is unestablished;
    /// retention follows its categorical paint role, not absence from inspected compositions.</summary>
    internal static readonly RgbColor Amber = new(31, 22, 0);
    /// <summary>Equal red/blue dark-purple panel field, background palettes 5 and 7 ink 3.</summary>
    internal static readonly RgbColor PanelField = new(2, 0, 2);
    /// <summary>Neutral foreground metal highlight, repeated by glass/tube.</summary>
    internal static readonly RgbColor MetalLight = new(20, 20, 20);
    /// <summary>Neutral foreground metal low surface/contour shade.</summary>
    internal static readonly RgbColor MetalLow = new(8, 8, 8);
    /// <summary>Red lens/readout light, background palettes 5 and 7 ink 8.</summary>
    internal static readonly RgbColor RedLens = new(25, 0, 0);
    /// <summary>Blue lamp, capsule-strip and panel-indicator light.</summary>
    internal static readonly RgbColor BlueLens = new(0, 0, 23);
    /// <summary>Small warm fixture/readout accents, saturated red plus chosen green/blue.</summary>
    internal static readonly RgbColor WarmAccent = new(31, 11, 6);
    /// <summary>Selected quarter-intensity recessed shadow, with nearest RGB5 rounding.</summary>
    internal const int RecessedShadowDivisor = 4;
    /// <summary>Selected half-intensity contour beneath the low metal shade.</summary>
    internal const int MetalContourDivisor = 2;
    /// <summary>Red and blue lens shadows share a seven-level active-channel decrease.</summary>
    internal const int LensShadeDrop = 7;
    /// <summary>Recessed ramp red rounds upward; green/blue and other ramps round nearest.</summary>
    internal const bool RecessedRedCeiling = true;
}
