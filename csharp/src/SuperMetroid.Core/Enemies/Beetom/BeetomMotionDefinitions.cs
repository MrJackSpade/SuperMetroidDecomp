namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>How a Beetom idles, crawls, hops, lunges, drains Samus and is thrown off (#2149).</summary>
internal static partial class BeetomMotionDefinitions
{
    /// <summary>The furthest place along the quadratic speed curve a falling or flung Beetom accelerates to.</summary>
    internal static readonly QuadraticSpeedCurvePlace MaximumYSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>How close Samus comes horizontally, in pixels and exclusive, before a deciding Beetom lunges at her.</summary>
    internal const int ProximityDistance = 96;

    /// <summary>How many changes of Samus's held buttons it takes to throw off a Beetom draining her.</summary>
    internal const int MashCount = 64;

    /// <summary>How long a Beetom idles before it decides again, in updates, the last the one its wait runs out on.</summary>
    internal const int IdleDuration = 33;

    /// <summary>
    /// How long a Beetom crawls before it decides again, in updates, the last the one its wait runs out on. Each crawl
    /// that runs its course sets the next crawl's length; a crawl that starts with its wait already run out decides again
    /// on its first update.
    /// </summary>
    internal const int CrawlDuration = 65;

    /// <summary>How far ahead of itself a crawling Beetom looks for the floor's edge, in pixels.</summary>
    internal const int LedgeProbeReach = 8;

    /// <summary>How far down a crawling Beetom feels for the floor ahead of it, in pixels.</summary>
    internal const int LedgeProbeDrop = 1;

    /// <summary>How fast a Beetom crawls: a quarter pixel an update.</summary>
    internal static SignedSixteenSixteen CrawlSpeed { get; } = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>How fast a hopping Beetom moves sideways: a quarter pixel an update.</summary>
    internal static SignedSixteenSixteen HopSpeed { get; } = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>How fast a lunging Beetom moves sideways: three pixels an update.</summary>
    internal static SignedSixteenSixteen LungeSpeed { get; } = SignedSixteenSixteen.FromWholePixels(3);

    /// <summary>The speed-curve marks a short hop slows by each update as it rises, and speeds up by as it falls.</summary>
    internal const int ShortHopMarks = 4;

    /// <summary>The speed-curve marks a long hop slows by each update as it rises, and speeds up by as it falls.</summary>
    internal const int LongHopMarks = 5;

    /// <summary>The speed-curve marks a lunge slows by each update as it rises.</summary>
    internal const int LungeRisingMarks = 5;

    /// <summary>The speed-curve marks a lunge speeds up by each update as it falls.</summary>
    internal const int LungeFallingMarks = 3;

    /// <summary>The speed-curve marks a thrown-off Beetom speeds up by each update as it falls.</summary>
    internal const int FlungFallingMarks = 1;

    /// <summary>How far above Samus's middle a draining Beetom holds on, in pixels.</summary>
    internal const int DrainHoldAbove = 4;

    /// <summary>How far Samus throws off a Beetom, in pixels, away from the side it clung to.</summary>
    internal const int EscapeThrow = 16;

    /// <summary>
    /// How far a thrown Beetom that meets a wall is moved back the other way, in pixels: twice the throw, so it lands the
    /// throw's distance on Samus's other side.
    /// </summary>
    internal const int EscapeRebound = 2 * EscapeThrow;

    /// <summary>How fast a dropping Beetom falls: three pixels an update.</summary>
    internal static SignedSixteenSixteen DropSpeed { get; } = SignedSixteenSixteen.FromWholePixels(3);

    /// <summary>How fast a thrown-off Beetom flies sideways: two pixels an update.</summary>
    internal static SignedSixteenSixteen FlungSpeed { get; } = SignedSixteenSixteen.FromWholePixels(2);
}
