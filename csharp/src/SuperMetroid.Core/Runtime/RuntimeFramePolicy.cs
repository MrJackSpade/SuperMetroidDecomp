namespace SuperMetroid.Core.Runtime;

/// <summary>The outer game-state loop's policy for one frame: what the gameplay state permits (#633).</summary>
public sealed record RuntimeFramePolicy
{
    /// <summary>Ordinary main gameplay.</summary>
    public static RuntimeFramePolicy Gameplay { get; } = new();

    /// <summary>
    /// Allows the Ceres shaft room-main trigger to request departure; the outer game-state loop enables
    /// this only in main gameplay, not the elevator-hold and blackout departure states. Shaft rotation still advances independently.
    /// </summary>
    public bool AllowCeresElevatorDeparture { get; init; } = true;

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
}
