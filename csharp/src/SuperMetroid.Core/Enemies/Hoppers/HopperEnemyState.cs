using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper. Its hops follow places along the quadratic speed curve: replacing them with gravity fields would
/// hide the quadratic speed curve that defines the jump arc.
/// </summary>
public sealed class HopperEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal HopperEnemyState(HopperAction action) => Action = action;

    /// <summary>The current hopper AI step, advanced one setup stage an update.</summary>
    public HopperAction Action { get; internal set; }

    /// <summary>
    /// The whole-mark place along the quadratic speed curve of the current vertical step. It moves back by
    /// <see cref="YSpeedCurvePlaceStep"/> while moving away from the surface, where a place before the curve's start
    /// is stored as the curve's start at the apex, and on by them up to mark 64 while returning, so it stays within the
    /// curve's marks; a place outside them is rejected.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace
    {
        get;
        internal set => field = value.IsNegative || value.WholeMarks >= QuadraticSpeedCurveDefinitions.MarkCount
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A hop's place lies outside the quadratic speed curve.")
            : value;
    }

    /// <summary>
    /// The way the hopper hops (#2338): toward Samus as each hop starts, and turned around when a hop strikes a wall or,
    /// moving away from its surface, strikes the room above or below.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>The size of the hop being prepared, which the hop-size choice draws (#2338).</summary>
    internal HopperHopSize HopSize { get; set; }

    /// <summary>
    /// Whole-pixel horizontal velocity, with no fraction of a pixel: <see cref="HopperHopDefinitions.HorizontalSpeed"/>
    /// toward <see cref="Heading"/> (#2338).
    /// </summary>
    public int XVelocity => Heading == HorizontalDirection.Right ? HopperHopDefinitions.HorizontalSpeed : -HopperHopDefinitions.HorizontalSpeed;

    /// <summary>
    /// The step along the speed curve, three marks for small hops or four for big hops, taken back while moving away from
    /// the attachment surface and on while returning.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlaceStep { get; internal set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HopperSegment> Playback { get; set; }
    /// <summary>True selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Latch set by the idle animation's ready cue and consumed by WaitToHop; authored animation cadence, not a separate jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The kind of hopper its definition is, which selects its animations.</summary>
    public HopperKind Kind { get; internal set; }

    /// <summary>The surface its placement mounts it on, which decides which way is up for its hops (#2338).</summary>
    internal HopperSurface Surface { get; set; }
}
