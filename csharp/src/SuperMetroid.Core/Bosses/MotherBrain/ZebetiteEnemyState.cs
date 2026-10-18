using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one half of Tourian's Zebetite barrier. The values the cartridge keeps in the enemy's common record are
/// emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>The half's slot, whose population parameters hold its pairing and linked-slot index.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the state for one Zebetite half's slot.</summary>
    internal ZebetiteEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// The bank-$A6 indirect main-AI entry, independent for each physical half; none once a kill has emptied the
    /// record.
    /// </summary>
    public ZebetiteAiFunction? Function { get; internal set; }

    /// <summary>
    /// Palette-cycle counter. Only the primary Zebetite's, in physical slot zero, advances: every Zebetite in the room
    /// cycles that one counter, even after the first barrier generation has vacated the slot.
    /// </summary>
    public int PaletteCycle
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// Barrier generation 0 through 4, formed from persistent events five/four/three; four means every barrier is
    /// destroyed.
    /// </summary>
    public int Generation
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>Including the sign bit that requests a linked second half.</summary>
    public ZebetiteGenerationFlags GenerationFlags { get; internal set; }

    /// <summary>Where this half's health program resumes (#641).</summary>
    internal AnimationProgramCursor<ZebetiteProgram> Cursor { get; set; }

    /// <summary>The linked half's native <c>$40</c>-byte slot index, kept in parameter two.</summary>
    public int LinkedNativeIndex
    {
        get => _slot.Parameter2;
        internal set => _slot.Parameter2 = checked((ushort)value);
    }

    /// <summary>Nonzero native init0/parameter one identifies the linked bottom half; it skips primary palette cycling and dies without publishing the next generation's events or respawn.</summary>
    public bool IsSecondaryHalf => _slot.Parameter1 != 0;

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        PaletteCycle = 0;
        Generation = 0;
        GenerationFlags = default;
    }
}
