namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The music and sound processing one update of an open message box performs, and whether it advances the liquid wave
/// and statue descent (#2055). While a box is shown gameplay does not run, so the music queue, the sound queues and the
/// liquid's advance run only as the box's update says; when they run decides when queued sounds are accepted and where the
/// liquid wave's swap of the random number falls.
/// </summary>
/// <remarks>
/// Read by the frame's audio advance after every update of an open box. <see cref="MessageBoxPauses"/> states it for the
/// updates paused before the window opens and after it closes; every other update of the box runs music and sound once
/// (<see cref="MusicAndSounds"/>), except the update showing a moved save choice, which runs neither. Evidence: the original
/// message box's display waits each process the music and sound queues once, the choice toggle's redraw wait
/// (<c>$85:8532</c>) processes neither, and the SMV replays accept the box's sounds on the native recordings' frames.
/// </remarks>
/// <param name="AdvancesLiquidWaveAndStatueDescent">Whether this update advances the liquid wave, the statue descent and a power bomb's explosion, as one update after the box closes does.</param>
/// <param name="MusicQueuePasses">Times this update processes the music queue; zero suppresses the ordinary per-update music processing.</param>
/// <param name="SoundQueuePasses">Times this update processes the sound queues, including the extra time when the box returns to gameplay.</param>
public readonly partial record struct MessageBoxFrameAudio(bool AdvancesLiquidWaveAndStatueDescent, int MusicQueuePasses, int SoundQueuePasses)
{
    /// <summary>Ordinary message update: music and sound processed once each, and no liquid wave or statue advance; describes the processing for the host rather than performing it.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}
