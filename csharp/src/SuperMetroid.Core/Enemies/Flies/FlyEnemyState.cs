using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The state of one Mellow, Mella or Memu.</summary>
public sealed class FlyEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FlySegment> Playback { get; set; }

    /// <summary>Counts up per attack update, down below zero during retreat, and then down from 24 to gate another idle attack.</summary>
    public int RetreatTimer { get; internal set; }

    /// <summary>
    /// Signed 8.8 attack/retreat velocity in room pixels per actor update: X sampled from Samus's attack-start angle
    /// and retained during retreat; Y, whose sign reverses when the saved target height is crossed.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Samus's whole room-pixel height captured when the attack begins; only Y is meaningful. It determines the turnaround rather than tracking Samus's later movement.</summary>
    public Position16<RoomPixels> TargetHeight { get; internal set; }

    /// <summary>The circular flight's phase, advanced sixteen table units per circling update and wrapping after a full turn.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>The current circling, attacking or retreating behaviour.</summary>
    public FlyAction? Action { get; internal set; }

    /// <summary>One circling update's phase advance: sixteen table units one way or the other, wrapping after a full turn.</summary>
    /// <param name="clockwise">Whether the fly circles clockwise.</param>
    internal void AdvanceCircle(bool clockwise) => Angle = Angle.TurnedBySteps(clockwise ? 16 : -16);
}
