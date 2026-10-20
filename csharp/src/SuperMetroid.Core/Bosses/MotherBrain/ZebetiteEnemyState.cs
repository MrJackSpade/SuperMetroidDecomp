using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one half of Tourian's Zebetite barrier. The values the cartridge keeps in the enemy's common record are
/// emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>Creates the state for one Zebetite half from its spawn parameters: its pairing and linked-slot index.</summary>
    internal ZebetiteEnemyState(RoomEnemyPopulationRecord population)
    {
        IsSecondaryHalf = population.Parameter1 != 0;
        LinkedNativeIndex = population.Parameter2;
    }

    /// <summary>
    /// The bank-$A6 indirect main-AI entry, independent for each physical half; none once a kill has emptied the
    /// record.
    /// </summary>
    public ZebetiteAiFunction? Function { get; internal set; }

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

    /// <summary>The linked half's native <c>$40</c>-byte slot index (parameter two).</summary>
    public int LinkedNativeIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Whether this is the linked bottom half (nonzero parameter one); it skips primary palette cycling and dies without publishing the next generation's events or respawn.</summary>
    public bool IsSecondaryHalf { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Generation = 0;
        GenerationFlags = default;
        LinkedNativeIndex = 0;
        IsSecondaryHalf = false;
    }
}
