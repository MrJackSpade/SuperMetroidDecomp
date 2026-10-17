using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Named view of the common enemy words and the three parallel extension-array words used by
/// $A3:A77D-$A9A8. Exposing these values makes the attachment clock and 16.16 steering easy to
/// inspect in C# without pretending the native arrays were part of the common $40-byte slot.
/// </summary>
public sealed class MochtroidEnemyState
{
    /// <summary>Live common enemy slot backing the velocity, shake timer, and one-frame movement selector.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the Mochtroid's named variables over its physical enemy slot.</summary>
    internal MochtroidEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Signed 16.16 velocity: X fraction and whole words in variables A and B, Y fraction and
    /// whole words in variables C and D.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableB, _slot.VariableA, _slot.VariableD, _slot.VariableC);
        internal set
        {
            _slot.VariableA = value.X.Fraction;
            _slot.VariableB = value.X.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.Y.WholeWord;
        }
    }

    /// <summary>Variable E; used only by the shipped-but-unreferenced shake mode.</summary>
    public NativeCountdown ShakeTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Gets the one-update movement selector cleared before each main-AI dispatch.</summary>
    public MochtroidMovementMode MovementMode
    {
        get => (MochtroidMovementMode)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

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
}
