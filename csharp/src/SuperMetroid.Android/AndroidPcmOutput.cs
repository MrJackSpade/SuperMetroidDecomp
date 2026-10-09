using Android.Media;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Android;

/// <summary>
/// Android-only PCM sink. A bounded worker isolates AudioTrack's bursty blocking
/// writes. The emulation owner submits copied frames and drains before disposal.
/// </summary>
internal sealed class AndroidPcmOutput : IDisposable
{
    /// <summary>
    /// The streaming Android audio track that receives interleaved stereo PCM.
    /// </summary>
    private readonly AudioTrack track;

    /// <summary>
    /// Queues copied video-frame audio and drains it through the device writer on its worker.
    /// </summary>
    private readonly QueuedPcmSink sink;

    /// <summary>
    /// Records track startup and periodic write diagnostics for Android audio recovery.
    /// </summary>
    private readonly AndroidResumeTrace trace;

    /// <summary>
    /// Cumulative number of PCM samples successfully handed to the track, used to annotate write traces.
    /// </summary>
    private long writtenSamples;

    /// <summary>
    /// Creates and starts a low-latency stereo PCM track with a bounded queue for frame submissions.
    /// </summary>
    /// <param name="trace">The diagnostic trace that receives startup and write measurements.</param>
    public AndroidPcmOutput(AndroidResumeTrace trace)
    {
        this.trace = trace;
        int minimumBytes = AudioTrack.GetMinBufferSize(CartridgeAudioRenderer.SampleRate,
            ChannelOut.Stereo, Encoding.Pcm16bit);
        if (minimumBytes <= 0) throw new IOException($"AudioTrack minimum buffer query failed: {minimumBytes}.");
        using var attributes = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Game)!
            .SetContentType(AudioContentType.Music)!.Build()!;
        using var format = new AudioFormat.Builder()
            .SetSampleRate(CartridgeAudioRenderer.SampleRate)!
            .SetEncoding(Encoding.Pcm16bit)!
            .SetChannelMask(ChannelOut.Stereo)!.Build()!;
        using var builder = new AudioTrack.Builder();
        // Request the interactive route rather than the device's deep-buffer music
        // route. Android may still report None if a fast track is not granted; keep
        // the effective mode in diagnostics instead of assuming the hint was honored.
        track = builder.SetAudioAttributes(attributes)!
            .SetAudioFormat(format)!
            .SetTransferMode(AudioTrackMode.Stream)!
            .SetPerformanceMode(AudioTrackPerformanceMode.LowLatency)!
            .SetBufferSizeInBytes(Math.Max(minimumBytes,
                CartridgeAudioRenderer.StereoFramesPerVideoFrame * CartridgeAudioRenderer.ChannelCount * sizeof(short) * 3))!
            .Build();
        if (track.State != AudioTrackState.Initialized)
        {
            track.Dispose();
            throw new IOException("AudioTrack did not initialize.");
        }
        track.Play();
        trace.Record("play", $"bufferFrames={track.BufferSizeInFrames} mode={track.PerformanceMode} head={track.PlaybackHeadPosition}");
        // Some routes release AudioTrack space in large bursts. Eight
        // video-frame buffers absorb batching without unbounded latency;
        // the simulation still owns its ordinary 60Hz deadline and never drops PCM.
        sink = new QueuedPcmSink(WriteToDevice, capacity: 8);
    }

    /// <summary>
    /// Gets the underrun count reported by the active Android audio track.
    /// </summary>
    public int UnderrunCount => track.UnderrunCount;

    /// <summary>
    /// Gets the number of submitted audio frames still waiting in the bounded PCM queue.
    /// </summary>
    public int PendingFrameCount => sink.PendingFrameCount;

    /// <summary>
    /// Submits a copied PCM frame to the bounded worker queue for playback.
    /// </summary>
    /// <param name="samples">Interleaved stereo PCM samples for one rendered frame.</param>
    public void Submit(short[] samples) => sink.Submit(samples);

    /// <summary>
    /// Writes all samples in one queued buffer to the track, retrying partial writes until it is accepted.
    /// </summary>
    /// <param name="samples">The interleaved PCM buffer dequeued for device playback.</param>
    private void WriteToDevice(short[] samples)
    {
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        int offset = 0;
        while (offset < samples.Length)
        {
            int written = track.Write(samples, offset, samples.Length - offset, WriteMode.Blocking);
            if (written <= 0) throw new IOException($"AudioTrack failed to accept PCM: {written}.");
            offset += written;
        }
        writtenSamples += samples.Length;
        if (!trace.Full)
            trace.Record("write", $"start={start} samples={writtenSamples} head={track.PlaybackHeadPosition} underruns={track.UnderrunCount}");
    }

    /// <summary>
    /// Drains and stops the queued writer, then pauses, flushes, releases, and disposes the Android track.
    /// </summary>
    public void Dispose()
    {
        try { sink.Dispose(); }
        finally
        {
            track.Pause();
            track.Flush();
            track.Release();
            track.Dispose();
        }
    }
}
