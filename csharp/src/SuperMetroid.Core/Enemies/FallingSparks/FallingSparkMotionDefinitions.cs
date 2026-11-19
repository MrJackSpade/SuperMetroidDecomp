namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>Where a falling spark appears, how it falls and how it bounces off the floor (#2149).</summary>
internal static class FallingSparkMotionDefinitions
{
    /// <summary>How far below its source a spark appears, in pixels.</summary>
    internal const int SpawnBelowSource = 8;

    /// <summary>How much a falling spark's vertical speed gains each falling update: a quarter pixel an update.</summary>
    internal static SignedSixteenSixteen Gravity { get; } = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>The whole-pixel vertical speed a falling spark's speed stops rising at.</summary>
    internal const int TerminalWholeSpeed = 4;

    /// <summary>How many times faster a spark drifts sideways once it has hit the floor.</summary>
    internal const int ImpactDriftScale = 4;

    /// <summary>The vertical speed a spark bounces up at when it hits the floor: half a pixel upward.</summary>
    internal static SignedSixteenSixteen ImpactBounce { get; } = -SignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>How far a spark is lifted off the floor as it hits it, in pixels.</summary>
    internal const int ImpactLift = 2;
}
