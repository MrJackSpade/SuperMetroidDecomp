using System.Runtime.InteropServices;
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
    /// <param name="NoteStarts">Frames at which the effect keyed a note on: the clip's note starts.</param>
    internal sealed record OneShot(short[] Clip, long ActiveFrames, IReadOnlyList<long> NoteStarts);

    /// <summary>One channel of a never-ending effect: its clip through its second state repeat and its loop.</summary>
    /// <param name="Clip">Interleaved stereo PCM of the channel's voice from the start to its E2.</param>
    /// <param name="LoopStart">The channel's E1: the clip repeats [E1, E2).</param>
    /// <param name="NoteStarts">Frames before E2 at which the channel keyed a note on: the clip's note starts.</param>
    internal sealed record LoopingLayer(short[] Clip, long LoopStart, IReadOnlyList<long> NoteStarts);

    /// <summary>
    /// Creates a renderer, attached to <paramref name="observer"/>, that has uploaded the common and
    /// <paramref name="context"/> banks on frame 0 (rendered when <paramref name="synthesize"/>,
    /// otherwise only sequenced) and queued <paramref name="library"/>'s request
    /// <paramref name="command"/> for frame 1, over <paramref name="catalog"/>. From there on,
    /// <paramref name="reads"/>, when given, records what the renderer reads.
    /// </summary>
    internal static SpcAudioClipRenderer Start(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        int command,
        SoundEffectObserver observer,
        bool synthesize,
        SoundEffectRenderReads? reads)
    {
        var renderer = new SpcAudioClipRenderer(catalog, observer);
        observer.Attach(renderer);
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.AudioSet);
        if (context is not null)
            renderer.LoadBank(context.Value);
        if (synthesize)
            renderer.RenderFrame(new short[SpcAudioClipRenderer.NativeSamplesPerFrame]);
        else
            renderer.AdvanceSequencerFrame();
        reads?.Attach(renderer.Player, synthesize);
        renderer.RequestSound(SoundEffectCommands.Decode(library, command));
        return renderer;
    }

    /// <summary>
    /// The starts of the renderers a render of <paramref name="library"/>'s request <paramref name="command"/> in
    /// <paramref name="context"/> uses (#1503): the synthesizing start, and the sequencing start when the effect
    /// <paramref name="loops"/>.
    /// </summary>
    internal static SoundEffectRenderStarts Starts(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        int command,
        bool loops)
    {
        SoundEffectRenderStart Probe(bool synthesize) => SoundEffectRenderStart.Capture(Start(catalog, context, library, command,
            new SoundEffectObserver(library, command), synthesize, reads: null));
        return new SoundEffectRenderStarts(Probe(synthesize: true), loops ? Probe(synthesize: false) : null);
    }

    /// <summary>
    /// Renders frames, appending the samples from the observed start on to <paramref name="pcm"/>,
    /// until <paramref name="done"/> holds for the frame just rendered.
    /// </summary>
    internal static void RenderFromStart(SpcAudioClipRenderer renderer, SoundEffectObserver observer, List<short> pcm, Func<short[], bool> done)
    {
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        do
        {
            renderer.RenderFrame(frame);
            if (observer.StartSample is long start)
            {
                long frameStart = (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
                int skip = (int)Math.Max(0, start - frameStart) * 2;
                pcm.AddRange(frame.AsSpan(skip));
            }
        }
        while (!done(frame));
    }

    /// <summary>
    /// Renders a one-shot effect through its logical end and the silent frame after its release, recording what it
    /// reads in <paramref name="reads"/> when given.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect never ends, or never starts.</exception>
    internal static OneShot RenderOneShot(
        ExtractedAudioAssetCatalog catalog, AudioBank? context, SoundEffectLibrary library, int command, SoundEffectRenderReads? reads)
    {
        var observer = new SoundEffectObserver(library, command);
        SpcAudioClipRenderer renderer = Start(catalog, context, library, command, observer, synthesize: true, reads);
        bool silentAfterEnd = false;
        var pcm = new List<short>();
        RenderFromStart(renderer, observer, pcm, frame =>
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
        return new OneShot(CollectionsMarshal.AsSpan(pcm)[..(frames * 2)].ToArray(), end - start, [.. observer.KeyOns.Select(keyOn => keyOn.Sample)]);
    }

    /// <summary>
    /// Finds each channel's first repeated state with the sequencer alone, then renders each
    /// channel's voice by itself from the start to its E2, recording what every renderer reads in
    /// <paramref name="reads"/> when given.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect ends, or a channel the effect allocated ends instead of looping.</exception>
    internal static IReadOnlyList<LoopingLayer> RenderLoopingLayers(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        int command,
        int allocatedChannels,
        SoundEffectRenderReads? reads)
    {
        var finder = new SoundEffectObserver(library, command);
        SpcAudioClipRenderer sequencer = Start(catalog, context, library, command, finder, synthesize: false, reads);
        while (!finder.AllChannelLoopsFound)
        {
            if (finder.EndSample is not null)
                throw new InvalidDataException($"Never-ending {library} ${command:X2} reached its logical end.");
            sequencer.AdvanceSequencerFrame();
        }
        IReadOnlyList<ChannelLoop> loops = finder.ChannelLoops;
        if (loops.Count != allocatedChannels)
        {
            throw new InvalidDataException(
                $"Never-ending {library} ${command:X2} loops {loops.Count} of its {allocatedChannels} channels; a channel ended.");
        }

        var layers = new List<LoopingLayer>(loops.Count);
        foreach ((long first, long second, int voice) in loops)
        {
            var observer = new SoundEffectObserver(library, command);
            SpcAudioClipRenderer renderer = Start(catalog, context, library, command, observer, synthesize: true, reads);
            renderer.Player.dsp.OutputVoices = voice;
            // The last frame overshoots E2 by less than a frame, so the buffer never regrows.
            int length = checked((int)(second * 2));
            var pcm = new List<short>(length + SpcAudioClipRenderer.NativeSamplesPerFrame);
            RenderFromStart(renderer, observer, pcm, _ => pcm.Count >= length);
            long[] keyOns = [.. observer.KeyOns.Where(keyOn => (keyOn.Voices & voice) != 0 && keyOn.Sample < second).Select(keyOn => keyOn.Sample)];
            layers.Add(new LoopingLayer(CollectionsMarshal.AsSpan(pcm)[..length].ToArray(), first, keyOns));
        }
        return layers;
    }
}
