using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed names for the bank-$A9 function pointers stored in Shitroid's enemy variable A.
/// The numeric values intentionally remain the cartridge addresses used by the dispatcher.
/// </summary>
public enum ShitroidAiFunction : ushort
{
    /// <summary>$A9:EFDF, keeps the encounter actor inactive in the alternate camera state.</summary>
    Dormant = 0xefdf,
    /// <summary>$A9:EFE6, waits for the camera before starting the scripted entrance.</summary>
    WaitForCamera = 0xefe6,
    /// <summary>$A9:F02B, initializes the Shitroid entrance delay.</summary>
    BeginEntranceDelay = 0xf02b,
    /// <summary>$A9:F037, counts down before Shitroid enters the room.</summary>
    EntranceDelay = 0xf037,
    /// <summary>$A9:F049, flies toward the living Sidehopper victim.</summary>
    FlyToSidehopper = 0xf049,
    /// <summary>$A9:F06D, tracks the Sidehopper until attachment.</summary>
    ChaseSidehopper = 0xf06d,
    /// <summary>$A9:F094, attaches to the Sidehopper and begins the feeding pose.</summary>
    AttachToSidehopper = 0xf094,
    /// <summary>$A9:F0E6, drains the Sidehopper while advancing its palette.</summary>
    DrainSidehopper = 0xf0e6,
    /// <summary>$A9:F125, activates the dead Sidehopper corpse actor.</summary>
    ActivateSidehopperCorpse = 0xf125,
    /// <summary>$A9:F138, rises away from the victim after feeding.</summary>
    RiseAfterFeeding = 0xf138,
    /// <summary>$A9:F180, hovers near Samus before beginning pursuit.</summary>
    HoverNearSamus = 0xf180,
    /// <summary>$A9:F1FA, chases Samus for the scripted drain.</summary>
    ChaseSamus = 0xf1fa,
    /// <summary>$A9:F20E, installs the Samus-draining attachment state.</summary>
    BeginDrainingSamus = 0xf20e,
    /// <summary>$A9:F21B, drains Samus while controlling her attached position.</summary>
    DrainSamus = 0xf21b,
    /// <summary>$A9:F2A2, initializes the pause after draining Samus.</summary>
    BeginPostDrainPause = 0xf2a2,
    /// <summary>$A9:F2AE, holds the completed drain pose.</summary>
    PostDrainPause = 0xf2ae,
    /// <summary>$A9:F2C0, rises after releasing the drained Samus.</summary>
    RiseAfterDrainingSamus = 0xf2c0,
    /// <summary>$A9:F2FB, flies left during the scripted departure.</summary>
    FlyLeft = 0xf2fb,
    /// <summary>$A9:F324, flies right during the scripted departure.</summary>
    FlyRight = 0xf324,
    /// <summary>$A9:F360, initializes the final room exit.</summary>
    BeginExit = 0xf360,
    /// <summary>$A9:F36D, flies out of the encounter room.</summary>
    Exit = 0xf36d,
    /// <summary>$A9:F3A3, holds Samus briefly before the scripted release.</summary>
    HoldSamusBeforeRelease = 0xf3a3,
    /// <summary>$A9:F3BE, follows the released-Samus sequence to its handoff.</summary>
    ReleasedFollow = 0xf3be,
}

/// <summary>One hardcoded bank-$84 wall mutation emitted by Shitroid's room script.</summary>
/// <param name="Block">The room block the PLM is spawned at (native inline byte arguments).</param>
/// <param name="Header">The bank-$84 PLM header.</param>
public readonly record struct ShitroidPlmRequest(Position16<RoomBlocks> Block, PlmHeaderId Header);

/// <summary>One delayed music command emitted by the Shitroid encounter.</summary>
public readonly record struct ShitroidMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>Typed projection of Shitroid's common and extended enemy WRAM.</summary>
public sealed class ShitroidEnemyState
{
    /// <summary>Target palette image staged across the encounter's palette transitions.</summary>
    private readonly Bgr555[] _targetPalette = new Bgr555[256];

    /// <summary>Projects Shitroid's common variables through its owning physical enemy slot.</summary>
    internal ShitroidEnemyState(RoomEnemySlot slot) => Slot = slot;

    /// <summary>Gets the physical enemy slot that owns the projected common words.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Gets the current bank-$A9 Shitroid AI function pointer.</summary>
    public ShitroidAiFunction Function
    {
        get => (ShitroidAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Signed 8.8 velocity: X in native enemy variable B, Y in variable C.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(Slot.VariableB), new SnesSignedEightEight(Slot.VariableC));
        internal set
        {
            Slot.VariableB = value.X.RawValue;
            Slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>Low-byte palette phase and high-byte delay timer in variable D.</summary>
    public ushort PaletteTimerAndPhase
    {
        get => Slot.VariableD;
        internal set => Slot.VariableD = value;
    }

    /// <summary>Gets the phase reload controlling Shitroid's palette-cycle cadence.</summary>
    public ushort PaletteDelay
    {
        get => Slot.VariableE;
        internal set => Slot.VariableE = value;
    }

    /// <summary>Gets the current AI state's general-purpose countdown.</summary>
    public ushort StateTimer
    {
        get => Slot.VariableF;
        internal set => Slot.VariableF = value;
    }

    /// <summary>Native extended word 01: randomized vertical-follow duration.</summary>
    public ushort HoverTimer { get; internal set; }

    /// <summary>Native extended word 02: consecutive horizontal-lock accumulator.</summary>
    public ushort HorizontalLockAccumulator { get; internal set; }

    /// <summary>Native extended word 04: enables the periodic Shitroid cry.</summary>
    public ushort CryEnabled { get; internal set; }

    /// <summary>Native extended word 05: counts every fourth palette-cycle cry.</summary>
    public ushort CryCounter { get; internal set; }

    /// <summary>
    /// Diagnostic mirror of native word 08 in physical slot one. Shitroid writes the actual
    /// dead-sidehopper palette stage through <c>ActivateDeadSidehopperVictim</c>; retaining
    /// this value on the boss state makes that cross-slot side effect directly auditable.
    /// </summary>
    public ushort VictimActivationFlag { get; internal set; }
    /// <summary>Mutable target palette storage used by encounter palette updates.</summary>
    internal Span<Bgr555> MutableTargetPalette => _targetPalette;
}
