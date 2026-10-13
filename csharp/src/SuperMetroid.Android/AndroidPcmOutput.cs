using Android.Media;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Android;

/// <summary>
/// Android-only PCM sink. A bounded worker isolates AudioTrack's bursty blocking
/// writes. The emulation owner submits copied frames and drains before disposal.
/// </summary>
internal sealed class AndroidPcmOutput : IDisposable
{
    /// <summary>Android playback device configured for streaming stereo PCM.</summary>
    private readonly AudioTrack track;
    /// <summary>Bounded worker queue that absorbs bursty blocking writes.</summary>
    private readonly QueuedPcmSink sink;
    /// <summary>Playback diagnostics used during Android resume investigation.</summary>
    private readonly AndroidResumeTrace trace;
    /// <summary>Cumulative accepted sample count recorded alongside the playback head.</summary>
    private long writtenSamples;

    /// <summary>Starts a low-latency AudioTrack and an eight-video-frame PCM worker queue.</summary>
    public AndroidPcmOutput(AndroidResumeTrace trace)
    {
        this.trace = trace;
        int minimumBytes = AudioTrack.GetMinBufferSize(CartridgeAudioRenderer.SampleRate,
            ChannelOut.Stereo, Encoding.Pcm16bit);
        if (minimumBytes <= 0) throw new IOException($"AudioTrack minimum buffer query failed: {minimumBytes}.");
        using var attributesBuilder = new AudioAttributes.Builder();
        attributesBuilder.SetUsage(AudioUsageKind.Game);
        attributesBuilder.SetContentType(AudioContentType.Music);
        using var attributes = attributesBuilder.Build() ??
            throw new IOException("Android could not build the game audio attributes.");
        using var formatBuilder = new AudioFormat.Builder();
        formatBuilder.SetSampleRate(CartridgeAudioRenderer.SampleRate);
        formatBuilder.SetEncoding(Encoding.Pcm16bit);
        formatBuilder.SetChannelMask(ChannelOut.Stereo);
        using var format = formatBuilder.Build() ??
            throw new IOException("Android could not build the PCM audio format.");
        using var builder = new AudioTrack.Builder();
        // Request the interactive route rather than the device's deep-buffer music
        // route. Android may still report None if a fast track is not granted; keep
        // the effective mode in diagnostics instead of assuming the hint was honored.
        track = builder.SetAudioAttributes(attributes)
            .SetAudioFormat(format)
            .SetTransferMode(AudioTrackMode.Stream)
            .SetPerformanceMode(AudioTrackPerformanceMode.LowLatency)
            .SetBufferSizeInBytes(Math.Max(minimumBytes,
                CartridgeAudioRenderer.StereoFramesPerVideoFrame * CartridgeAudioRenderer.ChannelCount * sizeof(short) * 3))
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

    /// <summary>Device-reported playback underruns observed since track creation.</summary>
    public int UnderrunCount => track.UnderrunCount;
    /// <summary>Video-frame PCM buffers still awaiting the worker's device write.</summary>
    public int PendingFrameCount => sink.PendingFrameCount;

    /// <summary>Copies a submitted frame into the bounded queue for ordered playback.</summary>
    public void Submit(short[] samples) => sink.Submit(samples);

    /// <summary>Writes the full buffer despite partial AudioTrack accepts and records playback diagnostics.</summary>
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

    /// <summary>Drains the queued sink before pausing, flushing, and releasing AudioTrack.</summary>
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
