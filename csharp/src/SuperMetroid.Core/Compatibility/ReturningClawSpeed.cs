namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How fast a ninja Space Pirate's claw flies back (#1846): it speeds up by an eighth of a pixel per update, and a claw
/// that keeps flying back past 255 and 255/256 pixels an update starts again from rest.
/// </summary>
/// <remarks>
/// Consulted once per update of a returning claw, after the claw has moved by the whole pixels of its speed before the
/// change. Evidence: the original keeps the claw's speed as a sixteen-bit count of 256ths of a pixel that its return adds
/// to without a limit; the ninja claw flight check steps a thrown claw for 2200 updates, past that wrap, against that
/// arithmetic.
/// </remarks>
[CompatibilityService("a ninja pirate's returning claw that keeps flying speeds up to 255 pixels an update and then starts again from rest")]
public static class ReturningClawSpeed
{
    /// <summary>How much a returning claw speeds up each update, in 256ths of a pixel.</summary>
    private const int Acceleration = 32;

    /// <summary>The speeds, in 256ths of a pixel per update, a returning claw goes round: just short of 256 pixels.</summary>
    private const int SpeedRange = 256 * 256;

    /// <summary>The speed after one more update of a returning claw at <paramref name="twoHundredFiftySixths"/>.</summary>
    public static int Accelerated(int twoHundredFiftySixths) => (twoHundredFiftySixths + Acceleration) % SpeedRange;
}
