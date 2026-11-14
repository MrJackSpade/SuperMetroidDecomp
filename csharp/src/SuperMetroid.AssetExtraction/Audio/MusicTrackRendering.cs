using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Renders an analyzed music track's PCM (#1305). Every render loads the banks on frame 0 and
/// selects the track on frame 1, as <see cref="MusicTrackAnalysis.Analyze"/> does, so sample
/// positions agree with the analysis.
/// </summary>
internal static class MusicTrackRendering
{
    /// <summary>
    /// Renders the exported clip of <paramref name="track"/>: from the tick that started it to its
    /// E2 for a looping track, whose loop is [E1, E2), or to the tick that ended it. Every music
    /// loop that repeats its tempo phase repeats [E1, E2) sample-exactly.
    /// </summary>
    /// <returns>Interleaved native stereo PCM whose frame zero is the start tick.</returns>
    internal static short[] RenderClip(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track) =>
        Render(catalog, track, track.StartSample, track.ClipEndSample);

    /// <summary>
    /// Renders whole frames from frame 0 until stereo frame <paramref name="end"/> exists, keeping
    /// stereo frames [<paramref name="start"/>, <paramref name="end"/>) in a buffer sized once.
    /// </summary>
    /// <returns>Interleaved native stereo PCM whose frame zero is native stereo frame <paramref name="start"/>.</returns>
    internal static short[] Render(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track, long start, long end)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        MusicTrackAnalysis.QueueBanks(renderer, track.Bank);
        short[] clip = new short[checked((int)((end - start) * 2))];
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        renderer.RenderFrame(frame);
        long rendered = Keep(frame, 0, clip, start * 2);
        renderer.PlayTrack(MusicTrackNumbers.Decode(track.Bank, track.Track));
        while (rendered < end * 2)
        {
            renderer.RenderFrame(frame);
            rendered = Keep(frame, rendered, clip, start * 2);
        }
        return clip;
    }

    /// <summary>
    /// Copies the part of a rendered frame, whose first interleaved sample is
    /// <paramref name="frameFirst"/>, that falls in the clip, whose first is
    /// <paramref name="clipFirst"/>, into the clip.
    /// </summary>
    /// <returns>The interleaved sample position after the frame.</returns>
    private static long Keep(ReadOnlySpan<short> frame, long frameFirst, Span<short> clip, long clipFirst)
    {
        long from = Math.Max(frameFirst, clipFirst);
        long to = Math.Min(frameFirst + frame.Length, clipFirst + clip.Length);
        if (from < to)
            frame[(int)(from - frameFirst)..(int)(to - frameFirst)].CopyTo(clip[(int)(from - clipFirst)..]);
        return frameFirst + frame.Length;
    }
}
