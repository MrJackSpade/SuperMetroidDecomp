using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of a Mochtroid: its 1/65536-pixel steering and its one-update movement selector.
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>Signed 1/65536-pixel velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>How the Mochtroid moves this update, cleared before each run of its main AI.</summary>
    public MochtroidMovementMode MovementMode { get; internal set; }

    /// <summary>
    /// The animation the enemy plays and where it stands in it (#1600), against which the Mochtroid tells whether its
    /// movement asks for a new one (#2145).
    /// </summary>
    internal AnimationPlayback<MochtroidSegment> Playback { get; set; }

    /// <summary>
    /// Attached updates counted since the last contact damage: contact damage occurs once the count reaches the damage
    /// period. An update in which Samus attacks with her body counts without the check, but that touch kills the
    /// Mochtroid outright, every body attack dealing at least its whole health, so the count never runs far past the
    /// period.
    /// </summary>
    public int AttachmentDamageTimer { get; internal set; }

    /// <summary>Counts one attached update.</summary>
    internal void CountAttachedUpdate() => AttachmentDamageTimer++;

    /// <summary>Whether the count has reached <paramref name="period"/>.</summary>
    /// <param name="period">The damage period in attached updates.</param>
    internal bool HasReachedDamagePeriod(int period) => AttachmentDamageTimer >= period;
}
