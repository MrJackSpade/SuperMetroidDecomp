namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One shooting star's flight: how it accelerates, how its picture animates and how long it waits to start.</summary>
/// <param name="Acceleration">How much the velocity changes each update on both axes.</param>
/// <param name="Pace">
/// The pace its picture changes at: each frame's duration, or still, keeping its first visible frame for the whole of every
/// flight and so never doubling its acceleration (#1983).
/// </param>
/// <param name="Delay">Initial invisible delay; zero starts immediately.</param>
internal readonly partial record struct EndingShootingStarDefinition(
    PixelVelocity<ScreenPixels> Acceleration, EndingShootingStarPace Pace, int Delay)
{
    /// <summary>A star whose picture animates, each frame lasting <paramref name="period"/> updates.</summary>
    internal static EndingShootingStarDefinition Animated(PixelVelocity<ScreenPixels> acceleration, int period, int delay) =>
        new(acceleration, EndingShootingStarPace.Animated(period), delay);

    /// <summary>A still star, which keeps its first visible frame for the whole of every flight.</summary>
    internal static EndingShootingStarDefinition Still(PixelVelocity<ScreenPixels> acceleration, int delay) =>
        new(acceleration, EndingShootingStarPace.Still, delay);
}
