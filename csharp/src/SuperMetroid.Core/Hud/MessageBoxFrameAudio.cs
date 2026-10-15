namespace SuperMetroid.Core.Hud;

/// <summary>Audio and HDMA-object handler calls made by one bank-$85 frame.</summary>
/// <param name="RunsHdmaObjects">Whether the host must run the HDMA-object handler for this accepted message update, notably at the PPU restoration seam.</param>
/// <param name="MusicHandlerCalls">Number of native music-queue handler calls represented by this update; zero suppresses the ordinary host music call.</param>
/// <param name="SoundHandlerCalls">Number of native sound-handler calls represented by this update, including the extra dispatch call when the coroutine returns.</param>
public readonly record struct MessageBoxFrameAudio(bool RunsHdmaObjects, int MusicHandlerCalls, int SoundHandlerCalls)
{
    /// <summary>Ordinary message-update descriptor: one music handler, one sound handler, and no HDMA-object handler; describes calls for the host rather than executing them.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}
