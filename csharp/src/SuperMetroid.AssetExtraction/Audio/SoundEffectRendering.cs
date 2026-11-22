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
internal static partial class SoundEffectRendering
{
    /// <summary>A rendered one-shot effect: its clip through the release and its logical end.</summary>
    /// <param name="Clip">Interleaved stereo PCM from the start, ending at the last nonzero frame; empty for a silent effect.</param>
    /// <param name="ActiveFrames">Frames from the start to the library's logical end.</param>
    internal sealed partial record OneShot(short[] Clip, long ActiveFrames);

    /// <summary>One channel of a never-ending effect: its clip through its second state repeat and its loop.</summary>
    /// <param name="Clip">Interleaved stereo PCM of the channel's voice from the start to its E2.</param>
    /// <param name="LoopStart">The channel's E1: the clip repeats [E1, E2).</param>
    internal sealed partial record LoopingLayer(short[] Clip, long LoopStart);

    /// <summary>
    /// Prepares <paramref name="renderer"/>, attached to <paramref name="observer"/>, having uploaded the common and
    /// <paramref name="context"/> banks on frame 0 (rendered when <paramref name="synthesize"/>,
    /// otherwise only sequenced) and queued <paramref name="library"/>'s request
    /// <paramref name="command"/> for frame 1, over <paramref name="catalog"/>.
    /// </summary>
    internal static void Start(
        SpcAudioClipRenderer renderer,
        AudioBank? context,
        SoundEffectLibrary library,
        int command,
        SoundEffectObserver observer,
        bool synthesize)
    {
        renderer.Observe(observer);
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.AudioSet);
        if (context is not null)
            renderer.LoadBank(context.Value);
        if (synthesize)
            renderer.RenderFrame(new short[SpcAudioClipRenderer.NativeSamplesPerFrame]);
        else
            renderer.AdvanceSequencerFrame();
        renderer.RequestSound(SoundEffectCommands.Decode(library, command));
    }

    /// <summary>
    /// Renders frames, appending the samples from the observed start on to <paramref name="pcm"/>,
    /// until <paramref name="done"/> holds for the frame just rendered.
    /// </summary>
    internal static void RenderFromStart(SpcAudioClipRenderer renderer, SoundEffectObserver observer, List<short> pcm, Func<short[], bool> done)
    {
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        do
            RenderFrameFromStart(renderer, observer, pcm, frame);
        while (!done(frame));
    }

    /// <summary>
    /// Renders frames as <see cref="RenderFromStart"/> does until <paramref name="finished"/> yields its result for the
    /// frame just rendered, and returns that result.
    /// </summary>
    private static T RenderUntil<T>(SpcAudioClipRenderer renderer, SoundEffectObserver observer, List<short> pcm, Func<short[], T?> finished)
        where T : class
    {
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        T? result;
        do
        {
            RenderFrameFromStart(renderer, observer, pcm, frame);
            result = finished(frame);
        }
        while (result is null);
        return result;
    }

    /// <summary>Renders one frame into <paramref name="frame"/>, appending its samples from the observed start to <paramref name="pcm"/>.</summary>
    private static void RenderFrameFromStart(SpcAudioClipRenderer renderer, SoundEffectObserver observer, List<short> pcm, short[] frame)
    {
        renderer.RenderFrame(frame);
        if (observer.StartSample is long start)
        {
            long frameStart = (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
            int skip = (int)Math.Max(0, start - frameStart) * 2;
            pcm.AddRange(frame.AsSpan(skip));
        }
    }

    /// <summary>
    /// Renders a one-shot effect through its logical end and the silent frame after its release.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect never ends, or never starts.</exception>
    internal static OneShot RenderOneShot(
        ExtractedAudioAssetCatalog catalog, AudioBank? context, SoundEffectLibrary library, int command)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        var observer = new SoundEffectObserver(renderer, library, command);
        Start(renderer, context, library, command, observer, synthesize: true);
        bool silentAfterEnd = false;
        var pcm = new List<short>();
        SoundEffectSpan lifetime = RenderUntil(renderer, observer, pcm, frame =>
        {
            if (observer.Ended is not { } ended)
                return null;
            if (silentAfterEnd)
                return ended;
            silentAfterEnd = Array.TrueForAll(frame, sample => sample == 0);
            return null;
        });
        int frames = pcm.Count / 2;
        while (frames > 0 && pcm[frames * 2 - 1] == 0 && pcm[frames * 2 - 2] == 0)
            frames--;
        return new OneShot(CollectionsMarshal.AsSpan(pcm)[..(frames * 2)].ToArray(), lifetime.EndSample - lifetime.StartSample);
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
        int command,
        int allocatedChannels)
    {
        var sequencer = new SpcAudioClipRenderer(catalog);
        var finder = new SoundEffectObserver(sequencer, library, command);
        Start(sequencer, context, library, command, finder, synthesize: false);
        while (!finder.AllChannelLoopsFound)
        {
            if (finder.Ended is not null)
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
            var renderer = new SpcAudioClipRenderer(catalog);
            var observer = new SoundEffectObserver(renderer, library, command);
            Start(renderer, context, library, command, observer, synthesize: true);
            renderer.Player.dsp.OutputVoices = voice;
            // The last frame overshoots E2 by less than a frame, so the buffer never regrows.
            int length = checked((int)(second * 2));
            var pcm = new List<short>(length + SpcAudioClipRenderer.NativeSamplesPerFrame);
            RenderFromStart(renderer, observer, pcm, _ => pcm.Count >= length);
            layers.Add(new LoopingLayer(CollectionsMarshal.AsSpan(pcm)[..length].ToArray(), first));
        }
        return layers;
    }
}
