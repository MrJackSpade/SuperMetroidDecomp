using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Typed projection of the only two common words used by escape Etecoon. Keeping the
/// values on <see cref="RoomEnemySlot"/> preserves the original WRAM layout while giving
/// debugger users the meanings established by <c>$B3:E655-$E730</c>.
/// </summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>The actor's typed program cursor, in place of its native instruction-list pointer.</summary>
    internal AnimationProgramCursor<EscapeEtecoonProgram> Program { get; set; }

    /// <summary>Underlying enemy slot whose common variables A and F carry Etecoon's speed and bank-$B3 callback.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects escape Etecoon's common WRAM words without allocating parallel movement state.</summary>
    internal EscapeEtecoonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A, a signed 8.8 horizontal speed consumed by <c>$B3:E680</c>.</summary>
    public ushort HorizontalSpeed
    {
        get => _slot.VariableA;
        set => _slot.VariableA = value;
    }

    /// <summary>Variable F, the literal bank-$B3 pre-instruction pointer.</summary>
    public EscapeEtecoonPreInstruction PreInstruction
    {
        get => (EscapeEtecoonPreInstruction)_slot.VariableF;
        set => _slot.VariableF = (ushort)value;
    }
}
