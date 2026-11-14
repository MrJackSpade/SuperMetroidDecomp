namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The behaviours Shitroid's main AI dispatches.
/// Its values are arbitrary; verification's catalog relates each member to its source (#1413).
/// </summary>
public enum ShitroidAction
{
    /// <summary>Keeps the encounter actor inactive in the alternate camera state.</summary>
    Dormant,
    /// <summary>Waits for the camera before starting the scripted entrance.</summary>
    WaitForCamera,
    /// <summary>Initializes the Shitroid entrance delay.</summary>
    BeginEntranceDelay,
    /// <summary>Counts down before Shitroid enters the room.</summary>
    EntranceDelay,
    /// <summary>Flies toward the living Sidehopper victim.</summary>
    FlyToSidehopper,
    /// <summary>Tracks the Sidehopper until attachment.</summary>
    ChaseSidehopper,
    /// <summary>Attaches to the Sidehopper and begins the feeding pose.</summary>
    AttachToSidehopper,
    /// <summary>Drains the Sidehopper while advancing its palette.</summary>
    DrainSidehopper,
    /// <summary>Activates the dead Sidehopper corpse actor.</summary>
    ActivateSidehopperCorpse,
    /// <summary>Rises away from the victim after feeding.</summary>
    RiseAfterFeeding,
    /// <summary>Hovers near Samus before beginning pursuit.</summary>
    HoverNearSamus,
    /// <summary>Chases Samus for the scripted drain.</summary>
    ChaseSamus,
    /// <summary>Installs the Samus-draining attachment state.</summary>
    BeginDrainingSamus,
    /// <summary>Drains Samus while controlling her attached position.</summary>
    DrainSamus,
    /// <summary>Initializes the pause after draining Samus.</summary>
    BeginPostDrainPause,
    /// <summary>Holds the completed drain pose.</summary>
    PostDrainPause,
    /// <summary>Rises after releasing the drained Samus.</summary>
    RiseAfterDrainingSamus,
    /// <summary>Flies left during the scripted departure.</summary>
    FlyLeft,
    /// <summary>Flies right during the scripted departure.</summary>
    FlyRight,
    /// <summary>Initializes the final room exit.</summary>
    BeginExit,
    /// <summary>Flies out of the encounter room.</summary>
    Exit,
    /// <summary>Holds Samus briefly before the scripted release.</summary>
    HoldSamusBeforeRelease,
    /// <summary>Follows the released-Samus sequence to its handoff.</summary>
    ReleasedFollow,
}
