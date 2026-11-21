using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four-boss statue sequence's timing and shaking (#1415): how long the statue waits once the last boss is
/// released, how it lowers, and the earthquakes the descent and each release start.
/// </summary>
public static partial class TourianStatueSequenceDefinitions
{
    /// <summary>
    /// The updates the statue holds, after the last release ends, before it starts to lower, the last the one that
    /// raises the descent's dust (#2281).
    /// </summary>
    public const int DescentDelay = 301;

    /// <summary>How far the statue lowers each update: a quarter of a pixel.</summary>
    public static readonly SignedSixteenSixteen DescentStep = SignedSixteenSixteen.FromPixelFraction(1, 4);

    /// <summary>The pixels the statue lowers before the descent completes.</summary>
    public const int DescentDistance = 240;

    /// <summary>The statue room's upper scroll cell, the statues' screen, open aligned to its top while the statues stand raised.</summary>
    public static Position16<RoomScreens> UpperScrollCell { get; } = new(0, 0);

    /// <summary>The statue room's lower scroll cell, below the statues, closed until they have lowered.</summary>
    public static Position16<RoomScreens> LowerScrollCell { get; } = new(0, 1);

    /// <summary>The dust puffs the descent raises.</summary>
    public const int DescentDustPuffs = 4;

    /// <summary>The room point, in whole pixels, the descent's dust puffs return to whenever they restart.</summary>
    public static Position16<RoomPixels> DescentDustAnchor { get; } = new(128, 188);

    /// <summary>
    /// The medium up-and-down earthquake, shaking both layers, the descent sustains every update it runs
    /// (<see cref="SuperMetroid.Core.Fx.RoomEarthquake.Sustain"/>), so the shake lasts past its end.
    /// </summary>
    public static readonly EarthquakeShake DescentEarthquake =
        new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocksAndScenery);

    /// <summary>The light up-and-down foreground earthquake a release sustains.</summary>
    public static readonly EarthquakeShake ReleaseEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks);
}
