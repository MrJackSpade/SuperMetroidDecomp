using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Typed state of the three Pipe Bug families. The function and the Brinstar flight direction are values the cartridge
/// keeps in the enemy's common record, emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>); the
/// rest is per-family state the cartridge kept in its extended $7E:7800 words. Each property belongs to the family its
/// summary names; the others never read it.
/// </summary>
public sealed class PipeBugEnemyState
{
    /// <summary>Captures the original enemy identity before generic death can clear its live definition word.</summary>
    internal PipeBugEnemyState(RoomEnemySlot slot)
    {
        DefinitionAtInitialization = slot.DefinitionId;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<PipeBugProgram> Program { get; set; }

    /// <summary>
    /// Header which created this physical state projection. Generic death clears the live
    /// definition word on the following scheduler scan, but native formation code continues
    /// to address the record's variables through fixed <c>+$40</c> aliases. Species-aware
    /// typed views must therefore use this immutable owner, not the disposable live word.
    /// </summary>
    public EnemyDefinitionId DefinitionAtInitialization { get; }

    /// <summary>
    /// Whether a Brinstar Pipe Bug flies left, set from Samus's side of the bug every update it waits with Samus in its
    /// vertical band. The cartridge keeps it as the sign bit of the record word the other two species use for their
    /// function.
    /// </summary>
    public bool BrinstarFliesLeft { get; internal set; }

    /// <summary>The current bank-$B3 AI function; none once a kill has emptied the record.</summary>
    public PipeBugEnemyFunction? Function { get; internal set; }

    /// <summary>Gets the whole-pixel spawn position restored after leaving the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>
    /// The highest point of a Brinstar Pipe Bug's emergence, sixteen pixels above its spawn point; only Y is
    /// meaningful. The bug keeps rising while it is still below this point.
    /// </summary>
    public Position16<RoomPixels> EmergenceTop { get; internal set; }
    /// <summary>Gets the requested Brinstar animation selector bits.</summary>
    public PipeBugAnimationSelector AnimationState { get; internal set; }
    /// <summary>Gets the Brinstar animation selector currently installed in the enemy slot.</summary>
    public PipeBugAnimationSelector InstalledAnimationState { get; internal set; }

    /// <summary>
    /// A Norfair formation member's stagger counter, counting up from zero to <see cref="LargestCount"/> and wrapping
    /// to zero past it, as the cartridge's word does. It returns to zero only when the member departs, so a member
    /// reset off screen keeps its count into the next rise; <see cref="HasReachedStaggerTarget"/> compares it with
    /// the target by the sign of their sixteen-bit difference.
    /// </summary>
    public int FormationCounter { get; internal set; }

    /// <summary>The largest <see cref="FormationCounter"/>; counting one more wraps to zero.</summary>
    internal const int LargestCount = 65535;

    /// <summary>Counts one formation update, wrapping from <see cref="LargestCount"/> to zero.</summary>
    internal void CountFormationTick() => FormationCounter = FormationCounter == LargestCount ? 0 : FormationCounter + 1;

    /// <summary>
    /// Whether a Norfair member's count has reached its <see cref="StaggerTarget"/>, judged as the cartridge does by
    /// the sign of their difference wrapped to sixteen bits.
    /// </summary>
    internal bool HasReachedStaggerTarget => CoordinateAxis.Difference(FormationCounter, StaggerTarget) >= 0;

    /// <summary>
    /// A Brinstar Pipe Bug's respawn countdown, in AI updates: started at 48 when it leaves the screen, and expiring
    /// when a decrement reaches zero.
    /// </summary>
    public WrappingCountdown RespawnDelay { get; internal set; }
    /// <summary>The row of the shared linear speed table this Norfair bug flies at.</summary>
    public EnemyLinearSpeedRow FlightSpeeds { get; internal set; }
    /// <summary>The point at which a Norfair member's initial rise ended; only Y is meaningful. Its stagger moves are measured from it.</summary>
    public Position16<RoomPixels> RiseEnd { get; internal set; }
    /// <summary>The formation count, 72 through 136, at which this Norfair member departs.</summary>
    public int StaggerTarget { get; internal set; }
    /// <summary>Gets the function selected for this Norfair member after rising.</summary>
    public PipeBugEnemyFunction NorfairPostRiseFunction { get; internal set; }
    /// <summary>A yellow Pipe Bug's signed 16.16 rightward flight velocity in room pixels per update.</summary>
    public SignedSixteenSixteen PositiveVelocity { get; internal set; }
    /// <summary>A yellow Pipe Bug's leftward flight velocity: the separately authored negative record, not a negation.</summary>
    public SignedSixteenSixteen NegativeVelocity { get; internal set; }
    /// <summary>Gets the yellow Pipe Bug's countdown before straight flight begins.</summary>
    public WrappingCountdown EmergenceDelay { get; internal set; }
    /// <summary>
    /// The quadratic-speed record a yellow Pipe Bug's arc reads, from 0 through the table's last record. The rising
    /// phase counts it down from 40 and ends when it would go below zero; the returning phase counts it back up.
    /// </summary>
    public int ArcCounter { get; internal set; }
    /// <summary>Whether a yellow Pipe Bug's arc is in its rising phase rather than its returning one.</summary>
    public bool ArcRising { get; internal set; }
    /// <summary>Gets whether a yellow Pipe Bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        BrinstarFliesLeft = false;
        Function = null;
    }
}
