using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The state of one Beetom. Its facing is inherited from the previous Beetom in its place (<see
/// cref="PlaceInheritance{TOccupant}"/>).
/// </summary>
public sealed class BeetomEnemyState
{
    /// <summary>The furthest place along the quadratic speed curve a falling or flung Beetom accelerates to: mark 64.</summary>
    internal static readonly QuadraticSpeedCurvePlace MaximumYSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>
    /// Starts one Beetom, facing the way the previous Beetom in its place last faced, or left for the place's first.
    /// </summary>
    /// <remarks>
    /// The Beetom's setup never writes the facing, and the touch response reads it to choose the drain's side
    /// before the Beetom's first action decision writes it, so a fresh Beetom touched at once drains facing its
    /// predecessor's way. Every other value setup writes itself.
    /// </remarks>
    /// <param name="previous">The place's previous Beetom, or null for the place's first in the room.</param>
    /// <param name="action">The action the Beetom starts in.</param>
    internal BeetomEnemyState(BeetomEnemyState? previous, BeetomAction action)
    {
        Facing = previous?.Facing ?? HorizontalDirection.Left;
        Action = action;
    }

    /// <summary>
    /// The current arc's whole-mark place along the quadratic speed curve, from the curve's start to
    /// <see cref="MaximumYSpeedCurvePlace"/> or the arc's later starting place: rising moves it back by the arc's step and
    /// restarts it at the curve's start at the apex, falling moves it on up to the maximum, so it never leaves that range.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }
    /// <summary>The per-update behaviour, including separate decision and setup updates.</summary>
    public BeetomAction Action { get; internal set; }
    /// <summary>
    /// The updates left in the idle or crawl, the one it runs out on included: a step that takes it to zero ends the idle or
    /// crawl, and one at zero ends it on its next step (#2281).
    /// </summary>
    public int ActionTimer { get; internal set; }
    /// <summary>Remaining controller changes needed to escape attachment, restarted at 64 by the initial touch.</summary>
    public int ButtonCounter { get; internal set; }
    /// <summary>The player's actions last observed by initialization or the attached counter; any differing actions, including releases, decrement <see cref="ButtonCounter"/>.</summary>
    public GameplayAction LastSeenPlayerActions { get; internal set; }
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BeetomSegment> Playback { get; set; }
    /// <summary>Arc phase: false uses the rising speed-curve half; passing the apex switches to the falling half at the curve's start.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Attachment latch set on first touch and cleared after escape or the common shot reaction; it prevents repeated attachment setup.</summary>
    public bool AttachedToSamus { get; internal set; }
    /// <summary>The way the Beetom faces, inherited from the place's previous Beetom; collision reverses it, while the flung state moves opposite this facing.</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// The way the current action goes, for an action that goes one way (#2277): the crawl's, hop's or lunge's direction, or
    /// the side of Samus a drain holds. It is chosen as the action is, and need not be <see cref="Facing"/>.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>Starts <paramref name="action"/> going toward <paramref name="heading"/>.</summary>
    internal void Begin(BeetomAction action, HorizontalDirection heading)
    {
        Action = action;
        Heading = heading;
    }

    /// <summary>
    /// One rising update of an arc: moves the place back by <paramref name="marks"/> marks; once that would pass the
    /// curve's start the arc has reached its apex, so the place restarts at the curve's start and the arc falls.
    /// </summary>
    /// <param name="marks">The arc's rising step, three to five marks.</param>
    internal void StepRisingSpeed(int marks)
    {
        YSpeedCurvePlace -= QuadraticSpeedCurvePlace.FromMarks(marks);
        if (!YSpeedCurvePlace.IsNegative)
            return;
        YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        Falling = true;
    }

    /// <summary>One falling update: moves the place on by <paramref name="marks"/> marks, holding at <see cref="MaximumYSpeedCurvePlace"/>.</summary>
    /// <param name="marks">The arc's falling step, one to five marks.</param>
    internal void StepFallingSpeed(int marks) =>
        YSpeedCurvePlace = QuadraticSpeedCurvePlace.Min(MaximumYSpeedCurvePlace, YSpeedCurvePlace + QuadraticSpeedCurvePlace.FromMarks(marks));
}
