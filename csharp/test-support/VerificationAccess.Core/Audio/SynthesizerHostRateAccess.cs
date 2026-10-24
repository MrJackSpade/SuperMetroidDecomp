using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>
/// The emulated driver's 48-kHz output for verification (#1305). The offline renderer produces
/// native 32.04-kHz frames only; these members resample them with the runtime's
/// <see cref="PcmFrameResampler"/>, exactly as the driver's host output did, so the pinned PCM
/// corpora keep their bytes.
/// </summary>
internal static class SynthesizerHostRateAccess
{
    /// <summary>Host stereo frames per 60-Hz update.</summary>
    private const int HostStereoFramesPerVideoFrame = CartridgeAudioSession.StereoFramesPerVideoFrame;

    /// <summary>Host-rate constants the driver definitions no longer carry.</summary>
    extension(SpcDriverDefinitions)
    {
        /// <summary>Eight hundred host stereo frames per 60-Hz update.</summary>
        internal static int HostStereoFramesPerVideoFrame => HostStereoFramesPerVideoFrame;
    }

    /// <summary>The backend's host-rate render.</summary>
    extension(ManagedSpcAudioBackend self)
    {
        /// <summary>Renders the update natively and resamples it to 48-kHz interleaved stereo.</summary>
        internal void Render(in AudioFrameEvents frame, Span<short> stereo48k)
        {
            short[] native = new short[SpcPortDefinitions.NativeStereoFramesPerVideoFrame * 2];
            self.RenderNative(frame, native);
            PcmFrameResampler.ResampleStereoLinear(native, stereo48k[..(HostStereoFramesPerVideoFrame * 2)]);
        }
    }

    /// <summary>The player's host-rate frame.</summary>
    extension(ManagedSpcPlayer self)
    {
        /// <summary>Advances one frame natively and resamples it to 48-kHz interleaved stereo.</summary>
        internal void GenerateFrame(ReadOnlySpan<SoundClockStep<SoundEffectId>> schedule, Span<short> destination)
        {
            if (destination.Length < HostStereoFramesPerVideoFrame * 2)
                throw new ArgumentException($"Audio destination requires {HostStereoFramesPerVideoFrame * 2} samples.", nameof(destination));
            short[] native = new short[SpcPortDefinitions.NativeStereoFramesPerVideoFrame * 2];
            self.GenerateNativeFrame(schedule, native);
            PcmFrameResampler.ResampleStereoLinear(native, destination[..(HostStereoFramesPerVideoFrame * 2)]);
        }
    }

    /// <summary>The DSP's host-rate copy.</summary>
    extension(ManagedSnesDsp self)
    {
        /// <summary>Resamples the completed native frame to <paramref name="stereoFrameCount"/> host frames and starts the next frame.</summary>
        internal void CopyResampledSamples(Span<short> destination, int stereoFrameCount)
        {
            if (stereoFrameCount <= 0 || destination.Length < stereoFrameCount * 2)
                throw new ArgumentOutOfRangeException(nameof(stereoFrameCount));
            short[] native = new short[SpcPortDefinitions.NativeStereoFramesPerVideoFrame * 2];
            self.CopyNativeSamples(native);
            PcmFrameResampler.ResampleStereoLinear(native, destination[..(stereoFrameCount * 2)]);
        }
    }
}
