using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The state of one Tripper or suspensor platform: the height it rises back to, its vertical acceleration, and the
/// movement and art it has selected. It reads its patrol speed and its acceleration's cap from its spawn settings as it
/// moves (#2439).
/// </summary>
public sealed class PlatformEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal PlatformSpawnSettings Settings { get; }

    /// <summary>Creates the state of an enemy whose entry told its species <paramref name="settings"/>.</summary>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal PlatformEnemyState(PlatformSpawnSettings settings) => Settings = Ensure.NotNull(settings);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PlatformSegment> Playback { get; set; }

    /// <summary>
    /// The room Y a depressed platform rises back to, <see cref="PlatformMotionDefinitions.RestingDepthBelowPlacement"/> below its population position. Rising stops once the
    /// platform's whole Y lies above it.
    /// </summary>
    public int RestingY { get; internal set; }

    /// <summary>
    /// Whole-mark place along the shared quadratically increasing enemy speeds, from the curve's start to as many marks on
    /// as the spawn settings' vertical acceleration updates: it starts at the curve's start, moves on one mark per
    /// accelerating update and is clamped at that cap, so it never leaves that range.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>The patrol direction: initialized from its spawn movement and changed by wall collisions or animation cues.</summary>
    public HorizontalDirection XMovement { get; internal set; }

    /// <summary>
    /// The art the platform last chose for its vertical movement, or none before its first choice (#2145). Choosing the same
    /// art again starts nothing, even where a wall has since started the still art inside a moving phase, so that still
    /// art stays until the platform rests; a first choice starts its art afresh.
    /// </summary>
    public PlatformVerticalArt? VerticalArt { get; internal set; }

    /// <summary>The previous actor update's rider-selected vertical movement; a change restarts the acceleration.</summary>
    public PlatformVerticalMovement PreviousYMovement { get; internal set; }

    /// <summary>Which platform this is, the Tripper or the suspensor platform, whose art differs.</summary>
    internal PlatformKind Kind { get; init; }
}
