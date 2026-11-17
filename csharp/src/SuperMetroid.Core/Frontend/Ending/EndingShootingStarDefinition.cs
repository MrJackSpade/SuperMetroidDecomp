namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One shooting star's flight: how it accelerates, how its picture animates and how long it waits to start.</summary>
/// <param name="Acceleration">Signed 1/256-pixel velocity change each update on both axes.</param>
/// <param name="Period">
/// Animation frame duration, or null for a still star, which keeps its first visible frame for the whole of every flight
/// and so never doubles its acceleration (#1983).
/// </param>
/// <param name="Delay">Initial invisible delay; zero starts immediately.</param>
internal readonly record struct EndingShootingStarDefinition(
    EightEightVector<ScreenPixels> Acceleration, int? Period, int Delay)
{
    /// <summary>A star whose picture animates, each frame lasting <paramref name="period"/> updates.</summary>
    internal static EndingShootingStarDefinition Animated(EightEightVector<ScreenPixels> acceleration, int period, int delay) =>
        new(acceleration, period, delay);

    /// <summary>A still star, which keeps its first visible frame for the whole of every flight.</summary>
    internal static EndingShootingStarDefinition Still(EightEightVector<ScreenPixels> acceleration, int delay) =>
        new(acceleration, null, delay);
}
