using System.Buffers.Binary;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>The tree's sounds and music: each a WAV file of sixteen-bit stereo samples, which says where it loops when it does.</summary>
internal sealed partial class EngineAssetTree
{
    /// <summary>
    /// Writes a sound. One that goes on and on says so in the file itself: from <paramref name="loopStart"/>, a
    /// count of frames from its start, to its end it is played over and over.
    /// </summary>
    /// <remarks>
    /// A looping sound's file holds one frame more than the sound: the loop's first frame again, after its last, and
    /// the loop is written as ending on that frame. Godot's player (4.7.2, <c>AudioStreamPlaybackWAV::_mix_internal</c>)
    /// plays up to and including the end it is given and then goes on from the frame after the loop's start, or, when
    /// the buffer it is filling ends just before the end, leaves the end out and goes on from the loop's start itself.
    /// Either way one frame of the two is left out, so with the end on the sound's own last frame each turn of the loop
    /// would be a frame short. With the first frame written again as the end, either way plays every frame of the loop
    /// once a turn.
    /// </remarks>
    /// <param name="path">The sound's path within the tree.</param>
    /// <param name="sampleRate">How many frames of it are played a second.</param>
    /// <param name="samples">Its samples, left then right for each frame.</param>
    /// <param name="loopStart">The frame it loops back to from its end, or null for a sound played once.</param>
    /// <exception cref="ArgumentException">The samples are not whole frames, there are none, or the loop starts outside them.</exception>
    internal void WriteSound(string path, int sampleRate, ReadOnlySpan<short> samples, long? loopStart)
    {
        const int channels = 2, bytesPerSample = sizeof(short), headerBytes = 44, loopBytes = 8 + 36 + 24;
        if (samples.Length == 0 || samples.Length % channels != 0)
            throw new ArgumentException($"The sound {path} has {samples.Length} samples, which is no whole number of stereo frames.", nameof(samples));
        int frames = samples.Length / channels;
        if (loopStart is { } start && (start < 0 || start >= frames))
            throw new ArgumentException($"The sound {path} loops from frame {start} of {frames}.", nameof(loopStart));
        int writtenFrames = frames + (loopStart is null ? 0 : 1);
        int dataBytes = writtenFrames * channels * bytesPerSample;
        byte[] file = new byte[headerBytes + dataBytes + (loopStart is null ? 0 : loopBytes)];
        Span<byte> bytes = file;
        "RIFF"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], file.Length - 8);
        "WAVE"u8.CopyTo(bytes[8..]);
        "fmt "u8.CopyTo(bytes[12..]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[16..], 16);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[20..], 1);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[22..], channels);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[24..], sampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[28..], sampleRate * channels * bytesPerSample);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[32..], channels * bytesPerSample);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[34..], bytesPerSample * 8);
        "data"u8.CopyTo(bytes[36..]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[40..], dataBytes);
        for (int sample = 0; sample < samples.Length; sample++)
            BinaryPrimitives.WriteInt16LittleEndian(bytes[(headerBytes + sample * bytesPerSample)..], samples[sample]);

        if (loopStart is { } loopsFrom)
        {
            // The loop's first frame again, after the sound's last.
            for (int channel = 0; channel < channels; channel++)
            {
                BinaryPrimitives.WriteInt16LittleEndian(
                    bytes[(headerBytes + (frames * channels + channel) * bytesPerSample)..], samples[(int)loopsFrom * channels + channel]);
            }

            // The sampler chunk: nine numbers about the sample, of which only the count of loops matters here, and
            // one loop, played forward, from its first frame to that frame written again.
            Span<byte> loop = bytes[(headerBytes + dataBytes)..];
            "smpl"u8.CopyTo(loop);
            BinaryPrimitives.WriteInt32LittleEndian(loop[4..], 36 + 24);
            BinaryPrimitives.WriteInt32LittleEndian(loop[16..], 1_000_000_000 / sampleRate);
            BinaryPrimitives.WriteInt32LittleEndian(loop[20..], 60);
            BinaryPrimitives.WriteInt32LittleEndian(loop[36..], 1);
            BinaryPrimitives.WriteInt32LittleEndian(loop[52..], (int)loopsFrom);
            BinaryPrimitives.WriteInt32LittleEndian(loop[56..], frames);
        }

        Write(path, file);
    }
}
