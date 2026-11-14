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
    /// Whether the normal frame tail advances the four-part gameplay clock; disabling it does
    /// not suppress the frame boundary, the random draw, or gameplay-owner updates.
    /// </summary>
    public bool AdvanceGameTime { get; init; } = true;
}
