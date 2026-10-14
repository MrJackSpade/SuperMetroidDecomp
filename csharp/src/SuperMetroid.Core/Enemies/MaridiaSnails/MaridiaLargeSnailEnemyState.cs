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
