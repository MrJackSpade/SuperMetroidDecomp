namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Typed names for the bank-$A9 function pointers stored in Shitroid's enemy variable A.
/// The numeric values intentionally remain the cartridge addresses used by the dispatcher.
/// </summary>
public enum ShitroidAiFunction : ushort
{
    /// <summary>$A9:EFDF, keeps the encounter actor inactive in the alternate camera state.</summary>
    Dormant = 1,
    /// <summary>$A9:EFE6, waits for the camera before starting the scripted entrance.</summary>
    WaitForCamera,
    /// <summary>$A9:F02B, initializes the Shitroid entrance delay.</summary>
    BeginEntranceDelay,
    /// <summary>$A9:F037, counts down before Shitroid enters the room.</summary>
    EntranceDelay,
    /// <summary>$A9:F049, flies toward the living Sidehopper victim.</summary>
    FlyToSidehopper,
    /// <summary>$A9:F06D, tracks the Sidehopper until attachment.</summary>
    ChaseSidehopper,
    /// <summary>$A9:F094, attaches to the Sidehopper and begins the feeding pose.</summary>
    AttachToSidehopper,
    /// <summary>$A9:F0E6, drains the Sidehopper while advancing its palette.</summary>
    DrainSidehopper,
    /// <summary>$A9:F125, activates the dead Sidehopper corpse actor.</summary>
    ActivateSidehopperCorpse,
    /// <summary>$A9:F138, rises away from the victim after feeding.</summary>
    RiseAfterFeeding,
    /// <summary>$A9:F180, hovers near Samus before beginning pursuit.</summary>
    HoverNearSamus,
    /// <summary>$A9:F1FA, chases Samus for the scripted drain.</summary>
    ChaseSamus,
    /// <summary>$A9:F20E, installs the Samus-draining attachment state.</summary>
    BeginDrainingSamus,
    /// <summary>$A9:F21B, drains Samus while controlling her attached position.</summary>
    DrainSamus,
    /// <summary>$A9:F2A2, initializes the pause after draining Samus.</summary>
    BeginPostDrainPause,
    /// <summary>$A9:F2AE, holds the completed drain pose.</summary>
    PostDrainPause,
    /// <summary>$A9:F2C0, rises after releasing the drained Samus.</summary>
    RiseAfterDrainingSamus,
    /// <summary>$A9:F2FB, flies left during the scripted departure.</summary>
    FlyLeft,
    /// <summary>$A9:F324, flies right during the scripted departure.</summary>
    FlyRight,
    /// <summary>$A9:F360, initializes the final room exit.</summary>
    BeginExit,
    /// <summary>$A9:F36D, flies out of the encounter room.</summary>
    Exit,
    /// <summary>$A9:F3A3, holds Samus briefly before the scripted release.</summary>
    HoldSamusBeforeRelease,
    /// <summary>$A9:F3BE, follows the released-Samus sequence to its handoff.</summary>
    ReleasedFollow,
}
