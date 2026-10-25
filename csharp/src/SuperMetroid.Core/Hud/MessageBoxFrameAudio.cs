namespace SuperMetroid.Core.Hud;

/// <summary>The music and sound handler calls one message-box update makes, and whether it advances the per-line effects.</summary>
/// <param name="AdvancesPerLineEffects">Whether this update advances the per-line screen effects and a power bomb's explosion, as the box's restoring update does.</param>
/// <param name="MusicHandlerCalls">Number of native music-queue handler calls represented by this update; zero suppresses the ordinary host music call.</param>
/// <param name="SoundHandlerCalls">Number of native sound-handler calls represented by this update, including the extra dispatch call when the coroutine returns.</param>
[IndependentFlag("whether this message-box update advances the per-line effects, a fact of the update")]
public readonly record struct MessageBoxFrameAudio(bool AdvancesPerLineEffects, int MusicHandlerCalls, int SoundHandlerCalls)
{
    /// <summary>Ordinary message update: one music handler, one sound handler, and no per-line effect advance; describes calls for the host rather than executing them.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}
