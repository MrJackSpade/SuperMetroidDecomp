using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed room-flash paint composition at $A9:D082-D141: warm background flood
/// against a dim foreground silhouette. Geometry, AI and independent gameplay clocks are excluded.</summary>
internal static class MotherBrainRoomFlashPaintDefinitions
{
    /// <summary>$A9:D112-D12A: warm flood paint, RGB5(31,31,22), repeated over the selected highlighted slots.</summary>
    internal static readonly Bgr555 WarmFlood = new(31, 31, 22);
    /// <summary>$A9:D112-D12A: twelve background colors plus first foreground field brighten;
    /// the remaining eleven foreground colors dim. This split is the selected material composition.</summary>
    internal const int HighlightedColorCount = 13;
    /// <summary>$A9:D082/D0B2/D0E2/D112: four images interpolate the highlighted materials across three equal intervals.</summary>
    internal const int BrightenIntervals = 3;
    /// <summary>$A9:D12C-D140: foreground intensity ends at one quarter; the four strengths select full, three-quarter, half and quarter intensity.</summary>
    internal const int DimIntervals = 4;
}
