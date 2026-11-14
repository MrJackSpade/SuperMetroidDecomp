namespace SuperMetroid.Core.Hud;

/// <summary>The music and sound processing one message-box update performs, and whether it advances the liquid wave and statue descent.</summary>
/// <param name="AdvancesLiquidWaveAndStatueDescent">Whether this update advances the liquid wave, the statue descent and a power bomb's explosion, as the box's restoring update does.</param>
/// <param name="MusicQueuePasses">Times this update processes the music queue; zero suppresses the ordinary per-update music processing.</param>
/// <param name="SoundQueuePasses">Times this update processes the sound queues, including the extra time when the box returns to gameplay.</param>
[IndependentFlag("whether this message-box update advances the liquid wave and statue descent, a fact of the update")]
public readonly record struct MessageBoxFrameAudio(bool AdvancesLiquidWaveAndStatueDescent, int MusicQueuePasses, int SoundQueuePasses)
{
    /// <summary>Ordinary message update: music and sound processed once each, and no liquid wave or statue advance; describes the processing for the host rather than performing it.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}
