namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Norfair Ridley's four health bands (#1934). Each lower band seeks faster, lunges harder and bounces higher, and the
/// three below full health each show their own health shade.
/// </summary>
internal static class NorfairRidleyHealthBandDefinitions
{
    /// <summary>Ridley from full health down to 9000: his slowest seek and lunge, low bounces, and his own colours.</summary>
    internal static NorfairRidleyHealthBand Unhurt { get; } = new(healthBelow: null, hoverDivisor: 12, grabDivisor: 15,
        [Pogo(160, 800, 32, 64), Pogo(192, 896, 32, 64), Pogo(224, 928, 32, 64), Pogo(256, 960, 32, 64)], healthShade: null);

    /// <summary>Ridley below 9000 health, showing the first health shade.</summary>
    internal static NorfairRidleyHealthBand Wounded { get; } = new(healthBelow: 9000, hoverDivisor: 8, grabDivisor: 13,
        [Pogo(168, 992, 48, 128), Pogo(200, 1088, 48, 128), Pogo(232, 1152, 48, 128), Pogo(264, 1184, 48, 128)], healthShade: 0);

    /// <summary>Ridley below 5400 health, showing the second health shade.</summary>
    internal static NorfairRidleyHealthBand BadlyWounded { get; } = new(healthBelow: 5400, hoverDivisor: 6, grabDivisor: 9,
        [Pogo(176, 1408, 64, 1024), Pogo(208, 1440, 64, 1024), Pogo(240, 1472, 64, 1024), Pogo(272, 1504, 64, 1024)], healthShade: 1);

    /// <summary>Ridley below 1800 health: his fastest seek and lunge, his highest bounces, and the last health shade.</summary>
    internal static NorfairRidleyHealthBand NearlyDead { get; } = new(healthBelow: 1800, hoverDivisor: 4, grabDivisor: 6,
        [Pogo(184, 1664, 80, 1280), Pogo(216, 1696, 80, 1280), Pogo(248, 1728, 80, 1280), Pogo(280, 1760, 80, 1280)], healthShade: 2);

    /// <summary>The bands below full health, lowest first, the order Ridley's health is tested against them.</summary>
    internal static IReadOnlyList<NorfairRidleyHealthBand> LowestFirst { get; } = [NearlyDead, BadlyWounded, Wounded];

    /// <summary>
    /// One launch, in 256ths of a pixel an update: its rightward speed and upward speed, then its upward and downward
    /// accelerations.
    /// </summary>
    private static RidleyPogoDefinition Pogo(int horizontal, int upwardSpeed, int upward, int downward) =>
        new(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(horizontal, -upwardSpeed),
            SignedEightEight.FromTwoHundredFiftySixthsOfPixel(upward), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(downward));
}
