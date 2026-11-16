namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Exact bounded motion policies for the Landing Site gunship.</summary>
internal static class GunshipMotionDefinitions
{

    /// <summary>The landing brake's signed whole-pixel Y delta for each of its 17 frames: +1 for frames 0..5, zero for 6..10, and -1 for 11..16.</summary>
    /// <remarks>
    /// These are explicit six/five/six braking intervals, not a sampled physical acceleration curve. The landing adds the
    /// signed whole-pixel delta with coordinate wrapping, then advances.
    /// </remarks>
    internal static readonly int[] LandingBrakeYDeltas = [1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1];

    /// <summary>The idle bob's four phases: timer 16 and signed delta +1,-1,-1,+1.</summary>
    /// <remarks>
    /// Endpoint phases move down, middle phases move up. The ship's hover counts each phase down and wraps
    /// from the last phase to the first.
    /// </remarks>
    internal static readonly GunshipIdleBobDefinition[] IdleBobs = [new(16, 1), new(16, -1), new(16, -1), new(16, 1)];

    /// <summary>Above this Y the descending ship moves 4.5 pixels an update; below it, 2.5.</summary>
    internal const int SlowDescentY = 768;

    /// <summary>The descending ship's speed above <see cref="SlowDescentY"/>: four and a half pixels an update.</summary>
    internal static readonly SignedSixteenSixteen FastDescentSpeed = SignedSixteenSixteen.FromSixteenthsOfPixel(72);

    /// <summary>The descending ship's speed below <see cref="SlowDescentY"/>: two and a half pixels an update.</summary>
    internal static readonly SignedSixteenSixteen SlowDescentSpeed = SignedSixteenSixteen.FromSixteenthsOfPixel(40);

    /// <summary>The landed top hull's Y, where the descent clamps.</summary>
    internal const int LandedTopY = 1119;

    /// <summary>The landed bottom hull's Y.</summary>
    internal const int LandedBottomY = 1159;

    /// <summary>The landed entrance pad's Y.</summary>
    internal const int LandedPadY = 1118;

    /// <summary>The landed ship's X, where Samus stands to board.</summary>
    internal const int LandedX = 1152;

    /// <summary>How far above its placement the landed top hull rests, in pixels.</summary>
    internal const int LandedTopHullRaise = 25;

    /// <summary>How far below its placement the landed bottom hull rests, in pixels.</summary>
    internal const int LandedBottomHullDrop = 15;

    /// <summary>How far above Samus the top hull sits while she rides in the gunship, in pixels.</summary>
    internal const int TopHullAboveSamus = 17;

    /// <summary>How far above the top hull the entrance pad sits, in pixels.</summary>
    internal const int PadAboveTopHull = 1;

    /// <summary>How far below Samus the bottom hull sits while she rides in the gunship, in pixels.</summary>
    internal const int BottomHullBelowSamus = 23;

    /// <summary>The escaping ship's starting upward speed: two pixels an update.</summary>
    internal static readonly SignedEightEight EscapeStartVelocity = SignedEightEight.FromPixels(2);

    /// <summary>The escaping ship's acceleration each update: a quarter pixel an update.</summary>
    internal static readonly SignedEightEight EscapeAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The whole pixels an update the escaping speed's whole part reaches when the cap applies: ten.</summary>
    internal const int EscapeVelocityCapTriggerPixels = 10;

    /// <summary>The escaping ship's capped upward speed: nine pixels an update.</summary>
    internal static readonly SignedEightEight EscapeVelocityCap = SignedEightEight.FromPixels(9);
}
