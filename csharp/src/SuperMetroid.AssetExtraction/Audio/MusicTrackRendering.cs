using SuperMetroid.Core.Audio;
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
    internal static short[] RenderClip(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track)
    {
        long end = track.ClipEndSample;
        short[] pcm = Render(catalog, track, end);
        return pcm[(int)(track.StartSample * 2)..(int)(end * 2)];
    }

    /// <summary>Renders whole frames from frame 0 until at least <paramref name="length"/> stereo frames exist.</summary>
    /// <returns>Interleaved native stereo PCM from sample 0.</returns>
    internal static short[] Render(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track, long length)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        MusicTrackAnalysis.QueueBanks(renderer, track.Bank);
        var pcm = new List<short>(checked((int)(length * 2)) + SpcAudioClipRenderer.NativeSamplesPerFrame);
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        renderer.RenderFrame(frame);
        pcm.AddRange(frame);
        renderer.PlayTrack(new MusicTrack(track.Track));
        while (pcm.Count < length * 2)
        {
            renderer.RenderFrame(frame);
            pcm.AddRange(frame);
        }
        return [.. pcm];
    }
}
