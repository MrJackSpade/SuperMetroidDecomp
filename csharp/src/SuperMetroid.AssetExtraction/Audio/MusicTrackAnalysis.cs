using System.Runtime.InteropServices;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One music track's structure, measured by running the translated driver offline (#1305):
/// whether it loops or ends, where its loop is, and its tempo timeline. Sample positions count
/// native stereo frames from the frame that loaded the banks; the track is selected on the
/// following frame.
/// </summary>
/// <param name="Bank">Music bank the track belongs to.</param>
/// <param name="Track">Track number written to the music port.</param>
/// <param name="Period">Whether the track loops, with its loop, ends, with its end, or showed no repeat within the search bound.</param>
/// <param name="StartSample">The driver tick that started the track (the music port took the track number).</param>
/// <param name="TempoChanges">The tempo's whole byte after the start tick and after every tick that changed it, by tick sample.</param>
internal sealed record MusicTrackAnalysis(
    AudioBank Bank,
    int Track,
    MusicTrackPeriod Period,
    long StartSample,
    IReadOnlyList<TempoChange> TempoChanges)
{
    /// <summary>
    /// Passes over one top-level record without any repeated phrase state after which a track is
    /// reported as having no finite period: one full wrap of any eight-bit field changed once per
    /// pass. It bounds the search, not elapsed time, and its result goes to the owner (#1305 gate).
    /// </summary>
    internal const int NonRepeatingPassLimit = 256;

    /// <summary>Whether the track loops, ends, or showed no repeat within the search bound.</summary>
    internal MusicTrackShape Shape => Period.Shape;

    /// <summary>The clip's end: E2 for a looping track, the end tick for an ending one.</summary>
    /// <exception cref="InvalidOperationException">The track showed no period, so it has no clip.</exception>
    internal long ClipEndSample => Period switch
    {
        MusicTrackPeriod.Loop loop => loop.SecondArrival,
        MusicTrackPeriod.Ending ending => ending.EndSample,
        _ => throw new InvalidOperationException($"{Bank} track {Track} has no period."),
    };

    /// <summary>
    /// Queues the common and <paramref name="bank"/> uploads for a renderer's first frame. Every
    /// analysis advances that frame, then selects the track on the second, so positions agree.
    /// </summary>
    internal static void QueueBanks(SpcAudioClipRenderer renderer, AudioBank bank)
    {
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.AudioSet);
        renderer.LoadBank(bank);
    }

    /// <summary>
    /// Runs the track's sequencer alone until its first repeated phrase state has arrived twice,
    /// or it ends, or <see cref="NonRepeatingPassLimit"/> passes over one top-level record show no
    /// repeat. The DSP is not clocked: the driver never reads it back.
    /// </summary>
    /// <exception cref="InvalidDataException">The track never starts.</exception>
    internal static MusicTrackAnalysis Analyze(ExtractedAudioAssetCatalog catalog, AudioBank bank, int track)
    {
        var renderer = new SpcAudioClipRenderer(catalog);
        var observer = new LoopObserver(renderer);
        renderer.Observe(observer);
        QueueBanks(renderer, bank);
        renderer.AdvanceSequencerFrame();
        renderer.PlayTrack(MusicTrackNumbers.Decode(bank, track));
        // A period is found only among the started track's phrases, so the track has started by the time one is.
        while (observer.Outcome is not { } period || observer.StartSample is not long start)
            renderer.AdvanceSequencerFrame();
        return new MusicTrackAnalysis(bank, track, period, start, observer.TempoChanges);
    }

    /// <summary>The native sample position of <paramref name="tick"/> in the renderer's current frame.</summary>
    internal static long NativeSample(SpcAudioClipRenderer renderer, SoundClockStep<SoundEffectLibrary, byte> tick) =>
        (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame + tick.SampleOffset;

    /// <summary>Records phrase-start states, the track's start and end, and its tempo timeline.</summary>
    /// <param name="renderer">The renderer whose frame count positions each tick.</param>
    private sealed class LoopObserver(SpcAudioClipRenderer renderer) : ISpcDriverObserver
    {
        /// <summary>First arrival of each phrase state (tempo phase excluded), by state key.</summary>
        private readonly Dictionary<string, long> arrivals = [];
        /// <summary>Phrase starts per top-level record address, bounding a non-repeating search.</summary>
        private readonly Dictionary<int, int> passes = [];
        /// <summary>Reusable snapshot buffer.</summary>
        private readonly List<byte> state = [];
        /// <summary>Native sample position of the tick being serviced.</summary>
        private long tickSample;

        /// <summary>The track's period once known: its loop, its end, or none within the search bound.</summary>
        internal MusicTrackPeriod? Outcome { get; private set; }
        /// <summary>The tick that started the track, once it has.</summary>
        internal long? StartSample { get; private set; }
        /// <summary>The tempo after the start tick and after each tick that changed it.</summary>
        internal List<TempoChange> TempoChanges { get; } = [];

        /// <inheritdoc />
        public void TickStarting(SoundClockStep<SoundEffectLibrary, byte> tick) =>
            tickSample = NativeSample(renderer, tick);

        /// <inheritdoc />
        public void PhraseStarting()
        {
            if (Outcome is not null)
                return;
            ManagedSpcPlayer player = renderer.Player;
            state.Clear();
            player.WriteSequencerState(state, includeTempoPhase: false);
            string key = Convert.ToBase64String(CollectionsMarshal.AsSpan(state));
            if (arrivals.TryGetValue(key, out long first))
            {
                Outcome = new MusicTrackPeriod.Loop(first, tickSample);
                return;
            }
            arrivals[key] = tickSample;
            int pointer = player.ReadMusicTopLevelPointer();
            int count = passes.GetValueOrDefault(pointer) + 1;
            passes[pointer] = count;
            if (count > NonRepeatingPassLimit)
                Outcome = new MusicTrackPeriod.NoneFound();
        }

        /// <inheritdoc />
        public void KeysPublished(int keyOn, int keyOff)
        {
            ManagedSpcPlayer player = renderer.Player;
            if (player.Music.musicPortToSnes != 0)
            {
                int tempo = player.ReadTempo();
                StartSample ??= tickSample;
                if (TempoChanges.Count == 0 || TempoChanges[^1].Tempo != tempo)
                    TempoChanges.Add(new TempoChange(tickSample, tempo));
            }
            else if (StartSample is not null && Outcome is null)
            {
                Outcome = new MusicTrackPeriod.Ending(tickSample);
            }
        }
    }
}
