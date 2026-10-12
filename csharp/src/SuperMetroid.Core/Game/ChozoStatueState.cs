using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA pre-instruction pointers used by the two retail Chozo-statue variants.
/// The numeric values are the actual cartridge addresses stored in <c>ai_preinstr</c>.
/// </summary>
public enum ChozoStatuePreInstruction : ushort
{
    /// <summary>$AA:E7A6, the initializer's one-byte RTL.</summary>
    Idle = 0xe7a6,

    /// <summary>$AA:807B, the bank-common RTL installed by instruction $8074.</summary>
    Cleared = 0x807b,

    /// <summary>$AA:E445, waits for Lower Norfair's hand-trigger PLM.</summary>
    WaitForLowerNorfairHandTrigger = 0xe445,

    /// <summary>$AA:E7AE, waits for Phantoon's boss bit and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger = 0xe7ae,

    /// <summary>$AA:E7DA, the no-op installed while the Wrecked Ship statue walks.</summary>
    Walking = 0xe7da,
}

/// <summary>
/// Typed view of the common enemy words reused by the bank-$AA Chozo-statue controller.
/// The wrapper intentionally writes through to the physical slot so debugger watches retain
/// the same layout as WRAM <c>$0FA8..$0FB2</c>.
/// </summary>
public sealed class ChozoStatueState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new chozo statue state instance.</summary>
    internal ChozoStatueState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Movement-table byte offset consumed by instruction $AA:E5D8.</summary>
    public ushort MovementTableOffset
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable A, initialized to -$0100 when Wrecked Ship wakes.</summary>
    public ushort VariableA
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native variable B, initialized to +$0100 when Wrecked Ship wakes.</summary>
    public ushort VariableB
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>The literal bank-$AA pre-instruction pointer stored in variable F.</summary>
    public ChozoStatuePreInstruction PreInstruction
    {
        get => (ChozoStatuePreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// One cross-bank PLM request made by the Chozo-statue enemy AI. Hardcoded requests carry
/// explicit room-block coordinates; ordinary requests use the block selected by the preceding
/// pixel probe. Retaining the header pointer keeps this seam consumable by bank $84 later.
/// </summary>
public readonly record struct ChozoStatuePlmRequest(
    PlmHeaderId HeaderPointer,
    Position16<RoomBlocks> Block,
    bool IsHardcoded);
