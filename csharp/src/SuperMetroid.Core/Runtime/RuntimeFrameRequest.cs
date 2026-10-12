namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What a host asks of one runtime frame beyond controller input (#633): the gameplay-state policy,
/// the cartridge audio calls the frame makes at their native positions, and host work immediately
/// after the accepted NMI. Nothing here is retained after the frame or serialized with the runtime.
/// </summary>
/// <remarks>A default request is an ordinary state-eight frame: departure allowed, game time advancing, deferred audio, no NMI work.</remarks>
public sealed record RuntimeFrameRequest
{
    /// <summary>The outer dispatcher's state policy for this frame.</summary>
    public RuntimeFramePolicy Policy { get; init; } = RuntimeFramePolicy.Gameplay;

    /// <summary>How the frame's echo and low-health audio calls reach the cartridge audio queue.</summary>
    public RuntimeFrameAudio Audio { get; init; } = RuntimeFrameAudio.Deferred;

    /// <summary>
    /// Host work immediately after the ordinary accepted NMI and before HDMA and main-loop owners;
    /// the suspended message-box frame path does not run it.
    /// </summary>
    public Action AfterAcceptedNmi { get; init; } = static () => { };
}

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

/// <summary>
/// How one runtime frame's speed-booster echo and low-health check reach the cartridge audio queue
/// (#633). Each scenario is named; there is no partially configured audio host.
/// </summary>
public sealed class RuntimeFrameAudio
{
    /// <summary>Initializes a new runtime frame audio instance.</summary>
    private RuntimeFrameAudio(Func<ushort>? queueEchoSound, Action? checkLowHealth)
    {
        QueueEchoSound = queueEchoSound;
        CheckLowHealth = checkLowHealth;
    }

    /// <summary>
    /// Standalone runtimes without an attached audio queue: the echo is published after the frame
    /// and no low-health check runs.
    /// </summary>
    public static RuntimeFrameAudio Deferred { get; } = new(null, null);

    /// <summary>
    /// Gameplay with an attached queue: the echo is queued synchronously, its accumulator selecting
    /// the reset tables, and the admitted low-health check runs after Samus's periodic damage.
    /// </summary>
    public static RuntimeFrameAudio Gameplay(Func<ushort> queueEchoSound, Action checkLowHealth) =>
        new(Ensure.NotNull(queueEchoSound),
            Ensure.NotNull(checkLowHealth));

    /// <summary>Attract playback: the echo is queued synchronously and no low-health check runs.</summary>
    public static RuntimeFrameAudio EchoOnly(Func<ushort> queueEchoSound) =>
        new(Ensure.NotNull(queueEchoSound), null);

    /// <summary>The synchronous echo call, returning its accumulator; none publishes the echo after the frame.</summary>
    internal Func<ushort>? QueueEchoSound { get; }

    /// <summary>The admitted low-health check; none when this scenario makes no check.</summary>
    internal Action? CheckLowHealth { get; }
}
