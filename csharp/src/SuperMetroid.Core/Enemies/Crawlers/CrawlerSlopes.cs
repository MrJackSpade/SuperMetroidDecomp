using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Scales a crawler's or Yard's tangent speed along the slope it moves on.</summary>
public static class CrawlerSlopes
{
    /// <summary>
    /// Converts the crawler's speed along the surface into its displacement along <paramref name="shape"/>, by
    /// the shape's crawler tangent share, without truncating the product.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="shape"/> is null.</exception>
    public static SignedSixteenSixteen Scale(SignedEightEight velocity, SlopeShape shape) =>
        velocity.ScaledBy(Ensure.NotNull(shape).CrawlerTangentShare);
}
