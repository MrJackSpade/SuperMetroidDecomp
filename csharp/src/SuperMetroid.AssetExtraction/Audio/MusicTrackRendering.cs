using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// PCM-level measurements of an analyzed music track (#1305): the canonical render, whether the
/// loop seam repeats sample-exactly, and whether a perturbed entry-state group changes output.
/// All renders load the banks on frame 0 and select the track on frame 1, as
/// <see cref="MusicTrackAnalysis.Analyze"/> does, so sample positions agree.
/// </summary>
internal static class MusicTrackRendering
{
    /// <summary>
    /// Renders <paramref name="track"/> from the canonical post-upload state through one loop
    /// body past E3 (looping) or to its end (ending). An ended track's echo may keep ringing, so
    /// the render does not wait for silence.
    /// </summary>
    /// <returns>Interleaved native stereo PCM from sample 0.</returns>
    internal static short[] RenderCanonical(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track)
    {
        long length = track.Shape switch
        {
            MusicTrackShape.Loops => track.Arrivals[2] + track.LoopSamples,
            MusicTrackShape.Ends => track.IntroSamples,
            _ => throw new ArgumentException($"Track {track.Track} has no period to render.", nameof(track)),
        };
        return Render(catalog, track, length);
    }

    /// <summary>
    /// Compares the loop body [E2, E3) with what follows it, [E3, E3 + loop), in <paramref name="pcm"/>:
    /// the error of looping [E2, E3) where the cartridge plays its next pass.
    /// </summary>
    internal static SeamProfile CompareSeam(MusicTrackAnalysis track, short[] pcm) =>
        SeamProfile.Compare(pcm, track.Arrivals[1], track.Arrivals[2], track.LoopSamples);

    /// <summary>
    /// Compares [E1, E2) with [E2, E3): the error of the shorter clip, intro [0, E1) and loop
    /// [E1, E2), whose loop carries the previous pass's tails instead of its own.
    /// </summary>
    internal static SeamProfile CompareEarlySeam(MusicTrackAnalysis track, short[] pcm) =>
        SeamProfile.Compare(pcm, track.Arrivals[0], track.Arrivals[1], track.Arrivals[1] - track.Arrivals[0]);

    /// <summary>
    /// Runs <paramref name="donor"/> alone to the frame holding its E2 (or its end), returning its
    /// driver as an entry-state source.
    /// </summary>
    internal static ManagedSpcPlayer DonorAtLoopPoint(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis donor)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        MusicTrackAnalysis.QueueBanks(renderer, donor.Bank);
        renderer.AdvanceSequencerFrame();
        renderer.WriteMusicPort(unchecked((byte)donor.Track));
        long target = donor.IntroSamples;
        while (renderer.Frame * ManagedSnesDsp.NativeStereoFramesPerVideoFrame <= target)
            renderer.AdvanceSequencerFrame();
        return renderer.Player;
    }

    /// <summary>
    /// Renders <paramref name="track"/> with <paramref name="group"/> copied from
    /// <paramref name="donor"/> just before the track is selected, and compares it with the
    /// canonical render over the track's intro and first loop body (or to its end).
    /// </summary>
    /// <returns>The first differing stereo frame, or null when identical.</returns>
    internal static long? FirstEntryDifference(
        ExtractedAudioAssetCatalog catalog,
        MusicTrackAnalysis track,
        short[] canonical,
        SpcEntryStateGroup group,
        ManagedSpcPlayer donor)
    {
        long compared = track.Shape == MusicTrackShape.Loops ? track.Arrivals[2] : track.IntroSamples;
        var renderer = new SpcAudioClipRenderer(catalog);
        MusicTrackAnalysis.QueueBanks(renderer, track.Bank);
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        renderer.RenderFrame(frame);
        long? first = FirstDifference(frame, canonical, 0, compared);
        if (first is not null)
            throw new InvalidOperationException("Entry renders diverged before the perturbation was applied.");
        renderer.Player.CopyEntryGroup(group, donor);
        renderer.WriteMusicPort(unchecked((byte)track.Track));
        while ((renderer.Frame * ManagedSnesDsp.NativeStereoFramesPerVideoFrame) < compared)
        {
            long offset = renderer.Frame * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
            renderer.RenderFrame(frame);
            first = FirstDifference(frame, canonical, offset, compared);
            if (first is not null)
                return first;
        }
        return null;
    }

    /// <summary>The first stereo frame at or after <paramref name="offset"/> where a rendered frame differs from <paramref name="reference"/>, within <paramref name="limit"/>.</summary>
    private static long? FirstDifference(short[] frame, short[] reference, long offset, long limit)
    {
        for (int index = 0; index < frame.Length; index++)
        {
            long sample = offset + index / 2;
            if (sample >= limit)
                return null;
            if (frame[index] != reference[offset * 2 + index])
                return sample;
        }
        return null;
    }

    /// <summary>Renders whole frames until at least <paramref name="length"/> stereo frames exist.</summary>
    private static short[] Render(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track, long length)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        MusicTrackAnalysis.QueueBanks(renderer, track.Bank);
        var pcm = new List<short>(checked((int)(length * 2)) + SpcAudioClipRenderer.NativeSamplesPerFrame);
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        renderer.RenderFrame(frame);
        pcm.AddRange(frame);
        renderer.WriteMusicPort(unchecked((byte)track.Track));
        while (pcm.Count < length * 2)
        {
            renderer.RenderFrame(frame);
            pcm.AddRange(frame);
        }
        return [.. pcm];
    }
}
