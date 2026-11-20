namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Appearance geometry, colours and visibility timing for Wrecked Ship ghost appearances.</summary>
public static class WreckedShipGhostAppearanceDefinitions
{
    /// <summary>
    /// Where the ghost appears around Samus for each way she has been moving: 64 pixels the way she moved on each
    /// axis, and level with her on an axis she held still. A Samus moving up and right is the exception: the ghost
    /// appears level with her, to her right.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusMotion, Offset16<RoomPixels>> SpawnOffsets =
        new Dictionary<SamusMotion, Offset16<RoomPixels>>
        {
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.TowardSmaller)] = new(-64, -64),
            [new(SamusAxisMotion.Still, SamusAxisMotion.TowardSmaller)] = new(0, -64),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.TowardSmaller)] = new(64, 0),
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.Still)] = new(-64, 0),
            [new(SamusAxisMotion.Still, SamusAxisMotion.Still)] = new(0, 0),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.Still)] = new(64, 0),
            [new(SamusAxisMotion.TowardSmaller, SamusAxisMotion.TowardLarger)] = new(-64, 64),
            [new(SamusAxisMotion.Still, SamusAxisMotion.TowardLarger)] = new(0, 64),
            [new(SamusAxisMotion.TowardLarger, SamusAxisMotion.TowardLarger)] = new(64, 64),
        };

    /// <summary>
    /// The timings of a ghost's flicker steps as it appears (<see cref="WreckedShipGhostFlickerStep"/>), in order; flickering
    /// ends when the last step does, leaving the ghost visible. The hidden steps shorten and the shown steps lengthen, each
    /// length held for a pair of hidden and shown steps; the ghost starts hidden. Once a step has shown the ghost, the hidden
    /// steps that follow leave it shown, so the steps' lengths decide when the ghost first appears and when its flicker
    /// ends, which the fade's completion waits for.
    /// </summary>
    public static IReadOnlyList<WreckedShipGhostFlickerTiming> FlickerTimings { get; } =
    [
        new(8, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(8, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(7, WreckedShipGhostFlickerVisibility.Unchanged), new(1, WreckedShipGhostFlickerVisibility.Shown),
        new(7, WreckedShipGhostFlickerVisibility.Unchanged), new(2, WreckedShipGhostFlickerVisibility.Shown),
        new(6, WreckedShipGhostFlickerVisibility.Unchanged), new(2, WreckedShipGhostFlickerVisibility.Shown),
        new(6, WreckedShipGhostFlickerVisibility.Unchanged), new(3, WreckedShipGhostFlickerVisibility.Shown),
        new(5, WreckedShipGhostFlickerVisibility.Unchanged), new(3, WreckedShipGhostFlickerVisibility.Shown),
        new(5, WreckedShipGhostFlickerVisibility.Unchanged),
    ];

    /// <summary>
    /// How long a new ghost waits before it watches Samus, in updates: as long as it waits to reappear
    /// (<see cref="ReappearanceDelay"/>) and a further <see cref="FirstWatchExtraDelay"/>.
    /// </summary>
    internal const int InitialDelay = ReappearanceDelay + FirstWatchExtraDelay;

    /// <summary>How much longer a new ghost waits before it first watches Samus than a faded one waits to reappear, in updates.</summary>
    private const int FirstWatchExtraDelay = 160;

    /// <summary>
    /// The updates from the ghost's appearance to its flicker's first step: the first update it brightens starts the step.
    /// </summary>
    internal const int FlickerFirstStepDelay = 1;

    /// <summary>How long the ghost stays visible once its appearance starts bobbing, in updates.</summary>
    internal const int VisibleDuration = 120;

    /// <summary>How long a ghost that has faded away stays hidden before it watches Samus again, in updates (#2182).</summary>
    internal const int ReappearanceDelay = 120;

    /// <summary>How long Samus stays nearly still, within <see cref="StillMargin"/>, before the ghost appears, in updates.</summary>
    internal const int StillPositionDuration = 64;

    /// <summary>How many watching updates Samus keeps her facing before the ghost appears.</summary>
    internal const int SteadyFacingDuration = 16;

    /// <summary>The vertical speed the visible ghost's bob starts at, one pixel an update.</summary>
    internal static SignedSixteenSixteen BobStartSpeed { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The bobbing ghost's vertical acceleration, three thirty-seconds of a pixel an update.</summary>
    internal static SignedSixteenSixteen BobAcceleration { get; } = SignedSixteenSixteen.FromPixelFraction(3, 32);

    /// <summary>How far Samus may move on each axis, in pixels, and still count as nearly still.</summary>
    internal static Offset16<RoomPixels> StillMargin { get; } = new(1, 1);
}
