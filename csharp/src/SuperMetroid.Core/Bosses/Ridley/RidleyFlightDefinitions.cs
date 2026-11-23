namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How both Ridleys swoop (#2171, #2229).</summary>
internal static partial class RidleyFlightDefinitions
{
    /// <summary>How far the swoop speed moves toward its target each update: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight SwoopSpeedStep = SignedEightEight.FromPixelFraction(1, 8);


    /// <summary>The speed Ridley starts rising at as he lifts off: a pixel and three eighths an update.</summary>
    internal static readonly SignedEightEight LiftoffRise = SignedEightEight.FromPixelFraction(-11, 8);

    /// <summary>The heading a swoop climbs toward, turned the swooping way: fifteen thirty-seconds of a turn.</summary>
    internal static readonly RotationAngle SwoopClimbTarget = RotationAngle.FromSteps(120);
}
