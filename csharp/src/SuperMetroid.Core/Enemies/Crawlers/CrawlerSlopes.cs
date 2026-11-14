using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Scales a crawler's or Yard's tangent speed along the slope it moves on.</summary>
public static class CrawlerSlopes
{
    /// <summary>
    /// Converts signed 1/256-pixel tangent speed into signed 1/65536-pixel displacement along <paramref name="shape"/>, by
    /// the shape's crawler tangent factor, without truncating the product.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="shape"/> is null.</exception>
    public static SignedSixteenSixteen Scale(SignedEightEight velocity, SlopeShape shape) =>
        velocity.ScaledBy(Ensure.NotNull(shape).CrawlerTangentFactor);
}
