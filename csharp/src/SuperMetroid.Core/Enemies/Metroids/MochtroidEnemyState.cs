using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of a Mochtroid: its steering and its one-update movement selector.
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>The Mochtroid's velocity in room pixels per update, positive right and down.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>How the Mochtroid moves this update, cleared before each run of its main AI.</summary>
    public MochtroidMovementMode MovementMode { get; internal set; }

    /// <summary>
    /// The animation the enemy plays and where it stands in it (#1600), against which the Mochtroid tells whether its
    /// movement asks for a new one (#2145).
    /// </summary>
    internal AnimationPlayback<MochtroidSegment> Playback { get; set; }

    /// <summary>
    /// The drain period between contact damages, counted each attached update: contact damage occurs once it is over,
    /// and starts it again. An update in which Samus attacks with her body counts it without the check, but that touch
    /// kills the Mochtroid outright, every body attack dealing at least its whole health.
    /// </summary>
    public UpdateCountdown DrainPeriod { get; internal set; } = UpdateCountdown.Lasting(MochtroidBehaviorDefinitions.AttachedDrainPeriod);
}
