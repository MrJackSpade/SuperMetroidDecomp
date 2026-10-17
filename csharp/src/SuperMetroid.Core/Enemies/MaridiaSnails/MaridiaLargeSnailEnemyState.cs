using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Typed debugger projection of Oum's six common enemy words and ten family-extra words.
/// Common variables remain backed by the physical enemy slot; family-extra values are named
/// host fields because this family has no cross-slot WRAM aliases.
/// </summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>Common enemy slot backing Oum's native variables A-F.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Oum's common words and family-extra state from its live enemy slot.</summary>
    internal MaridiaLargeSnailEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MaridiaLargeSnailProgram> Program { get; set; }

    /// <summary>
    /// Extra word $00: the requested animation, 0 through 7, an index into the eight-entry selector table at $CB77.
    /// Its lowest bit selects the left-facing program, and the idle, roll and attack transitions set, clear and toggle
    /// its upper two bits, which never carries it outside the table.
    /// </summary>
    public int RequestedInstructionListIndex { get; internal set; }

    /// <summary>Extra word $01: the last requested animation <c>SetOumInstList</c> installed, 0 through 7.</summary>
    public int InstalledInstructionListIndex { get; internal set; }

    /// <summary>Extra word $02: set by instruction $CCB3 when the attack animation ends.</summary>
    public bool AttackAnimationFinished { get; internal set; }

    /// <summary>
    /// Extra word $03: set only during the rolling frames where Oum may turn into its next
    /// attack. The native name is "attack allowing rotation flag"; it is not a hitbox flag.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>Extra word $04: Samus is within the strict 24-by-32 proximity window.</summary>
    public bool SamusInPushingRange { get; internal set; }

    /// <summary>Extra word $05: one when Samus is to the left, zero when to the right.</summary>
    public bool SamusIsLeft { get; internal set; }

    /// <summary>Extra words $06/$08: whole-pixel position captured before this frame's AI.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Extra word $0B: movement suppression produced after AI and consumed on the next frame.
    /// This one-frame pipeline is intentional cartridge ordering, not input latency invented
    /// by the host translation.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>Common variable A: outer behavior function.</summary>
    public MaridiaLargeSnailEnemyFunction Function
    {
        get => (MaridiaLargeSnailEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Common variable B: the bounce arc's quadratic-speed record in 256ths, stepped by 384 per update. Falling caps it
    /// at 16384; an impact removes 4096 and may leave it below zero, which ends the bounces, and rising stores zero
    /// once it would go below zero. It therefore stays within -3712 through 16384, and the slot variable keeps the
    /// signed word the cartridge stores for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int YSpeedTableIndex
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)checked((short)value));
    }

    /// <summary>Common variable C: signed-underflow attack cooldown, initialized to 128.</summary>
    public NativeCountdown AttackCooldown
    {
        get => NativeCountdown.FromRaw(_slot.VariableC);
        internal set => _slot.VariableC = value.RawValue;
    }

    /// <summary>Common variable D: zero moves right; one moves left.</summary>
    public bool MovingLeft
    {
        get => _slot.VariableD != 0;
        internal set => _slot.VariableD = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Common variable E: remaining landing impacts before Oum is settled.</summary>
    public NativeCountdown RemainingBounces
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Common variable F: falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBounceFunction BounceFunction
    {
        get => (MaridiaLargeSnailBounceFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
