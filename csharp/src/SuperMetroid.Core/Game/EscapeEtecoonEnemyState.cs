using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$B3 pre-instruction words stored by escape Etecoon enemy <c>$F2D3</c>. These are
/// executable cartridge addresses, not host state identifiers: the main AI at
/// <c>$B3:E655</c> performs an indirect call through common enemy variable F every frame.
/// </summary>
public enum EscapeEtecoonPreInstruction : ushort
{
    /// <summary><c>$B3:807B</c>, the bank-common one-byte RTL installed by opcode $8074.</summary>
    Cleared = 0x807b,

    /// <summary><c>$B3:E65C</c>, add 3.5 pixels to X without terrain collision.</summary>
    EscapeRight = 0xe65c,

    /// <summary><c>$B3:E670</c>, wait for the persistent “critters escaped” event.</summary>
    WaitForEscapeEvent = 0xe670,

    /// <summary><c>$B3:E680</c>, walk, reverse at walls, and remain floor-aligned.</summary>
    WalkAndFall = 0xe680,
}

/// <summary>
/// Typed projection of the only two common words used by escape Etecoon. Keeping the
/// values on <see cref="RoomEnemySlot"/> preserves the original WRAM layout while giving
/// debugger users the meanings established by <c>$B3:E655-$E730</c>.
/// </summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new escape etecoon enemy state instance.</summary>
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

/// <summary>
/// Debugger-visible typed marker for escape Dachora enemy <c>$F313</c>. Dachora owns no
/// private variables: all of its movement is authored directly in the three ROM instruction
/// lists at <c>$B3:E964-$EAA7</c>. These read-only properties expose that live script state
/// without inventing a parallel host state machine.
/// </summary>
public sealed class EscapeDachoraEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new escape dachora enemy state instance.</summary>
    internal EscapeDachoraEnemyState(RoomEnemySlot slot) => _slot = slot;
}
