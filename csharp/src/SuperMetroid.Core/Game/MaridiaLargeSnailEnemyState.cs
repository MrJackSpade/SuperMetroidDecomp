using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 behavior-function words stored in Oum's common variable A. Keeping the
/// cartridge addresses in the enum makes debugger state directly comparable with $7E:0F92.
/// </summary>
public enum MaridiaLargeSnailEnemyFunction : ushort
{
    /// <summary>Function_Oum_Idle, $A2:CDE6: completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within 24 horizontal pixels.</summary>
    Idle = 0xcde6,
    /// <summary>Function_Oum_Rolling, $A2:CE2B: applies bounce/floor and horizontal collision movement; the preceding update's stop flag also suppresses the signed-underflow attack countdown.</summary>
    Rolling = 0xce2b,
    /// <summary>Function_Oum_Attacking, $A2:CF40: waits for instruction $CCB3's animation-finished flag, consumes it, and installs the corresponding rolling list without moving during the wait.</summary>
    Attacking = 0xcf40,
}

/// <summary>
/// Literal bank-$A2 bounce-function words stored in Oum's common variable F. The enemy uses
/// the shared quadratic-speed table for a diminishing three-impact settling animation.
/// </summary>
public enum MaridiaLargeSnailBounceFunction : ushort
{
    /// <summary>Function_Oum_Falling, $A2:CF66: increases the quadratic-speed selector by $0180 per update up to $4000; a floor impact consumes a bounce, subtracts $1000, and selects rising.</summary>
    Falling = 0xcf66,
    /// <summary>Function_Oum_Rising, $A2:CFA9: subtracts $0180 per update and uses the quadratic table's negative 16.16 pixel displacement; signed selector underflow resets to zero and selects falling without moving that update.</summary>
    Rising = 0xcfa9,
}

/// <summary>
/// Typed debugger projection of Oum's six common enemy words and ten family-extra words.
/// Common variables remain backed by the physical enemy slot; family-extra values are named
/// host fields because this family has no cross-slot WRAM aliases.
/// </summary>
public sealed class MaridiaLargeSnailEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MaridiaLargeSnailEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Extra word $00: requested index into the eight-entry list table at $CB77.</summary>
    public ushort RequestedInstructionListIndex { get; internal set; }

    /// <summary>Extra word $01: last list-table index accepted by <c>SetOumInstList</c>.</summary>
    public ushort InstalledInstructionListIndex { get; internal set; }

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
    /// Extra word $0A is cleared by initialization and otherwise unused by the retail code.
    /// It remains visible so a debugger can prove that the untranslated-looking write exists.
    /// </summary>
    public ushort UnusedExtra0A { get; internal set; }

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

    /// <summary>Common variable B: $0180-stepped index used by the bounce arc.</summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Common variable C: signed-underflow attack cooldown, initialized to 128.</summary>
    public ushort AttackCooldown
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Common variable D: zero moves right; one moves left.</summary>
    public bool MovingLeft
    {
        get => _slot.VariableD != 0;
        internal set => _slot.VariableD = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Common variable E: remaining landing impacts before Oum is settled.</summary>
    public ushort RemainingBounces
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Common variable F: falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBounceFunction BounceFunction
    {
        get => (MaridiaLargeSnailBounceFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
