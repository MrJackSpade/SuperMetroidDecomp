using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The state of a hopper: the step it runs and its place along the quadratic speed curve that shapes each hop's arc.
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
    public int XVelocity => HopperHopDefinitions.HorizontalSpeed.Toward(Heading);

    /// <summary>
    /// The step along the speed curve, three marks for small hops or four for big hops, taken back while moving away from
    /// the attachment surface and on while returning.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlaceStep { get; internal set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<HopperSegment> Playback { get; set; }
    /// <summary>
    /// Whether the landing animation has readied the next hop: set by its ready cue and cleared by
    /// <see cref="HopperAction.WaitToHop"/> as it hops again, so authored animation cadence, not a separate jump timer,
    /// controls the next hop. It stays a mark beside the action rather than a phase of it (#2439): a hopper plays its
    /// landing animation from its placement while its first updates prepare its first hop, and a hurt state that holds those
    /// updates back lets the animation ready a hop before the hopper waits for one; that hop then follows the hopper's first
    /// landing at once.
    /// </summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>The kind of hopper its definition is, which selects its animations.</summary>
    public HopperKind Kind { get; internal set; }
}
