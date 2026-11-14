using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>A Ki-Hunter's wings' state: following the body, then, once detached, their orbit and falling arc.</summary>
public sealed class KiHunterWingState
{
    /// <summary>The animation the wings play and where they stand in it (#1600).</summary>
    internal AnimationPlayback<KiHunterSegment> Playback { get; set; }

    /// <summary>Main-AI step: following the body, or detached.</summary>
    public KiHunterAction? Action { get; internal set; }

    /// <summary>
    /// The detached wings' place along the shared quadratic speed curve: reset to <see cref="DetachedSpeedReset"/> at each
    /// arc and lowered by a mark and a half per update, landing on mark one when the step would go below the start,
    /// so it stays on the curve.
    /// </summary>
    public QuadraticSpeedCurvePlace DetachedSpeedCurvePlace { get; internal set; }

    /// <summary>
    /// The detached wings' quadratic speed sum, accumulated at detachment until it reaches the detached-speed threshold.
    /// Detachment never resets it: it continues from zero or the earlier sum. Its sum wraps as the game's counter does,
    /// through <see cref="AccumulateDetachedSpeed"/>.
    /// </summary>
    public int DetachedSpeedAccumulator
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }

    /// <summary>
    /// The detached wings' arc angle, which wraps over the whole turn: seeded below zero for each arc and turned each
    /// update by the coarse speed of its place on the speed curve. The arc changes when the angle crosses a quarter turn.
    /// </summary>
    public RotationAngle WingArcAngle { get; internal set; }

    /// <summary>The detached wings' movement, alternating between non-colliding and collision-bearing arcs.</summary>
    public KiHunterWingMovement? WingMovement { get; internal set; }

    /// <summary>
    /// The reset place along the speed curve, computed at detachment by stepping a mark and a half at a time until the
    /// quadratic speed sum reaches its threshold, and reapplied at each arc change; on the curve.
    /// </summary>
    public QuadraticSpeedCurvePlace DetachedSpeedReset { get; internal set; }

    /// <summary>The signed pixel correction for the non-colliding arc, calculated seven eighths of a turn round and at radius 48.</summary>
    public Offset16<RoomPixels> OrbitOffset { get; internal set; }

    /// <summary>The signed pixel correction for the collision-bearing arc, calculated five eighths of a turn round and at radius 48.</summary>
    public Offset16<RoomPixels> FallingArcOffset { get; internal set; }

    /// <summary>The room-pixel anchor used with an arc-specific correction; recaptured whenever the detached arc changes.</summary>
    public Position16<RoomPixels> OrbitCenter { get; internal set; }

    /// <summary>The wings' room position saved at detachment, where the first orbit is centred and where the collision-bearing arc deletes them.</summary>
    public Position16<RoomPixels> SavedWingPosition { get; internal set; }

    /// <summary>Adds one speed of the quadratic speed curve to <see cref="DetachedSpeedAccumulator"/>, wrapping as the game's counter does.</summary>
    internal void AccumulateDetachedSpeed(int step) => DetachedSpeedAccumulator = WrappingCounter.AddCount(DetachedSpeedAccumulator, step);
}
