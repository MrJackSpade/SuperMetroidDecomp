using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B3 function words used by all three Pipe Bug state machines. These are
/// addresses, not a host-designed progression enum: retaining the ROM values makes the
/// debugger state directly comparable with enemy WRAM and makes an unknown branch fail
/// explicitly instead of silently selecting a plausible-looking behavior.
/// </summary>
public enum PipeBugEnemyFunction : ushort
{
    /// <summary>$B3:8880, waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    BrinstarWaitUntilOnScreen = 0x8880,
    /// <summary>$B3:8890, waits for Samus to enter the Brinstar emergence region.</summary>
    BrinstarWaitForSamus = 0x8890,
    /// <summary>$B3:88E3, raises a Brinstar Pipe Bug out of its pipe.</summary>
    BrinstarEmerge = 0x88e3,
    /// <summary>$B3:891C, flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    BrinstarFlyHorizontally = 0x891c,
    /// <summary>$B3:897E, holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    BrinstarRespawnDelay = 0x897e,

    /// <summary>$B3:8BCD, waits for the five-member Norfair formation to become ready.</summary>
    NorfairWaitForFormation = 0x8bcd,
    /// <summary>$B3:8BFF, waits for Samus to trigger the Norfair formation.</summary>
    NorfairWaitForSamus = 0x8bff,
    /// <summary>$B3:8CA6, raises one Norfair formation member toward Samus.</summary>
    NorfairRise = 0x8ca6,
    /// <summary>$B3:8CFF, advances the formation leader's stagger counter.</summary>
    NorfairLeaderStagger = 0x8cff,
    /// <summary>$B3:8D0C, moves the near upper formation member to its stagger height.</summary>
    NorfairUpperNearStagger = 0x8d0c,
    /// <summary>$B3:8D4E, moves the far upper formation member to its stagger height.</summary>
    NorfairUpperFarStagger = 0x8d4e,
    /// <summary>$B3:8D90, moves the near lower formation member to its stagger height.</summary>
    NorfairLowerNearStagger = 0x8d90,
    /// <summary>$B3:8DD2, moves the far lower formation member to its stagger height.</summary>
    NorfairLowerFarStagger = 0x8dd2,
    /// <summary>$B3:8E14, flies a released Norfair formation member left.</summary>
    NorfairFlyLeft = 0x8e14,
    /// <summary>$B3:8E35, flies a released Norfair formation member right.</summary>
    NorfairFlyRight = 0x8e35,
    /// <summary>$B3:8E5A, waits for one Norfair member's stagger target.</summary>
    NorfairWaitForStagger = 0x8e5a,

    /// <summary>$B3:8FB5, waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    YellowWaitForSamus = 0x8fb5,
    /// <summary>$B3:8FF5, delays before a yellow Pipe Bug begins straight flight.</summary>
    YellowEmergenceDelay = 0x8ff5,
    /// <summary>$B3:9028, flies a yellow Pipe Bug left.</summary>
    YellowFlyLeft = 0x9028,
    /// <summary>$B3:90BD, flies a yellow Pipe Bug right.</summary>
    YellowFlyRight = 0x90bd,
    /// <summary>$B3:915A, performs the leftward yellow Pipe Bug arc.</summary>
    YellowArcLeft = 0x915a,
    /// <summary>$B3:91D8, performs the rightward yellow Pipe Bug arc.</summary>
    YellowArcRight = 0x91d8,
}

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
    /// <summary>Gets the species-specific respawn delay or formation stagger counter.</summary>
    public ushort DelayOrCounter { get; internal set; }
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
    public ushort EmergenceDelay { get; internal set; }
    /// <summary>Gets the quadratic-speed table counter used by a yellow Pipe Bug's arc.</summary>
    public ushort ArcCounter { get; internal set; }
    /// <summary>Gets whether the yellow Pipe Bug arc is in its rising or returning phase.</summary>
    public ushort ArcPhase { get; internal set; }
    /// <summary>Gets whether a yellow Pipe Bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
    /// <summary>Gets the horizontal position at which the current yellow arc began.</summary>
    public ushort ArcStartX { get; internal set; }
}
