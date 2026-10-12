using Android.Media;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Android;

/// <summary>
/// Android-only PCM sink. A bounded worker isolates AudioTrack's bursty blocking
/// writes. The emulation owner submits copied frames and drains before disposal.
/// </summary>
internal sealed class AndroidPcmOutput : IDisposable
{
    /// <summary>Stores track.</summary>
    private readonly AudioTrack track;
    /// <summary>Stores sink.</summary>
    private readonly QueuedPcmSink sink;
    /// <summary>Stores trace.</summary>
    private readonly AndroidResumeTrace trace;
    /// <summary>Stores written samples.</summary>
    private long writtenSamples;

    /// <summary>Initializes a new android pcm output instance.</summary>
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

    /// <summary>Gets underrun count.</summary>
    public int UnderrunCount => track.UnderrunCount;
    /// <summary>Gets pending frame count.</summary>
    public int PendingFrameCount => sink.PendingFrameCount;

    /// <summary>Submit.</summary>
    public void Submit(short[] samples) => sink.Submit(samples);

    /// <summary>Writes to device.</summary>
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

    /// <summary>Disposes.</summary>
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
