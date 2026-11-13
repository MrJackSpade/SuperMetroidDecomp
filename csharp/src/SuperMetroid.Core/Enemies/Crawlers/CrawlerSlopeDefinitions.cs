namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Non-square-slope tangent scaling shared by crawlers and Yard.</summary>
public static class CrawlerSlopeDefinitions
{
    /// <summary>
    /// The tangent multiplier, in 256ths, of each of the 32 slope shapes: square shapes keep the full speed, and
    /// the gentle and steep shapes scale it down. The unused additive values are not consumed by the enemy movement.
    /// </summary>
    internal static readonly int[] Multipliers =
    [
        256, 256, 256, 256, 256, 256, 256, 256,
        256, 256, 256, 256, 256, 256, 176, 176,
        256, 256, 192, 256, 192, 192, 216, 216,
        240, 240, 240, 128, 128, 80, 80, 80,
    ];
}
