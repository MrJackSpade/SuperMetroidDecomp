using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What one Samus animation step reads: input, the frame counter, system state, liquid-sound and pose-input policy, and the echo-sound queue (#633).</summary>
public sealed record SamusAnimationStep
{
    /// <summary>Controller input.</summary>
    public ControllerButton ControllerInput { get; init; } = 0;

    /// <summary>
    /// The frame clock's value for this step, 0 through 65535: the clock wraps back to 0 after 65535 and paces
    /// liquid effects, so the full wrapped value is carried; other values are rejected.
    /// </summary>
    public int FrameCounter
    {
        get;
        init => field = Ensure.BetweenInclusive(0, Compatibility.FrameClock.CounterPeriod - 1, value);
    }

    /// <summary>The session's random-number generator, which Samus's liquid effects draw air bubbles from.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>
    /// Whether the animation call is an update of its own, which begins Samus's audio requests; the runtime's animation
    /// runs inside its update and passes false.
    /// </summary>
    public bool BeginLiquidSoundRequestFrame { get; init; } = true;

    /// <summary>Prospective input pose.</summary>
    public SamusPoseId? ProspectiveInputPose { get; init; } = null;

    /// <summary>Demo pose input.</summary>
    public bool DemoPoseInput { get; init; } = false;

    /// <summary>Whether the room's boss ID is nonzero, which suppresses the running-footstep sound.</summary>
    public bool BossPresent { get; init; } = false;

    /// <summary>Queue echo sound.</summary>
    public Func<SoundQueueAdmission>? QueueEchoSound { get; init; } = null;
}
