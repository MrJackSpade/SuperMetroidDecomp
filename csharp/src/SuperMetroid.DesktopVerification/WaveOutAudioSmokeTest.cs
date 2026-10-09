using System.Diagnostics;

namespace SuperMetroid.Desktop;

/// <summary>Result of a host-device queue/backpressure diagnostic.</summary>
/// <param name="BuffersSubmitted">Number of silent PCM blocks submitted to the host device.</param>
/// <param name="SubmissionTime">Elapsed time spent submitting those blocks, excluding the final queue drain.</param>
public readonly record struct WaveOutAudioSmokeTestResult(
    int BuffersSubmitted,
    TimeSpan SubmissionTime);

/// <summary>
/// Exercises Windows PCM ownership independently of the ROM and translated SPC program.
/// </summary>
public static class WaveOutAudioSmokeTest
{
    /// <summary>PCM sample rate used to construct the Windows waveOut device.</summary>
    private const int SampleRate = 48_000;

    /// <summary>Number of interleaved PCM channels submitted for each audio frame.</summary>
    private const int ChannelCount = 2;

    /// <summary>Number of stereo sample frames corresponding to one 60 Hz video frame.</summary>
    private const int StereoFramesPerVideoFrame = SampleRate / 60;

    /// <summary>Default submission count, three times the six-buffer hardware queue capacity.</summary>
    private const int DefaultBuffersToSubmit = 18;

    /// <summary>
    /// Submits three times the live device's six-buffer capacity without dropping a block.
    /// </summary>
    /// <remarks>
    /// Silence prevents a command-line diagnostic from producing an unpleasant tone. waveOut
    /// still transfers and times every PCM byte, so submissions seven through eighteen must
    /// cross the same full-queue path that previously threw during playable startup.
    /// </remarks>
    public static WaveOutAudioSmokeTestResult Run(
        int buffersToSubmit = DefaultBuffersToSubmit)
    {
        if (buffersToSubmit <= WaveOutAudioPolicy.HardwareBufferCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(buffersToSubmit),
                "The diagnostic must exceed the live six-buffer queue capacity.");
        }

        short[] silence = new short[StereoFramesPerVideoFrame * ChannelCount];
        using (var startup = new WaveOutAudioDevice(
            SampleRate,
            ChannelCount,
            silence.Length))
        {
            startup.Submit(silence);
            startup.WaitForPendingSubmissions();
            int expectedPrepared = WaveOutAudioPolicy.PrerollSilenceBufferCount + 1;
            if (startup.PreparedBufferCountForVerification != expectedPrepared)
            {
                throw new InvalidDataException(
                    $"The first emulated PCM frame prepared " +
                    $"{startup.PreparedBufferCountForVerification} waveOut buffers; " +
                    $"expected {expectedPrepared} including startup scheduling reserve.");
            }
        }

        using var device = new WaveOutAudioDevice(
            SampleRate,
            ChannelCount,
            silence.Length);

        Stopwatch elapsed = Stopwatch.StartNew();
        for (int index = 0; index < buffersToSubmit; index++)
            device.Submit(silence);
        elapsed.Stop();
        if (elapsed.ElapsedMilliseconds >= 100)
        {
            throw new InvalidDataException(
                $"Submitting {buffersToSubmit} PCM frames blocked the caller for " +
                $"{elapsed.ElapsedMilliseconds} ms; waveOut pacing escaped its worker.");
        }
        device.WaitForPendingSubmissions();

        return new WaveOutAudioSmokeTestResult(buffersToSubmit, elapsed.Elapsed);
    }
}
