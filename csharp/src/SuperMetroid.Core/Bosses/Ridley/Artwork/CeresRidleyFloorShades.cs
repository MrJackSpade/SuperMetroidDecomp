using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Shades Ceres Ridley's Mode-7 floor by the getaway's zoom.</summary>
internal static class CeresRidleyFloorShades
{
    /// <summary>Reads an authored color of getaway zoom step <paramref name="zoomStep"/>.</summary>
    internal static RgbColor Resolve(CeresRidleyFloorPaint steps, int zoomStep, int color)
    {
        if ((uint)zoomStep >= CeresRidleyPaletteDefinitions.GetawayZoomStepCount)
            throw new ArgumentOutOfRangeException(nameof(zoomStep));
        return steps.Resolve(zoomStep, color);
    }
}
