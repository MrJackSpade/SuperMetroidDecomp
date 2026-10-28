using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Shades Ceres Ridley's Mode-7 floor by the getaway's zoom.</summary>
internal static class CeresRidleyFloorShades
{
    /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
    internal static RgbColor Resolve(CeresRidleyFloorPaint rows, int zoomHighByte, int color)
    {
        if ((uint)zoomHighByte >= CeresRidleyPaletteDefinitions.GetawayZoomRowCount)
            throw new ArgumentOutOfRangeException(nameof(zoomHighByte));
        return rows.Resolve(zoomHighByte, color);
    }

    /// <summary>Writes the fifteen-color zoom shade of <paramref name="zoomHighByte"/> to CGRAM 81..95.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom row is outside 0..8.</exception>
    internal static void Apply(CeresRidleyFloorPaint rows, GameplayColours colours, int zoomHighByte)
    {
        Ensure.NotNull(colours);
        _ = Resolve(rows, zoomHighByte, 0);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
            new RidleyColours(colours).CeresFloorZoom.Write(color, rows.Resolve(zoomHighByte, color));
    }
}
