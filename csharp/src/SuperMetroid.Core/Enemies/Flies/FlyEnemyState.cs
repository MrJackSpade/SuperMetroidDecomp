using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The state of one Mellow, Mella or Memu.</summary>
public sealed class FlyEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal FlyEnemyState(FlyAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FlySegment> Playback { get; set; }

    /// <summary>The updates the fly's dive has lasted (#2369), counted from the update it starts on.</summary>
    public ElapsedUpdates Dive { get; } = new();

    /// <summary>
    /// The retreat (#2369): its dive's updates and <see cref="FlyMotionDefinitions.RetreatBeyondDive"/> more, the last the
    /// one it turns back to circling on.
    /// </summary>
    public UpdateCountdown RetreatUpdatesLeft { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The circling updates left before the fly can dive again (#2369): <see cref="FlyMotionDefinitions.AttackCooldown"/>
    /// after each retreat, none before its first dive.
    /// </summary>
    public UpdateCountdown CooldownUpdatesLeft { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Attack and retreat velocity in room pixels per update: X sampled from Samus's attack-start angle
    /// and retained during retreat; Y, whose sign reverses when the saved target height is crossed.
    /// </summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>Samus's whole room-pixel Y captured when the attack begins. It determines the turnaround rather than tracking Samus's later movement.</summary>
    public int TargetY { get; internal set; }

    /// <summary>The circular flight's phase, advanced a sixteenth of a turn each circling update and wrapping after a full turn.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>The current circling, attacking or retreating behaviour.</summary>
    public FlyAction Action { get; internal set; }

    /// <summary>The way the fly circles (#2277): it starts clockwise and reverses after each full turn.</summary>
    public RotationDirection Rotation { get; internal set; }

    /// <summary>One circling update's phase advance the way the fly circles, wrapping after a full turn.</summary>
    internal void AdvanceCircle() =>
        Angle = Angle.TurnedBySteps(Rotation == RotationDirection.Clockwise ? FlyMotionDefinitions.CircleTurnSteps : -FlyMotionDefinitions.CircleTurnSteps);
}
