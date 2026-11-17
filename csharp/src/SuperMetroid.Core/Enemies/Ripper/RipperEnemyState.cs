using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one Ripper, grapple Ripper or jet Ripper: its flying animation, its patrol velocity, the speed it spawns with,
/// and grapple Ripper's two patrol bounds. All are the enemy's own: setup starts the animation for every variant and writes
/// the bounds for the grapple Ripper, the only variant that reads them, so nothing a previous occupant of the place left
/// reaches a new one.
/// </summary>
public sealed class RipperEnemyState
{
    /// <summary>The Ripper-family typed flying loop (#641), started by setup and restarted at each reversal.</summary>
    internal AnimationPlayback<RipperSegment> Playback { get; set; }

    /// <summary>The way the enemy flies (#2278).</summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>Signed 1/65536-pixel horizontal velocity per update: the spawn speed toward <see cref="Heading"/>.</summary>
    public SignedSixteenSixteen XVelocity => Heading == HorizontalDirection.Right ? Speeds.Positive : Speeds.Negative;

    /// <summary>The spawn settings' speed, set by initialization and read back when the velocity is loaded.</summary>
    public EnemyLinearSpeed Speeds { get; internal set; }

    /// <summary>
    /// grapple Ripper's exclusive left patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by which way the wrapped difference lies.
    /// </summary>
    public int MinimumXPosition { get; internal set; }

    /// <summary>
    /// grapple Ripper's inclusive right patrol reversal threshold, a room X coordinate in pixels from its spawn settings. The
    /// patrol compares it by which way the wrapped difference lies.
    /// </summary>
    public int MaximumXPosition { get; internal set; }
}
