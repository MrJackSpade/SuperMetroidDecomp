using SuperMetroid.Core.Programs;
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

    /// <summary>
    /// The carry-motion record, 0 through 31, the statue last moved by (instruction $AA:E5D8). Variable C keeps the
    /// cartridge's byte offset of that record, twice the record, for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int CarryRecord
    {
        get => _slot.VariableC / 2;
        internal set => _slot.VariableC = checked((ushort)(2 * Ensure.BetweenInclusive(0, ChozoCarryMotionDefinitions.RecordCount - 1, value)));
    }

    /// <summary>
    /// Variable A as a signed value: the Wrecked Ship wake writes -256 to it. Nothing in the statue reads it back and
    /// its meaning is unresolved; it is kept, as the signed word the cartridge stores, for other enemies' aliased
    /// enemy-RAM reads.
    /// </summary>
    public int UnresolvedWakeValueA
    {
        get => unchecked((short)_slot.VariableA);
        internal set => _slot.VariableA = unchecked((ushort)checked((short)value));
    }

    /// <summary>
    /// Variable B as a signed value: the Wrecked Ship wake writes +256 to it. Nothing in the statue reads it back and
    /// its meaning is unresolved; it is kept, as the signed word the cartridge stores, for other enemies' aliased
    /// enemy-RAM reads.
    /// </summary>
    public int UnresolvedWakeValueB
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)checked((short)value));
    }

    /// <summary>The literal bank-$AA pre-instruction pointer stored in variable F.</summary>
    public ChozoStatuePreInstruction PreInstruction
    {
        get => (ChozoStatuePreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
