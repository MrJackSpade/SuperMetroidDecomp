using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Exact seven15-color shifted drain images.
/// Reviewed source-prefix paint application, three final paints and selected shade progression. Source3800 is visible dark-blue ink1 here, not transparent; this particular source-prefix application is separately reviewed from the standalone targets.</summary>
internal static class SidehopperDrainPaintDefinitions
{
    /// <summary>Source slot1 becomes visible ink2: pale body/limb edge paint.</summary>
    internal static readonly RgbColor BodyLight = new(31, 27, 22);
    /// <summary>Source slot5 becomes visible ink6: installed detail light endpoint.
    /// Its precise visible pixels are not identified in the three alive maps.</summary>
    internal static readonly RgbColor DetailLight = new(27, 24, 19);
    /// <summary>Source slot8 becomes visible ink9: dark detail/mouth endpoint.</summary>
    internal static readonly RgbColor DetailDark = new(12, 10, 7);
    /// <summary>The drain contains six temporal levels including both endpoints.</summary>
    internal const int DrainIntervals = 5;
    /// <summary>The seventh separately selected corpse-prefix image.</summary>
    internal const int CorpseFrame = 6;
}
