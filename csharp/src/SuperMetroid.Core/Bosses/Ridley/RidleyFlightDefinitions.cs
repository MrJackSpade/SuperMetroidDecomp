namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How both Ridleys swoop (#2171, #2229).</summary>
internal static class RidleyFlightDefinitions
{
    /// <summary>How far the swoop speed moves toward its target each update: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight SwoopSpeedStep = SignedEightEight.FromPixelFraction(1, 8);
}
