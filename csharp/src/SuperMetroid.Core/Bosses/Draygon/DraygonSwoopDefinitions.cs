namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon's swoops speed up as his eye is hit, and when his arms reach out on them (#2300).</summary>
internal static partial class DraygonSwoopDefinitions
{
    /// <summary>The swoop acceleration a fight starts with and a tail-whip hit restores: three thirty-seconds of a pixel per frame per frame downward.</summary>
    internal static readonly SignedEightEight InitialAcceleration = SignedEightEight.FromPixelFraction(3, 32);

    /// <summary>The swoop acceleration each eye hit adds, a thirty-second of a pixel per frame per frame, while below the limit.</summary>
    internal static readonly SignedEightEight ShotAccelerationStep = SignedEightEight.FromPixelFraction(1, 32);

    /// <summary>The swoop acceleration, five eighths of a pixel per frame per frame, an eye hit may not reach.</summary>
    internal static readonly SignedEightEight AccelerationLimit = SignedEightEight.FromPixelFraction(5, 8);

    /// <summary>
    /// The updates before the bottom of a swoop at which Draygon's arms reach out on the way down, and the updates after it
    /// at which they return to rest on the climb: each update of the swoop follows one entry of its path.
    /// </summary>
    internal const int ArmsReachUpdatesFromBottom = 26;


    /// <summary>How fast Draygon swoops in from the side of the arena to spit his goop: one pixel an update.</summary>
    internal static readonly SignedSixteenSixteen GoopApproachSpeed = SignedSixteenSixteen.FromWholePixels(1);
}
