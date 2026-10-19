using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of a Mochtroid ($A3:A77D-$A9A8), with its three parallel extension-array words, so the attachment clock
/// and 16.16 steering are easy to inspect. The values the cartridge keeps in the enemy's common record are emptied with
/// it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>Signed 16.16 velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Variable E; used only by the shipped-but-unreferenced shake mode.</summary>
    public NativeCountdown ShakeTimer { get; internal set; }

    /// <summary>Gets the one-update movement selector cleared before each main-AI dispatch.</summary>
    public MochtroidMovementMode MovementMode { get; internal set; }

    /// <summary>$7E:7802 parallel word used by the instruction-list change detector: the installed animation, or none for the cleared word.</summary>
    internal MochtroidProgram? InstalledProgram { get; set; }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MochtroidProgram> Program { get; set; }

    /// <summary>
    /// Attached updates counted since the last contact damage, 0 through 65535: contact damage occurs once the count,
    /// compared as a signed word difference, reaches the damage period. Updates with Samus's own contact damage count
    /// without the check, so the count can grow past the sign bit and wrap (<see cref="CountAttachedUpdate"/>).
    /// </summary>
    public int AttachmentDamageTimer { get; internal set; }

    /// <summary>Counts one attached update, wrapping after 65535 as the cartridge's word does.</summary>
    internal void CountAttachedUpdate() =>
        AttachmentDamageTimer = AttachmentDamageTimer == ushort.MaxValue ? 0 : AttachmentDamageTimer + 1;

    /// <summary>Whether the count has reached <paramref name="period"/>, compared as the cartridge's signed word difference.</summary>
    /// <param name="period">The damage period in attached updates.</param>
    internal bool HasReachedDamagePeriod(int period) => unchecked((short)(AttachmentDamageTimer - period)) >= 0;

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Velocity = FixedVector<RoomPixels>.Zero;
        ShakeTimer = NativeCountdown.Start(0);
        MovementMode = default;
    }
}
