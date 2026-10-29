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

    /// <summary>Writes the fifteen-color zoom shade of <paramref name="zoomStep"/> to CGRAM 81..95.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom step is outside 0..8.</exception>
    internal static void Apply(CeresRidleyFloorPaint steps, GameplayColours colours, int zoomStep)
    {
        Ensure.NotNull(colours);
        _ = Resolve(steps, zoomStep, 0);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
            new RidleyColours(colours).CeresFloorZoom.Write(color, steps.Resolve(zoomStep, color));
    }
}
