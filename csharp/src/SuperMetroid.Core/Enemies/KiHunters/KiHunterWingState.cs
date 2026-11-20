using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>A Ki-Hunter's wings' state: following the body, then, once detached, their orbit and falling arc.</summary>
public sealed class KiHunterWingState
{
    /// <summary>Starts the wings in <paramref name="action"/>.</summary>
    internal KiHunterWingState(KiHunterWingAction action) => Action = action;

    /// <summary>The animation the wings play and where they stand in it (#1600).</summary>
    internal AnimationPlayback<KiHunterSegment> Playback { get; set; }

    /// <summary>Main-AI step: following the body, or detached.</summary>
    public KiHunterWingAction Action { get; internal set; }

    /// <summary>
    /// The detached wings' place along the shared quadratic speed curve: reset at each arc to where the wings' climb starts
    /// (<see cref="KiHunterMotionDefinitions.DetachedWingClimb"/>, #2474) and lowered by its step each update,
    /// landing on the curve's first mark when the step would go below the start, so it stays on the curve.
    /// </summary>
    public QuadraticSpeedCurvePlace DetachedSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The detached wings' arc angle around the turn: started at each arc's own starting angle, and turned each update by
    /// the coarse speed of its place on the speed curve. The arc changes
    /// when the angle crosses three quarters of a turn.
    /// </summary>
    public RotationAngle WingArcAngle { get; internal set; }

    /// <summary>
    /// The detached wings' movement, alternating between non-colliding and collision-bearing arcs; detached wings start
    /// with the orbit.
    /// </summary>
    public KiHunterWingMovement WingMovement { get; internal set; } = KiHunterWingMovement.Orbit;

    /// <summary>
    /// The room-pixel anchor used with an arc-specific correction: where the wings detached, then recaptured whenever the
    /// detached arc changes.
    /// </summary>
    public Position16<RoomPixels> OrbitCenter { get; internal set; }

    /// <summary>
    /// Where the wings detached (#2474): their first orbit's centre, and where the falling arc puts them back as it deletes
    /// them on landing. The deleted wings keep that position until the next update clears their place, as the game's do.
    /// </summary>
    public Position16<RoomPixels> DetachedAt { get; internal set; }

}
