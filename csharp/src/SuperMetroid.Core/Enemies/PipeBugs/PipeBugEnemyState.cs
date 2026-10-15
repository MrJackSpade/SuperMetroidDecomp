using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// Debugger-visible projection of Pipe Bug's native variables. Words A-F remain backed by
/// the physical 64-byte enemy record. The remaining named properties model the extended
/// $7E:7800 enemy words used by this family; several meanings intentionally overlap because
/// the three ROM initializers reuse the same storage for unrelated state machines.
/// </summary>
public sealed class PipeBugEnemyState
{
    /// <summary>Physical enemy record whose A-F words are shared by the three Pipe Bug variants.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Captures the original enemy identity before generic death can clear its live definition word.</summary>
    internal PipeBugEnemyState(RoomEnemySlot slot)
    {
        _slot = slot;
        DefinitionAtInitialization = slot.EnemyDefinitionPointer;
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

    /// <summary>Native variable A for Norfair/yellow bugs, or direction for Brinstar bugs.</summary>
    public ushort VariableA
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native variable F for Brinstar bugs; variable A for the other two species.</summary>
    public PipeBugEnemyFunction Function
    {
        get => (PipeBugEnemyFunction)(IsBrinstar ? _slot.VariableF : _slot.VariableA);
        internal set
        {
            if (IsBrinstar)
                _slot.VariableF = (ushort)value;
            else
                _slot.VariableA = (ushort)value;
        }
    }

    /// <summary>Gets whether the initialized owner is either Brinstar Pipe Bug variant.</summary>
    public bool IsBrinstar =>
        DefinitionAtInitialization is
            EnemyDefinitionId.Zeb or
            EnemyDefinitionId.Zebbo;

    // The names below follow their role in the currently selected species. They are kept
    // together because the cartridge overlays all of them in one Enemy_PipeBug structure.
    /// <summary>Gets the whole-pixel spawn position restored after leaving the screen.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>Gets the upper Y boundary of a Brinstar Pipe Bug's emergence.</summary>
    public ushort EmergenceTopY { get; internal set; }
    /// <summary>Gets the requested Brinstar animation selector bits.</summary>
    public PipeBugAnimationSelector AnimationState { get; internal set; }
    /// <summary>Gets the Brinstar animation selector currently installed in the enemy slot.</summary>
    public PipeBugAnimationSelector InstalledAnimationState { get; internal set; }
    /// <summary>
    /// Gets the Norfair formation stagger counter, counting up. Brinstar Pipe Bugs keep their respawn
    /// countdown in the same native word; see <see cref="RespawnDelay"/>.
    /// </summary>
    public ushort FormationCounter { get; internal set; }

    /// <summary>
    /// Gets the Brinstar Pipe Bug's respawn countdown: <see cref="FormationCounter"/>'s native word read as a
    /// wrapping countdown that expires when it reaches zero.
    /// </summary>
    public NativeCountdown RespawnDelay
    {
        get => NativeCountdown.FromRaw(FormationCounter);
        internal set => FormationCounter = value.RawValue;
    }
    /// <summary>Gets the byte offset of this Norfair bug's positive linear-speed record.</summary>
    public ushort LinearSpeedTableOffset { get; internal set; }
    /// <summary>Gets the Norfair member's Y position at the end of its initial rise.</summary>
    public ushort EmergenceY { get; internal set; }
    /// <summary>Gets the formation counter value at which this Norfair member departs.</summary>
    public ushort StaggerTarget { get; internal set; }
    /// <summary>Gets the function selected for this Norfair member after rising.</summary>
    public PipeBugEnemyFunction NorfairPostRiseFunction { get; internal set; }
    /// <summary>Gets the integer word of a yellow Pipe Bug's positive 16.16 velocity.</summary>
    public ushort PositiveVelocityWhole { get; internal set; }
    /// <summary>Gets the fractional word of a yellow Pipe Bug's positive 16.16 velocity.</summary>
    public ushort PositiveVelocityFraction { get; internal set; }
    /// <summary>Gets the integer word of a yellow Pipe Bug's negative 16.16 velocity.</summary>
    public ushort NegativeVelocityWhole { get; internal set; }
    /// <summary>Gets the fractional word of a yellow Pipe Bug's negative 16.16 velocity.</summary>
    public ushort NegativeVelocityFraction { get; internal set; }
    /// <summary>Gets the yellow Pipe Bug's countdown before straight flight begins.</summary>
    public NativeCountdown EmergenceDelay { get; internal set; }
    /// <summary>Gets the quadratic-speed table counter used by a yellow Pipe Bug's arc.</summary>
    public ushort ArcCounter { get; internal set; }
    /// <summary>Gets whether the yellow Pipe Bug arc is in its rising or returning phase.</summary>
    public ushort ArcPhase { get; internal set; }
    /// <summary>Gets whether a yellow Pipe Bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
    /// <summary>Gets the horizontal position at which the current yellow arc began.</summary>
    public ushort ArcStartX { get; internal set; }
}
