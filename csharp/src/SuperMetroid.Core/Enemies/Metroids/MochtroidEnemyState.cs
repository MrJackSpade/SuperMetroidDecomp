using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of a Mochtroid: its 1/65536-pixel steering, its shake timer and its one-update movement selector.
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>Signed 1/65536-pixel velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Shake countdown, used only by the shipped-but-unreferenced shake mode.</summary>
    public int ShakeTimer { get; internal set; }

    /// <summary>Gets the one-update movement selector cleared before each main-AI dispatch.</summary>
    public MochtroidMovementMode MovementMode { get; internal set; }

    /// <summary>Parallel value used by the animation change detector: the installed animation, or none for the cleared value.</summary>
    internal MochtroidSegment? InstalledAnimation { get; set; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MochtroidSegment> Playback { get; set; }

    /// <summary>
    /// Attached updates counted since the last contact damage, 0 through 65535: contact damage occurs once the count,
    /// compared as a signed difference, reaches the damage period. Updates with Samus's own contact damage count
    /// without the check, so the count can grow past 32767 and wrap (<see cref="CountAttachedUpdate"/>).
    /// </summary>
    public int AttachmentDamageTimer { get; internal set; }

    /// <summary>Counts one attached update, wrapping after 65535 as the game's counter does.</summary>
    internal void CountAttachedUpdate() =>
        AttachmentDamageTimer = WrappingCounter.IncrementCount(AttachmentDamageTimer);

    /// <summary>Whether the count has reached <paramref name="period"/>, compared as the game's signed difference.</summary>
    /// <param name="period">The damage period in attached updates.</param>
    internal bool HasReachedDamagePeriod(int period) => !WrappingCounter.IsBehind(AttachmentDamageTimer, period);
}
