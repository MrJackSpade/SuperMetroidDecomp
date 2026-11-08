using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed room-flash paint composition: warm background flood
/// against a dim foreground silhouette. Geometry, AI and independent gameplay clocks are excluded.</summary>
internal static class MotherBrainRoomFlashPaintDefinitions
{
    /// <summary>Warm flood paint, RGB5(31,31,22), repeated over the selected highlighted slots.</summary>
    internal static readonly RgbColor WarmFlood = new(255, 255, 181);
    /// <summary>Twelve background colors plus first foreground field brighten;
    /// the remaining eleven foreground colors dim. This split is the selected material composition.</summary>
    internal const int HighlightedColorCount = 13;
    /// <summary>Four images interpolate the highlighted materials across three equal intervals.</summary>
    internal const int BrightenIntervals = 3;
    /// <summary>Foreground intensity ends at one quarter; the four strengths select full, three-quarter, half and quarter intensity.</summary>
    internal const int DimIntervals = 4;
    /// <summary>Reviewed visual strength sequence in thirds for the fourteen flash images; each selects a paint image without gameplay callbacks.</summary>
    internal static readonly int[] FlashStrengths = [0, 1, 2, 3, 0, 1, 0, 3, 1, 0, 3, 2, 0, 2];
}
