namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Native non-square-slope tangent scaling shared by crawlers and Yard.</summary>
public static class CrawlerSlopeDefinitions
{
    /// <summary>
    /// The tangent multiplier, in 256ths, of each of the 32 native slope shapes: square shapes keep the full speed, and
    /// the gentle and steep shapes scale it down. The unused additive words are not consumed by the enemy movement
    /// routine.
    /// </summary>
    internal static readonly ushort[] Multipliers =
    [
        256, 256, 256, 256, 256, 256, 256, 256,
        256, 256, 256, 256, 256, 256, 176, 176,
        256, 256, 192, 256, 192, 192, 216, 216,
        240, 240, 240, 128, 128, 80, 80, 80,
    ];
}
