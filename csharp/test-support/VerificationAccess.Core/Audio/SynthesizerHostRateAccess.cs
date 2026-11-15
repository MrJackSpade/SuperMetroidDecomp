using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>
/// The emulated driver's output at the session's PCM rate for verification (#1305, #2057). The session publishes its PCM
/// at the clips' own rate, the driver's native 32.04-kHz frames, so the driver's frames are the session's frames as they
/// are; the host resamples both to its device.
/// </summary>
internal static class SynthesizerHostRateAccess
{
    /// <summary>The session's stereo frames per 60-Hz update.</summary>
    private const int HostStereoFramesPerVideoFrame = AudioSession.StereoFramesPerVideoFrame;

    /// <summary>Host-rate constants the driver definitions no longer carry.</summary>
    extension(SpcDriverDefinitions)
    {
        /// <summary>The session's stereo frames per 60-Hz update, the driver's native frames.</summary>
        internal static int HostStereoFramesPerVideoFrame => HostStereoFramesPerVideoFrame;
    }

    /// <summary>The backend's session-rate render.</summary>
    extension(ManagedSpcAudioBackend self)
    {
        /// <summary>Renders the update's interleaved stereo at the session's rate.</summary>
        internal void Render(in AudioFrameEvents frame, Span<short> stereo) =>
            self.RenderNative(frame, stereo[..(HostStereoFramesPerVideoFrame * 2)]);
    }

    /// <summary>The player's session-rate frame.</summary>
    extension(ManagedSpcPlayer self)
    {
        /// <summary>Advances one frame and writes its interleaved stereo at the session's rate.</summary>
        internal void GenerateFrame(ReadOnlySpan<SoundClockStep<SoundEffectLibrary, byte>> schedule, Span<short> destination)
        {
            if (destination.Length < HostStereoFramesPerVideoFrame * 2)
                throw new ArgumentException($"Audio destination requires {HostStereoFramesPerVideoFrame * 2} samples.", nameof(destination));
            self.GenerateNativeFrame(schedule, destination[..(HostStereoFramesPerVideoFrame * 2)]);
        }
    }
}
