using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Typed view of the common enemy words reused by the bank-$AA Chozo-statue controller.
/// The wrapper intentionally writes through to the physical slot so debugger watches retain
/// the same layout as WRAM <c>$0FA8..$0FB2</c>.
/// </summary>
public sealed class ChozoStatueState
{
    /// <summary>Live enemy slot whose common variable words hold movement and bank-$AA pre-instruction state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the Chozo controller's named variables over its physical room enemy slot.</summary>
    internal ChozoStatueState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ChozoStatueProgram> Program { get; set; }

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
