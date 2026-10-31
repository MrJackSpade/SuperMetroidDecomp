namespace SuperMetroid.Core.Hud;

/// <summary>The music and sound processing one message-box update performs, and whether it advances the per-line effects.</summary>
/// <param name="AdvancesPerLineEffects">Whether this update advances the per-line screen effects and a power bomb's explosion, as the box's restoring update does.</param>
/// <param name="MusicHandlerCalls">Times this update processes the music queue; zero suppresses the ordinary per-update music processing.</param>
/// <param name="SoundHandlerCalls">Times this update processes the sound queues, including the extra time when the box returns to gameplay.</param>
[IndependentFlag("whether this message-box update advances the per-line effects, a fact of the update")]
public readonly record struct MessageBoxFrameAudio(bool AdvancesPerLineEffects, int MusicHandlerCalls, int SoundHandlerCalls)
{
    /// <summary>Ordinary message update: music and sound processed once each, and no per-line effect advance; describes the processing for the host rather than performing it.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}
