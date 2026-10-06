namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed paints and shade policies for Mother Brain's final room at $A9:D082.
/// These specify this room's chosen color composition; all ramps, repeated slots and neutrals calculate.</summary>
internal static class MotherBrainFinalRoomPaintDefinitions
{
    /// <summary>$A9:D082: light fine rear-panel accents, BG palette3 ink4.</summary>
    internal const ushort WallLight = 0x4a16;
    /// <summary>$A9:D088: dark ends of the same rear-panel accents, BG palette3 ink7.</summary>
    internal const ushort WallDark = 0x1ca7;
    /// <summary>$A9:D08A: recessed pipe/panel-edge light, BG palette3 ink8.</summary>
    internal const ushort RecessedLight = 0x20e5;
    /// <summary>$A9:D096: selected amber BG palette3 inkE. Its exact pixel role is unestablished;
    /// retention follows its categorical paint role, not absence from inspected compositions.</summary>
    internal const ushort Amber = 31 | (22 << 5);
    /// <summary>$A9:D09A: equal red/blue dark-purple panel field, BG palette5/7 ink3.</summary>
    internal const ushort PanelField = 2 | (2 << 10);
    /// <summary>$A9:D09C: neutral foreground metal highlight, repeated by glass/tube.</summary>
    internal const ushort MetalLight = 20 | (20 << 5) | (20 << 10);
    /// <summary>$A9:D0A0: neutral foreground metal low surface/contour shade.</summary>
    internal const ushort MetalLow = 8 | (8 << 5) | (8 << 10);
    /// <summary>$A9:D0A4: red lens/readout light, BG palette5/7 ink8.</summary>
    internal const ushort RedLens = 25;
    /// <summary>$A9:D0A8: blue lamp, capsule-strip and panel-indicator light.</summary>
    internal const ushort BlueLens = 23 << 10;
    /// <summary>$A9:D0AE: small warm fixture/readout accents, saturated red plus chosen green/blue.</summary>
    internal const ushort WarmAccent = 31 | (11 << 5) | (6 << 10);
    /// <summary>$A9:D08A/D090: selected quarter-intensity recessed shadow, with nearest RGB5 rounding.</summary>
    internal const int RecessedShadowDivisor = 4;
    /// <summary>$A9:D0A0/D0A2/D0AC: selected half-intensity contour beneath the low metal shade.</summary>
    internal const int MetalContourDivisor = 2;
    /// <summary>$A9:D0A4-D0AA: red and blue lens shadows share a seven-level active-channel decrease.</summary>
    internal const int LensShadeDrop = 7;
    /// <summary>$A9:D08A-D090: recessed ramp red rounds upward; green/blue and other ramps round nearest.</summary>
    internal const bool RecessedRedCeiling = true;
}
