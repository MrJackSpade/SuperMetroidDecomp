namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Scales a crawler's or Yard's tangent speed along the slope it moves on.</summary>
public static class CrawlerSlopes
{
    /// <summary>The tangent multiplier of slope shape <paramref name="shape"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="shape"/> is not one of the 32 records.</exception>
    public static int Multiplier(int shape) =>
        (uint)shape < (uint)CrawlerSlopeDefinitions.Multipliers.Length
            ? CrawlerSlopeDefinitions.Multipliers[shape]
            : throw new InvalidDataException($"Crawler slope shape {shape} exceeds the 32 slope shapes.");

    /// <summary>Converts signed 1/256-pixel tangent speed into signed 1/65536-pixel displacement without truncating the product.</summary>
    public static SignedSixteenSixteen Scale(SignedEightEight velocity, int shape) => velocity.ScaledBy(Multiplier(shape));
}
