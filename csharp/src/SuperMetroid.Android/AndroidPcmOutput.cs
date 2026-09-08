using Android.Media;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Android;

/// <summary>
/// Android-only PCM sink. A bounded worker isolates AudioTrack's bursty blocking
/// writes. The emulation owner submits copied frames and drains before disposal.
/// </summary>
internal sealed class AndroidPcmOutput : IDisposable
{
    private readonly AudioTrack track;
    private readonly QueuedPcmSink sink;

    public AndroidPcmOutput()
    {
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
        track = builder.SetAudioAttributes(attributes)!
            .SetAudioFormat(format)!
            .SetTransferMode(AudioTrackMode.Stream)!
            .SetBufferSizeInBytes(Math.Max(minimumBytes,
                CartridgeAudioRenderer.StereoFramesPerVideoFrame * CartridgeAudioRenderer.ChannelCount * sizeof(short) * 3))!
            .Build();
        if (track.State != AudioTrackState.Initialized)
        {
            track.Dispose();
            throw new IOException("AudioTrack did not initialize.");
        }
        track.Play();
        // This device releases AudioTrack space in roughly 67ms bursts. Eight
        // video-frame buffers absorb that batching without unbounded latency;
        // the simulation still owns its ordinary 60Hz deadline and never drops PCM.
        sink = new QueuedPcmSink(WriteToDevice, capacity: 8);
    }

    public int UnderrunCount => track.UnderrunCount;

    public void Submit(short[] samples) => sink.Submit(samples);

    private void WriteToDevice(short[] samples)
    {
        int offset = 0;
        while (offset < samples.Length)
        {
            int written = track.Write(samples, offset, samples.Length - offset, WriteMode.Blocking);
            if (written <= 0) throw new IOException($"AudioTrack failed to accept PCM: {written}.");
            offset += written;
        }
    }

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
