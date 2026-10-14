namespace SuperMetroid.Core.Runtime;

/// <summary>The outer dispatcher's policy for one frame: what the gameplay state permits (#633).</summary>
public sealed record RuntimeFramePolicy
{
    /// <summary>Ordinary gameplay state eight.</summary>
    public static RuntimeFramePolicy Gameplay { get; } = new();

    /// <summary>
    /// Allows the Ceres shaft room-main trigger to request departure; the outer dispatcher enables
    /// this only in state eight, not departure states $20/$21. Shaft rotation still advances independently.
    /// </summary>
    public bool AllowCeresElevatorDeparture { get; init; } = true;

    /// <summary>
    /// Whether the normal frame tail advances the native four-word gameplay clock; disabling it does
    /// not suppress NMI, RNG, or gameplay-owner updates.
    /// </summary>
    public bool AdvanceGameTime { get; init; } = true;
}
