using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The state of a bank-$AA Chozo statue. The values the cartridge keeps in the enemy's common record are emptied with
/// it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class ChozoStatueState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ChozoStatueProgram> Program { get; set; }

    /// <summary>The carry-motion record, 0 through 31, the statue last moved by (instruction $AA:E5D8).</summary>
    public int CarryRecord { get; internal set; }

    /// <summary>The literal bank-$AA pre-instruction pointer; none once a kill has emptied the record.</summary>
    public ChozoStatuePreInstruction? PreInstruction { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        CarryRecord = 0;
        PreInstruction = null;
        HandTriggered = false;
    }

    /// <summary>Whether this is the Wrecked Ship statue (parameter two zero) rather than Lower Norfair's (two).</summary>
    public bool IsWreckedShip { get; internal set; }

    /// <summary>Whether Samus has triggered the statue's hand (parameter one), which its waiting phase wakes on.</summary>
    public bool HandTriggered { get; internal set; }
}
