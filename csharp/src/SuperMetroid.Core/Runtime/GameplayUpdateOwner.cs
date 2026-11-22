namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The game state that owns one gameplay update, which the game-state loop names when it asks the runtime for the update
/// (#2542): main gameplay's own, or one that runs main gameplay's update behind its own work. Each owner states here, once,
/// what its updates permit; the runtime reads those permissions and never which game state the game is in.
/// </summary>
public sealed partial class GameplayUpdateOwner
{
    /// <summary>Owners exist only as the named instances below.</summary>
    private GameplayUpdateOwner()
    {
    }

    /// <summary>
    /// A host stepping the runtime on its own, without the game-state loop around it: the update runs as main gameplay and counts the play time, while Samus's low-health check does not run and nothing enters
    /// the refill or the death the out-of-health check reports, so the check freezes nothing.
    /// </summary>
    public static GameplayUpdateOwner RuntimeOnlyHost { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = false,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = true,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// The first room's fade-in after a load or the new game's start: gameplay runs behind the brightening with Samus's
    /// low-health check and counts the play time, but the game is not yet in main gameplay, and nothing enters the refill or
    /// the death.
    /// </summary>
    public static GameplayUpdateOwner FadeIn { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// Main gameplay's own update, which the update the AUTO reserves' refill completes on also is (#2526): Samus's pause
    /// check, the Ceres elevator and her death are admitted, and the update enters the refill or the death its out-of-health
    /// check sends her to.
    /// </summary>
    public static GameplayUpdateOwner MainGameplay { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = true,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>
    /// The pause's darkening: gameplay runs behind the darkening fade, which owns the update, so no pause, death or Ceres
    /// elevator begins in it; out of health with AUTO reserves Samus enters the refill in the pause's place (#2517).
    /// </summary>
    public static GameplayUpdateOwner PauseDarkening { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>
    /// The pause's brightening, which runs main gameplay's whole update behind the screen fade and owns it: Samus's pause
    /// check refuses Start and her death waits for main gameplay, while the AUTO reserves' refill begins (#2517).
    /// </summary>
    public static GameplayUpdateOwner PauseBrightening { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>
    /// The time-up's white-out, which runs main gameplay's whole update and owns it: a Samus out of health without reserves
    /// does not die behind it (#2509), the AUTO reserves' refill begins, and the update does not return the game to the
    /// time-up it is already in (#2518).
    /// </summary>
    public static GameplayUpdateOwner TimeUpWhiteOut { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = false,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>
    /// The Ceres departure's elevator hold and fade to black: gameplay runs with the escape timer shown as it stands while
    /// the elevator holds Samus (#2504), and nothing enters the refill or the death.
    /// </summary>
    public static GameplayUpdateOwner CeresDeparture { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = true,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// Samus's death: its final gameplay pass and the surroundings' black-out run gameplay with the play time held (#1544).
    /// </summary>
    public static GameplayUpdateOwner Death { get; } = new()
    {
        CountsPlayTime = false,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// The gunship leaving Zebes: main gameplay's update runs behind every darkening update and counts the play time
    /// (#2517), and nothing enters the refill or the death.
    /// </summary>
    public static GameplayUpdateOwner LeavingZebes { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// The attract demo's first pass and playback: the recording runs gameplay without counting the play time and without
    /// Samus's low-health check, and nothing enters the refill or the death.
    /// </summary>
    public static GameplayUpdateOwner AttractDemo { get; } = new()
    {
        CountsPlayTime = false,
        RunsLowHealthCheck = false,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = false,
    };

    /// <summary>
    /// The AUTO reserves' refill's own updates: they run main gameplay's update behind the refill, which owns them (#2517),
    /// so no pause, death or Ceres elevator begins in them (#2526), and they enter the refill or the death their
    /// out-of-health check sends Samus to (#2519). The update the refill completes on hands the rest of itself to
    /// <see cref="MainGameplay"/> after the frame boundary.
    /// </summary>
    public static GameplayUpdateOwner ReserveRefill { get; } = new()
    {
        CountsPlayTime = true,
        RunsLowHealthCheck = true,
        ShowsHeldEscapeTimer = false,
        IsMainGameplay = false,
        MayReturnToTimeUp = true,
        EntersOutOfHealthRoute = true,
    };

    /// <summary>
    /// Whether the update's game-time step counts the play time (<see cref="SuperMetroid.Core.Progression.GameTimeState"/>);
    /// holding it does not suppress the frame boundary, the random draw, or the update's gameplay.
    /// </summary>
    public bool CountsPlayTime { get; private init; }

    /// <summary>Whether Samus's low-health check runs after her periodic damage (#633, #2092, #2499).</summary>
    public bool RunsLowHealthCheck { get; private init; }

    /// <summary>
    /// Whether the update shows a running escape timer that Samus's movement mode holds, as it stands and without counting
    /// it down (#2504). Under Samus's normal movement mode the timer counts and shows whatever this says.
    /// </summary>
    public bool ShowsHeldEscapeTimer { get; private init; }

    /// <summary>
    /// Whether the game is in main gameplay through the update, rather than in a game state that runs main gameplay's
    /// update behind its own work (#2509, #2517). Only then does Samus's pause check admit Start, the Ceres elevator take
    /// her (#2534) and the out-of-health check begin her death; otherwise the check withholds the death of a Samus out of
    /// health without reserves, and that update does not count the play time. The AUTO reserves' refill begins whatever
    /// owns the update.
    /// </summary>
    public bool IsMainGameplay { get; private init; }

    /// <summary>
    /// Whether, once the Zebes time bomb is set, the update returns the game to the time-up its run-out escape countdown
    /// started (#2518); only the time-up's own white-out does not.
    /// </summary>
    public bool MayReturnToTimeUp { get; private init; }

    /// <summary>
    /// Whether the update's game state enters the AUTO reserves' refill or Samus's death that its out-of-health check sends
    /// her to (#2517, #2519). The check then freezes gameplay time itself, before the room shakes, so the update that begins
    /// either neither shakes the room nor counts its earthquake down; the game-state loop enters the refill or the death
    /// after the update.
    /// </summary>
    public bool EntersOutOfHealthRoute { get; private init; }
}
