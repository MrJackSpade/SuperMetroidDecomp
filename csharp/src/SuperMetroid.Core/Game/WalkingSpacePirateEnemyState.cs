using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Independently tested meanings in walking Pirate population parameter one. No other bits
/// are named: unknown/modded values remain losslessly available through the raw slot word.
/// </summary>
[Flags]
public enum WalkingSpacePirateParameterFlags : ushort
{
    /// <summary>Population parameter-one bit 0, tested by <c>InitAI_PirateWalking</c> at $B2:FD0B: selects the right-facing initial animation; clear selects left, independently of bit 15.</summary>
    StartsFacingRight = 0x0001,
    /// <summary>Population parameter-one bit 15: $B2:FD3B enables the projectile-flinch check after normal AI, and the inherited laser parameter selects two rather than four horizontal pixels per projectile update.</summary>
    SlowLaserAndProjectileFlinch = 0x8000,
}

/// <summary>
/// Literal 16-bit function identities stored in walking Space Pirate variable A. These are
/// addresses inside bank $B2, not host-selected behavior labels; exposing the raw identities
/// makes an instruction-list breakpoint directly comparable with the cartridge state.
/// </summary>
public enum WalkingSpacePirateFunction : ushort
{
    /// <summary>The shared bank-$B2 RTS copied by initialization and flinch lists.</summary>
    NoOperation = 0x804b,

    /// <summary>Walk left, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingLeft = 0xfd44,

    /// <summary>Walk right, fall when unsupported, and turn at a wall, ledge, or patrol post.</summary>
    WalkingRight = 0xfdce,

    /// <summary>The second RTS used while a firing/look-around list owns presentation.</summary>
    AnimationOwnedNoOperation = 0xfe4a,
}

/// <summary>
/// Debugger-facing projection of one walking Space Pirate's native state. The patrol limits
/// deliberately remain wrapped 16-bit world coordinates because population parameter two
/// is added/subtracted without clamping by <c>$B2:FD02</c>.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal WalkingSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Bank-$B2 function address stored in native enemy variable A.</summary>
    public WalkingSpacePirateFunction Function
    {
        get => (WalkingSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Left patrol post stored in native variable E.</summary>
    public ushort LeftPostXPosition
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Right patrol post stored in native variable F.</summary>
    public ushort RightPostXPosition
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Successful allocations from the actor's three-shot firing animations.</summary>
    public int SpawnedLaserCount { get; internal set; }
}
