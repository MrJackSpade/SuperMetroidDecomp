using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What one Samus animation step reads: input, the NMI clock, system state, liquid-sound and pose-input policy, and the echo-sound queue (#633).</summary>
public sealed record SamusAnimationStep
{
    /// <summary>Controller input.</summary>
    public ControllerButton ControllerInput { get; init; } = 0;

    /// <summary>
    /// The frame clock's value for this step, 0 through 65535: the clock wraps at sixteen bits and its low bits
    /// pace liquid effects, so the full wrapped value is carried; other values are rejected.
    /// </summary>
    public int NmiFrameCounter
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The session's random-number word, which Samus's liquid effects draw air bubbles from.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>Begin liquid sound request frame.</summary>
    public bool BeginLiquidSoundRequestFrame { get; init; } = true;

    /// <summary>Prospective input pose.</summary>
    public SamusPoseId? ProspectiveInputPose { get; init; } = null;

    /// <summary>Demo pose input.</summary>
    public bool DemoPoseInput { get; init; } = false;

    /// <summary>Whether the room's boss ID ($179C) is nonzero, which suppresses the running-footstep sound ($90:A409).</summary>
    public bool BossPresent { get; init; } = false;

    /// <summary>Queue echo sound.</summary>
    public Func<SoundQueueAdmission>? QueueEchoSound { get; init; } = null;
}
