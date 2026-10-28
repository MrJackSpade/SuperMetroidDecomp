namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Scales a crawler's or Yard's tangent speed along the slope it moves on.</summary>
public static class CrawlerSlopes
{
    /// <summary>The tangent multiplier of slope shape <paramref name="shape"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="shape"/> is not one of the 32 native records.</exception>
    public static ushort Multiplier(int shape) =>
        (uint)shape < (uint)CrawlerSlopeDefinitions.Multipliers.Length
            ? CrawlerSlopeDefinitions.Multipliers[shape]
            : throw new InvalidDataException($"Crawler slope shape {shape} exceeds the 32 native records.");

    /// <summary>Converts signed 8.8 tangent speed into signed 16.16 displacement without truncating the product.</summary>
    public static int Scale(int velocity, int shape) => unchecked((short)velocity) * Multiplier(shape);
}
