using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What one Samus animation step reads: input, the NMI clock, system state, liquid-sound and pose-input policy, and the echo-sound queue (#633).</summary>
public sealed record SamusAnimationStep
{
    /// <summary>Controller input.</summary>
    public SnesButton ControllerInput { get; init; } = 0;

    /// <summary>Nmi frame counter.</summary>
    public ushort NmiFrameCounter { get; init; } = 0;

    /// <summary>System.</summary>
    public Bank80SystemState? System { get; init; } = null;

    /// <summary>Begin liquid sound request frame.</summary>
    public bool BeginLiquidSoundRequestFrame { get; init; } = true;

    /// <summary>Prospective input pose.</summary>
    public SamusPoseId? ProspectiveInputPose { get; init; } = null;

    /// <summary>Demo pose input.</summary>
    public bool DemoPoseInput { get; init; } = false;

    /// <summary>Queue echo sound.</summary>
    public Func<ushort>? QueueEchoSound { get; init; } = null;
}
