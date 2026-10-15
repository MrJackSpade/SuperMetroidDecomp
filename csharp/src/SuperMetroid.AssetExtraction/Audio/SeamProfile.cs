namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// How a candidate loop body differs from the audio the cartridge plays in its place (#1305):
/// the number of differing stereo frames, the first and last of them relative to the loop start,
/// the largest sample difference, and the largest difference in the first 64 stereo frames, which
/// is the click a listener hears at the wrap.
/// </summary>
/// <param name="DifferingFrames">Stereo frames with any differing channel; zero when sample-exact.</param>
/// <param name="FirstDifference">First differing stereo frame, or -1 when sample-exact.</param>
/// <param name="LastDifference">Last differing stereo frame, or -1 when sample-exact.</param>
/// <param name="MaximumDifference">Largest absolute sample difference.</param>
/// <param name="WrapDifference">Largest absolute sample difference within the first <see cref="WrapFrames"/> stereo frames.</param>
internal sealed record SeamProfile(
    long DifferingFrames,
    long FirstDifference,
    long LastDifference,
    int MaximumDifference,
    int WrapDifference)
{
    /// <summary>Stereo frames after the wrap whose difference is reported separately: one 2-ms driver tick.</summary>
    internal const int WrapFrames = 64;

    /// <summary>Whether the candidate loop repeats the cartridge's audio sample-exactly.</summary>
    internal bool Exact => DifferingFrames == 0;

    /// <summary>Compares <paramref name="length"/> stereo frames of <paramref name="pcm"/> at two stereo-frame offsets.</summary>
    internal static SeamProfile Compare(IReadOnlyList<short> pcm, long loopStart, long nextStart, long length)
    {
        long differing = 0;
        long first = -1;
        long last = -1;
        int maximum = 0;
        int wrap = 0;
        for (long frame = 0; frame < length; frame++)
        {
            int left = Math.Abs(pcm[(int)((loopStart + frame) * 2)] - pcm[(int)((nextStart + frame) * 2)]);
            int right = Math.Abs(pcm[(int)((loopStart + frame) * 2 + 1)] - pcm[(int)((nextStart + frame) * 2 + 1)]);
            int difference = Math.Max(left, right);
            if (difference == 0)
                continue;
            differing++;
            if (first < 0)
                first = frame;
            last = frame;
            maximum = Math.Max(maximum, difference);
            if (frame < WrapFrames)
                wrap = Math.Max(wrap, difference);
        }
        return new SeamProfile(differing, first, last, maximum, wrap);
    }
}
