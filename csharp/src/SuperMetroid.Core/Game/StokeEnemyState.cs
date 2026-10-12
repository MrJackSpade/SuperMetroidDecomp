using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The direction word stored at Stoke WRAM <c>$0FB0,x</c>. Stoke's turn routine deliberately
/// leaves this word unchanged until the newly selected animation executes command
/// <c>$8990</c> or <c>$899D</c>; the enum therefore describes cartridge storage, not merely
/// the sign of the current velocity.
/// </summary>
public enum StokeDirection : ushort
{
    /// <summary>Native direction word zero: left-facing latch installed with the moving-left function by instruction $A2:8990.</summary>
    Left = 0,
    /// <summary>Native direction word one: right-facing latch installed with the moving-right function by instruction $A2:899D.</summary>
    Right = 1,
}

/// <summary>
/// Indirect main-AI pointers stored at Stoke WRAM <c>$0FB2,x</c>. The values are the actual
/// bank-$A2 routine offsets so a debugger watch can be compared directly with the ROM.
/// </summary>
public enum StokeAiFunction : ushort
{
    /// <summary>$A2:8A75, RTS_A28A75: no-op main entry while the attack animation owns projectile emission and eventual walk resumption.</summary>
    IdleDuringAttack = 0x8a75,
    /// <summary>$A2:8A43, Function_Stoke_MovingLeft: walking with the negative 16.16 speed pair, terrain-turn checks, and the per-update random left-attack choice.</summary>
    MovingLeft = 0x8a43,
    /// <summary>$A2:8A5C, Function_Stoke_MovingRight: walking with the positive 16.16 speed pair, terrain-turn checks, and the per-update random right-attack choice.</summary>
    MovingRight = 0x8a5c,
}

/// <summary>
/// Typed view over Stoke's six generic enemy-variable words. These properties remain backed
/// by <see cref="RoomEnemySlot"/> rather than duplicating state in host-only fields: inspecting
/// variables A-F consequently shows the same values as <c>$0FA8-$0FB2</c> in the original.
/// </summary>
public sealed class StokeEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new stoke enemy state instance.</summary>
    internal StokeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Fractional half of the positive 16.16 walking displacement.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed whole half of the positive 16.16 walking displacement.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Fractional half of the negative 16.16 walking displacement.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed whole half of the negative 16.16 walking displacement.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Facing latch changed by animation commands, one frame after a turn request.</summary>
    public StokeDirection Direction
    {
        get => (StokeDirection)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>Current indirect main-AI routine.</summary>
    public StokeAiFunction Function
    {
        get => (StokeAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
