namespace SuperMetroid.Core.Runtime;

/// <summary>
/// How one runtime frame's speed-booster echo and low-health check reach the cartridge audio queue
/// (#633). Each scenario is named; there is no partially configured audio host.
/// </summary>
public sealed class RuntimeFrameAudio
{
    /// <summary>Captures only the audio callbacks admitted by a named frame scenario.</summary>
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
