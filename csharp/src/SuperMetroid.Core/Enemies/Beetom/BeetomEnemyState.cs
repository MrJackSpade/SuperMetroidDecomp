using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The state of one Beetom. Its facing is inherited from the previous Beetom in its slot (<see
/// cref="SlotInheritance{TOccupant}"/>).
/// </summary>
public sealed class BeetomEnemyState
{
    /// <summary>The furthest place along the quadratic speed curve a falling or flung Beetom accelerates to: mark 64.</summary>
    internal static readonly QuadraticSpeedCurvePlace MaximumYSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(64);

    /// <summary>
    /// Starts one Beetom, facing the way the previous Beetom in its slot last faced, or left for the slot's first.
    /// </summary>
    /// <remarks>
    /// The Beetom's setup never writes the facing, and the touch response reads it to choose the drain's side
    /// before the Beetom's first action decision writes it, so a fresh Beetom touched at once drains facing its
    /// predecessor's way. Every other value setup writes itself.
    /// </remarks>
    /// <param name="previous">The slot's previous Beetom, or null for the slot's first in the room.</param>
    internal BeetomEnemyState(BeetomEnemyState? previous) => FacingRight = previous?.FacingRight ?? false;

    /// <summary>
    /// The current arc's whole-mark place along the quadratic speed curve, from the curve's start to
    /// <see cref="MaximumYSpeedCurvePlace"/> or the arc's later starting place: rising moves it back by the arc's step and
    /// restarts it at the curve's start at the apex, falling moves it on up to the maximum, so it never leaves that range.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }
    /// <summary>The per-update behaviour, including separate decision and setup updates.</summary>
    public BeetomAction? Action { get; internal set; }
    /// <summary>Idle/crawl countdown in gameplay updates; it expires when a step takes it below zero, not on reaching zero.</summary>
    public int ActionTimer { get; internal set; }
    /// <summary>Remaining controller changes needed to escape attachment, restarted at 64 by the initial touch.</summary>
    public int ButtonCounter { get; internal set; }
    /// <summary>Controller input last observed by initialization or the attached counter; any differing input, including releases, decrements <see cref="ButtonCounter"/>.</summary>
    public ControllerButton PreviousController1Input { get; internal set; }
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BeetomSegment> Playback { get; set; }
    /// <summary>Short-hop starting place: the first multiple of four marks along the curve whose integrated quadratic speeds reach the short hop's height.</summary>
    public QuadraticSpeedCurvePlace InitialShortLeapYSpeedCurvePlace { get; internal set; }
    /// <summary>Long-hop starting place: the first multiple of five marks along the curve whose integrated quadratic speeds reach the long hop's height.</summary>
    public QuadraticSpeedCurvePlace InitialLongLeapYSpeedCurvePlace { get; internal set; }
    /// <summary>Lunge starting place: the first multiple of three marks along the curve whose integrated quadratic speeds reach the lunge's height.</summary>
    public QuadraticSpeedCurvePlace InitialLungeYSpeedCurvePlace { get; internal set; }
    /// <summary>Arc phase: false uses the rising speed-curve half; passing the apex switches to the falling half at the curve's start.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Attachment latch set on first touch and cleared after escape or the common shot reaction; it prevents repeated attachment setup.</summary>
    public bool AttachedToSamus { get; internal set; }
    /// <summary>Whether the Beetom faces right rather than left, inherited from the slot's previous Beetom; collision reverses it, while the flung state moves opposite this facing.</summary>
    public bool FacingRight { get; internal set; }

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
