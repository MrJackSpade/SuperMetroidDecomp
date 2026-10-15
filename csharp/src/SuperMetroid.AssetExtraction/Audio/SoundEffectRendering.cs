using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Renders one sound-effect request alone from the driver's power-on state (#1305): the common
/// upload and the bank context's upload on frame 0, the request written on frame 1, and PCM
/// collected from the poll that accepts it. A one-shot effect renders as one layer; a
/// never-ending effect renders one looping layer per channel, each with only its own voice in the
/// mix, because the channels loop independently and their joint period can run for minutes.
/// </summary>
internal static class SoundEffectRendering
{
    /// <summary>A rendered one-shot effect: its clip through the release, its logical end and its key-ons.</summary>
    /// <param name="Clip">Interleaved stereo PCM from the start, ending at the last nonzero frame; empty for a silent effect.</param>
    /// <param name="ActiveFrames">Frames from the start to the library's logical end.</param>
    /// <param name="Voices">Every voice the effect keyed.</param>
    /// <param name="KeyOnFrames">Frames at which the effect keyed a note on.</param>
    internal sealed record OneShot(short[] Clip, long ActiveFrames, byte Voices, IReadOnlyList<long> KeyOnFrames);

    /// <summary>One channel of a never-ending effect: its clip through its second state repeat and its loop.</summary>
    /// <param name="Clip">Interleaved stereo PCM of the channel's voice from the start to its E2.</param>
    /// <param name="LoopStart">The channel's E1: the clip repeats [E1, E2).</param>
    /// <param name="Voice">The channel's hardware voice.</param>
    /// <param name="KeyOnFrames">Frames before E2 at which the channel keyed a note on.</param>
    internal sealed record LoopingLayer(short[] Clip, long LoopStart, byte Voice, IReadOnlyList<long> KeyOnFrames);

    /// <summary>
    /// Creates a renderer, attached to <paramref name="observer"/>, that has uploaded the common and
    /// <paramref name="context"/> banks on frame 0 (rendered when <paramref name="synthesize"/>,
    /// otherwise only sequenced) and queued <paramref name="library"/>'s request
    /// <paramref name="command"/> for frame 1, over <paramref name="catalog"/>.
    /// </summary>
    internal static SpcAudioClipRenderer Start(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        byte command,
        SoundEffectObserver observer,
        bool synthesize)
    {
        var renderer = new SpcAudioClipRenderer(catalog, observer);
        observer.Attach(renderer);
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.Bank);
        if (context is not null)
            renderer.LoadBank(context.Value);
        if (synthesize)
            renderer.RenderFrame(new short[SpcAudioClipRenderer.NativeSamplesPerFrame]);
        else
            renderer.AdvanceSequencerFrame();
        renderer.WriteSoundEffectPort(library, command);
        return renderer;
    }

    /// <summary>Renders frames, appending the samples from the observed start on, until <paramref name="done"/> holds.</summary>
    internal static List<short> RenderFromStart(SpcAudioClipRenderer renderer, SoundEffectObserver observer, Func<List<short>, short[], bool> done)
    {
        var pcm = new List<short>();
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        do
        {
            renderer.RenderFrame(frame);
            if (observer.StartSample is long start)
            {
                long frameStart = (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
                int skip = (int)Math.Max(0, start - frameStart) * 2;
                pcm.AddRange(frame.AsSpan(skip).ToArray());
            }
        }
        while (!done(pcm, frame));
        return pcm;
    }

    /// <summary>Renders a one-shot effect through its logical end and the silent frame after its release.</summary>
    /// <exception cref="InvalidDataException">The effect never ends, or never starts.</exception>
    internal static OneShot RenderOneShot(ExtractedAudioAssetCatalog catalog, AudioBank? context, SoundEffectLibrary library, byte command)
    {
        var observer = new SoundEffectObserver(SoundEffectLibraries.ToQueueIndex(library), command);
        SpcAudioClipRenderer renderer = Start(catalog, context, library, command, observer, synthesize: true);
        bool silentAfterEnd = false;
        List<short> pcm = RenderFromStart(renderer, observer, (_, frame) =>
        {
            if (observer.EndSample is null)
                return false;
            if (silentAfterEnd)
                return true;
            silentAfterEnd = Array.TrueForAll(frame, sample => sample == 0);
            return false;
        });
        long start = observer.StartSample ?? throw new InvalidDataException($"{library} ${command:X2} never started.");
        long end = observer.EndSample ?? throw new InvalidDataException($"{library} ${command:X2} never ended.");
        int frames = pcm.Count / 2;
        while (frames > 0 && pcm[frames * 2 - 1] == 0 && pcm[frames * 2 - 2] == 0)
            frames--;
        byte voices = 0;
        foreach ((long _, byte keyed) in observer.KeyOns)
            voices |= keyed;
        return new OneShot([.. pcm.GetRange(0, frames * 2)], end - start, voices, [.. observer.KeyOns.Select(keyOn => keyOn.Sample)]);
    }

    /// <summary>
    /// Finds each channel's first repeated state with the sequencer alone, then renders each
    /// channel's voice by itself from the start to its E2.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect ends, or a channel the effect allocated ends instead of looping.</exception>
    internal static IReadOnlyList<LoopingLayer> RenderLoopingLayers(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        byte command,
        int allocatedChannels)
    {
        int libraryIndex = SoundEffectLibraries.ToQueueIndex(library);
        var finder = new SoundEffectObserver(libraryIndex, command);
        SpcAudioClipRenderer sequencer = Start(catalog, context, library, command, finder, synthesize: false);
        while (!finder.AllChannelLoopsFound)
        {
            if (finder.EndSample is not null)
                throw new InvalidDataException($"Never-ending {library} ${command:X2} reached its logical end.");
            sequencer.AdvanceSequencerFrame();
        }
        IReadOnlyList<(long First, long Second, byte Voice)> loops = finder.ChannelLoops;
        if (loops.Count != allocatedChannels)
        {
            throw new InvalidDataException(
                $"Never-ending {library} ${command:X2} loops {loops.Count} of its {allocatedChannels} channels; a channel ended.");
        }

        var layers = new List<LoopingLayer>(loops.Count);
        foreach ((long first, long second, byte voice) in loops)
        {
            var observer = new SoundEffectObserver(libraryIndex, command);
            SpcAudioClipRenderer renderer = Start(catalog, context, library, command, observer, synthesize: true);
            renderer.Player.dsp.OutputVoices = voice;
            List<short> pcm = RenderFromStart(renderer, observer, (collected, _) => collected.Count >= second * 2);
            long[] keyOns = [.. observer.KeyOns.Where(keyOn => (keyOn.Voices & voice) != 0 && keyOn.Sample < second).Select(keyOn => keyOn.Sample)];
            layers.Add(new LoopingLayer([.. pcm.GetRange(0, (int)(second * 2))], first, voice, keyOns));
        }
        return layers;
    }
}
