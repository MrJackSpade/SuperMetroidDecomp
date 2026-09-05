namespace SuperMetroid.Core.Audio;

/// <summary>Host-rate conversion for one interleaved stereo S-DSP output block.</summary>
internal static class PcmFrameResampler
{
    /// <summary>
    /// Linearly interpolates the native DSP samples at the host-frame positions. The old
    /// nearest-neighbor path duplicated roughly one third of all 48-kHz samples when converting
    /// Super Metroid's 534-sample frame, producing content-dependent high-frequency imaging that
    /// is especially exposed after pause cancels the foreground sound-effect voices.
    /// </summary>
    public static void ResampleStereoLinear(
        ReadOnlySpan<short> source,
        Span<short> destination)
    {
        if (source.Length == 0 || (source.Length & 1) != 0)
            throw new ArgumentException("Source PCM must contain complete stereo frames.", nameof(source));
        if (destination.Length == 0 || (destination.Length & 1) != 0)
        {
            throw new ArgumentException(
                "Destination PCM must contain complete stereo frames.",
                nameof(destination));
        }

        int sourceFrames = source.Length / 2;
        int destinationFrames = destination.Length / 2;
        for (int destinationFrame = 0; destinationFrame < destinationFrames; destinationFrame++)
        {
            long sourcePosition = (long)destinationFrame * sourceFrames;
            int leftFrame = unchecked((int)(sourcePosition / destinationFrames));
            int fraction = unchecked((int)(sourcePosition % destinationFrames));
            int rightFrame = Math.Min(leftFrame + 1, sourceFrames - 1);
            for (int channel = 0; channel < 2; channel++)
            {
                int left = source[leftFrame * 2 + channel];
                int right = source[rightFrame * 2 + channel];
                int interpolated = left + unchecked((int)(((long)(right - left) * fraction) /
                    destinationFrames));
                destination[destinationFrame * 2 + channel] = unchecked((short)interpolated);
            }
        }
    }
}
