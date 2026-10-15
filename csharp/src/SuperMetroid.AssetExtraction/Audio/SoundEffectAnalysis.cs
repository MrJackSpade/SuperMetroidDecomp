using System.Security.Cryptography;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One sound effect rendered alone in one bank context (#1305), from the driver's power-on state
/// with no music: when the driver starts it, when the library's logical end comes (priority and
/// current sound reset), where a forever-repeating sound's state repeats, the voices it keys on
/// and a hash of its PCM. Sample positions are native stereo frames from the start event.
/// </summary>
/// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
/// <param name="Library">The sound library whose port carried the request.</param>
/// <param name="Command">The request byte.</param>
/// <param name="ActiveSamples">Samples from the start to the logical end, or null for a sound that never ends.</param>
/// <param name="Arrivals">For a never-ending sound, E1, E2 and E3 of its first repeated poll state; empty otherwise.</param>
/// <param name="KeyOns">Each tick that keyed on one of the sound's voices: its sample and voice mask.</param>
/// <param name="ClipSamples">The rendered length: through the release after the end, or E3 plus one loop body.</param>
/// <param name="Seam">For a looping sound, how [E2, E3) differs from the following pass; null otherwise.</param>
/// <param name="ChannelLoops">
/// For a looping sound, each allocated channel's own first repeated state (E1, E2) relative to the
/// start, with its voice. The sound's joint period is their least common multiple; the exporter
/// renders each channel as its own looping layer.
/// </param>
/// <param name="PcmSha256">Hash of the rendered PCM, identifying identical clips across contexts.</param>
internal sealed record SoundEffectAnalysis(
    AudioBank? Context,
    SoundEffectLibrary Library,
    byte Command,
    long? ActiveSamples,
    IReadOnlyList<long> Arrivals,
    IReadOnlyList<(long Sample, byte Voices)> KeyOns,
    long ClipSamples,
    SeamProfile? Seam,
    IReadOnlyList<(long First, long Second, byte Voice)> ChannelLoops,
    string PcmSha256)
{
    /// <summary>
    /// Renders the request on <paramref name="library"/> after the common upload and
    /// <paramref name="context"/>'s upload, through the release after its end or, for a
    /// never-ending sound, through one whole-driver loop body past E3.
    /// </summary>
    internal static SoundEffectAnalysis Analyze(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        byte command,
        bool repeatsForever)
    {
        var observer = new SoundEffectObserver(SoundEffectLibraries.ToQueueIndex(library), command);
        SpcAudioClipRenderer renderer = SoundEffectRendering.Start(catalog, context, library, command, observer, synthesize: true);
        bool silentAfterEnd = false;
        List<short> pcm = SoundEffectRendering.RenderFromStart(renderer, observer, (collected, frame) =>
        {
            if (repeatsForever)
            {
                if (observer.EndSample is not null)
                    throw new InvalidDataException($"Forever-repeating sound {library} ${command:X2} reached its logical end.");
                return observer.Arrivals.Count == 3 && collected.Count >= (2 * observer.Arrivals[2] - observer.Arrivals[1]) * 2;
            }
            if (observer.EndSample is null)
                return false;
            if (silentAfterEnd)
                return true;
            silentAfterEnd = Array.TrueForAll(frame, sample => sample == 0);
            return false;
        });

        long startSample = observer.StartSample ?? throw new InvalidDataException($"Sound {library} ${command:X2} never started.");
        SeamProfile? seam = null;
        long clipSamples = pcm.Count / 2;
        if (repeatsForever)
        {
            long loopStart = observer.Arrivals[1];
            long loopLength = observer.Arrivals[2] - loopStart;
            seam = SeamProfile.Compare(pcm, loopStart, observer.Arrivals[2], loopLength);
            clipSamples = observer.Arrivals[2] + loopLength;
        }
        else
        {
            // The render ran to a silent frame after the release; the clip ends at its last sound.
            while (clipSamples > 0 && pcm[(int)(clipSamples * 2 - 1)] == 0 && pcm[(int)(clipSamples * 2 - 2)] == 0)
                clipSamples--;
        }
        short[] clip = [.. pcm.GetRange(0, (int)(clipSamples * 2))];
        string hash = Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(clip.AsSpan())));
        return new SoundEffectAnalysis(
            context, library, command,
            observer.EndSample is long end ? end - startSample : null,
            observer.Arrivals, observer.KeyOns, clipSamples, seam, observer.ChannelLoops, hash);
    }
}
