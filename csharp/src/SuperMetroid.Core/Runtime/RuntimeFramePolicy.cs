namespace SuperMetroid.Core.Runtime;

/// <summary>The outer game-state loop's policy for one frame: what the gameplay state permits (#633).</summary>
public sealed record RuntimeFramePolicy
{
    /// <summary>Ordinary main gameplay.</summary>
    public static RuntimeFramePolicy Gameplay { get; } = new();

    /// <summary>
    /// Whether the normal frame tail advances the play time (<see cref="SuperMetroid.Core.Progression.GameTimeState"/>); disabling it does
    /// not suppress the frame boundary, the random draw, or gameplay-owner updates.
    /// </summary>
    public bool AdvanceGameTime { get; init; } = true;

    /// <summary>
    /// Whether Samus's low-health check runs after her periodic damage (#633, #2092, #2499): the gameplay states run it,
    /// attract playback and an ordinary host frame do not.
    /// </summary>
    public bool RunsLowHealthCheck { get; init; }

    /// <summary>
    /// Whether the update shows a running escape timer that Samus's movement mode holds (#2504): the Ceres departure's
    /// elevator hold and blackout show the timer as it stands while the elevator holds Samus, and neither counts it down.
    /// Under Samus's normal movement mode the timer counts and shows whatever this says.
    /// </summary>
    public bool ShowsHeldEscapeTimer { get; init; }

    /// <summary>
    /// Whether main gameplay owns the update, rather than a game state that runs main gameplay's update behind its own
    /// work (#2509, #2517): the fade-in, the pause's darkening and brightening, the time-up's white-out, the Ceres
    /// departure, the death, leaving Zebes, the attract demo and the reserve refill's own updates. Only main gameplay's own
    /// update, which the update the refill completes on is (#2526), admits Samus's pause check, lets the Ceres elevator
    /// take her (#2534) and begins her death; under any other the out-of-health check withholds the death of a Samus out of
    /// health without reserves, and that update does not count the play time. The AUTO reserves' refill begins under any of
    /// them.
    /// </summary>
    public bool MainGameplayOwnsUpdate { get; init; } = true;

    /// <summary>
    /// Whether the update runs main gameplay's update behind the time-up's white-out (#2509, #2518): an update the time-up
    /// owns does not return the game to it.
    /// </summary>
    public bool RunsBehindTimeUp { get; init; }

    /// <summary>
    /// Whether the update's game state enters the AUTO reserves' refill or Samus's death that its out-of-health check sends
    /// her to (#2519, #2517): main gameplay's update, which the pause's brightening, the time-up's white-out and the update
    /// the refill completes on also run, the pause's darkening, which enters the refill, and the refill's own update.
    /// The check then freezes gameplay time itself, before the room shakes, so the update that begins either neither shakes
    /// the room nor counts its earthquake down; the game-state loop enters the refill or the death after the update.
    /// </summary>
    public bool EntersOutOfHealthRoute { get; init; }
}
