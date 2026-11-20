namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Norfair Ridley's four health bands (#1934). Each lower band seeks faster, lunges harder and bounces higher, and the
/// three below full health each show their own health colouring.
/// </summary>
internal static class NorfairRidleyHealthBandDefinitions
{
    /// <summary>Ridley from full health down to 9000: his slowest seek and lunge, low bounces, and his own colours.</summary>
    internal static NorfairRidleyHealthBand Unhurt { get; } = new(healthBelow: null, hoverDivisor: 12, grabDivisor: 15,
    [
        Pogo(SignedEightEight.FromPixelFraction(5, 8), SignedEightEight.FromPixelFraction(25, 8), SignedEightEight.FromPixelFraction(1, 8), SignedEightEight.FromPixelFraction(1, 4)),
        Pogo(SignedEightEight.FromPixelFraction(3, 4), SignedEightEight.FromPixelFraction(7, 2), SignedEightEight.FromPixelFraction(1, 8), SignedEightEight.FromPixelFraction(1, 4)),
        Pogo(SignedEightEight.FromPixelFraction(7, 8), SignedEightEight.FromPixelFraction(29, 8), SignedEightEight.FromPixelFraction(1, 8), SignedEightEight.FromPixelFraction(1, 4)),
        Pogo(SignedEightEight.FromPixels(1), SignedEightEight.FromPixelFraction(15, 4), SignedEightEight.FromPixelFraction(1, 8), SignedEightEight.FromPixelFraction(1, 4)),
    ], healthShade: null);

    /// <summary>Ridley below 9000 health, showing the wounded colouring.</summary>
    private static NorfairRidleyHealthBand Wounded { get; } = new(healthBelow: 9000, hoverDivisor: 8, grabDivisor: 13,
    [
        Pogo(SignedEightEight.FromPixelFraction(21, 32), SignedEightEight.FromPixelFraction(31, 8), SignedEightEight.FromPixelFraction(3, 16), SignedEightEight.FromPixelFraction(1, 2)),
        Pogo(SignedEightEight.FromPixelFraction(25, 32), SignedEightEight.FromPixelFraction(17, 4), SignedEightEight.FromPixelFraction(3, 16), SignedEightEight.FromPixelFraction(1, 2)),
        Pogo(SignedEightEight.FromPixelFraction(29, 32), SignedEightEight.FromPixelFraction(9, 2), SignedEightEight.FromPixelFraction(3, 16), SignedEightEight.FromPixelFraction(1, 2)),
        Pogo(SignedEightEight.FromPixelFraction(33, 32), SignedEightEight.FromPixelFraction(37, 8), SignedEightEight.FromPixelFraction(3, 16), SignedEightEight.FromPixelFraction(1, 2)),
    ], RidleyHealthShadeDefinitions.Wounded);

    /// <summary>Ridley below 5400 health, showing the badly wounded colouring.</summary>
    private static NorfairRidleyHealthBand BadlyWounded { get; } = new(healthBelow: 5400, hoverDivisor: 6, grabDivisor: 9,
    [
        Pogo(SignedEightEight.FromPixelFraction(11, 16), SignedEightEight.FromPixelFraction(11, 2), SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.FromPixels(4)),
        Pogo(SignedEightEight.FromPixelFraction(13, 16), SignedEightEight.FromPixelFraction(45, 8), SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.FromPixels(4)),
        Pogo(SignedEightEight.FromPixelFraction(15, 16), SignedEightEight.FromPixelFraction(23, 4), SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.FromPixels(4)),
        Pogo(SignedEightEight.FromPixelFraction(17, 16), SignedEightEight.FromPixelFraction(47, 8), SignedEightEight.FromPixelFraction(1, 4), SignedEightEight.FromPixels(4)),
    ], RidleyHealthShadeDefinitions.BadlyWounded);

    /// <summary>Ridley below 1800 health: his fastest seek and lunge, his highest bounces, and the nearly-dead colouring.</summary>
    private static NorfairRidleyHealthBand NearlyDead { get; } = new(healthBelow: 1800, hoverDivisor: 4, grabDivisor: 6,
    [
        Pogo(SignedEightEight.FromPixelFraction(23, 32), SignedEightEight.FromPixelFraction(13, 2), SignedEightEight.FromPixelFraction(5, 16), SignedEightEight.FromPixels(5)),
        Pogo(SignedEightEight.FromPixelFraction(27, 32), SignedEightEight.FromPixelFraction(53, 8), SignedEightEight.FromPixelFraction(5, 16), SignedEightEight.FromPixels(5)),
        Pogo(SignedEightEight.FromPixelFraction(31, 32), SignedEightEight.FromPixelFraction(27, 4), SignedEightEight.FromPixelFraction(5, 16), SignedEightEight.FromPixels(5)),
        Pogo(SignedEightEight.FromPixelFraction(35, 32), SignedEightEight.FromPixelFraction(55, 8), SignedEightEight.FromPixelFraction(5, 16), SignedEightEight.FromPixels(5)),
    ], RidleyHealthShadeDefinitions.NearlyDead);

    /// <summary>The bands below full health, lowest first, the order Ridley's health is tested against them.</summary>
    internal static IReadOnlyList<NorfairRidleyHealthBand> LowestFirst { get; } = [NearlyDead, BadlyWounded, Wounded];

    /// <summary>
    /// One launch, each a speed an update: its rightward speed and upward speed, then its upward and downward
    /// accelerations.
    /// </summary>
    private static RidleyPogoDefinition Pogo(SignedEightEight rightward, SignedEightEight upwardSpeed, SignedEightEight upward, SignedEightEight downward) =>
        new(new PixelVelocity<RoomPixels>(rightward, -upwardSpeed), upward, downward);
}
