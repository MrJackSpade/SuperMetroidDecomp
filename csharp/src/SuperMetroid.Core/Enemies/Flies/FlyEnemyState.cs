using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The state of one Mellow, Mella or Memu.</summary>
public sealed class FlyEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal FlyEnemyState(FlyAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FlySegment> Playback { get; set; }

    /// <summary>
    /// The fly's dive and retreat clock (#2281): it counts the dive's updates, then the retreat's updates left, the dive's
    /// and <see cref="FlyMotionDefinitions.RetreatBeyondDive"/> more, to zero, and then the circling updates left from
    /// <see cref="FlyMotionDefinitions.AttackCooldown"/> before another dive.
    /// </summary>
    public int RetreatTimer { get; internal set; }

    /// <summary>
    /// Signed 1/256-pixel attack/retreat velocity in room pixels per actor update: X sampled from Samus's attack-start angle
    /// and retained during retreat; Y, whose sign reverses when the saved target height is crossed.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Samus's whole room-pixel Y captured when the attack begins. It determines the turnaround rather than tracking Samus's later movement.</summary>
    public int TargetY { get; internal set; }

    /// <summary>The circular flight's phase, advanced sixteen 256ths of a turn per circling update and wrapping after a full turn.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>The current circling, attacking or retreating behaviour.</summary>
    public FlyAction Action { get; internal set; }

    /// <summary>One circling update's phase advance, one way or the other, wrapping after a full turn.</summary>
    /// <param name="clockwise">Whether the fly circles clockwise.</param>
    internal void AdvanceCircle(bool clockwise) =>
        Angle = Angle.TurnedBySteps(clockwise ? FlyMotionDefinitions.CircleTurnSteps : -FlyMotionDefinitions.CircleTurnSteps);
}
