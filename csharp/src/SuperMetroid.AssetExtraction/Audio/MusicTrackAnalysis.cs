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
/// <param name="Shape">Whether the track loops, ends, or showed no repeat within the search bound.</param>
/// <param name="EndSample">Where an ending track returned the music port to zero, or null.</param>
/// <param name="Arrivals">
/// For a looping track, the first two arrivals E1 and E2 at the first repeated sequencer state;
/// the exported clip is intro [start, E1) and loop [E1, E2). Empty otherwise.
/// </param>
/// <param name="StartSample">The driver tick that started the track (the music port took the track number).</param>
/// <param name="TempoChanges">The tempo's whole byte after the start tick and after every tick that changed it, by tick sample.</param>
internal sealed record MusicTrackAnalysis(
    AudioBank Bank,
    int Track,
    MusicTrackShape Shape,
    long? EndSample,
    IReadOnlyList<long> Arrivals,
    long StartSample,
    IReadOnlyList<(long Sample, byte Tempo)> TempoChanges)
{
    /// <summary>
    /// Passes over one top-level record without any repeated phrase state after which a track is
    /// reported as having no finite period: one full wrap of any eight-bit field changed once per
    /// pass. It bounds the search, not elapsed time, and its result goes to the owner (#1305 gate).
    /// </summary>
    internal const int NonRepeatingPassLimit = 256;

    /// <summary>The clip's end: E2 for a looping track, the end tick for an ending one.</summary>
    internal long ClipEndSample => Shape switch
    {
        MusicTrackShape.Loops => Arrivals[1],
        MusicTrackShape.Ends => EndSample ?? throw new InvalidOperationException("An ending track has an end sample."),
        MusicTrackShape.NoPeriodFound => throw new InvalidOperationException($"{Bank} track {Track} has no period."),
        _ => throw new InvalidOperationException($"Unknown track shape {Shape}."),
    };

    /// <summary>
    /// Queues the common and <paramref name="bank"/> uploads for a renderer's first frame. Every
    /// analysis advances that frame, then selects the track on the second, so positions agree.
    /// </summary>
    internal static void QueueBanks(SpcAudioClipRenderer renderer, AudioBank bank)
    {
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.Bank);
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
        var observer = new LoopObserver();
        var renderer = new SpcAudioClipRenderer(catalog, observer);
        observer.Attach(renderer);
        QueueBanks(renderer, bank);
        renderer.AdvanceSequencerFrame();
        renderer.WriteMusicPort(unchecked((byte)track));
        while (observer.Outcome is null)
            renderer.AdvanceSequencerFrame();
        return new MusicTrackAnalysis(
            bank, track, observer.Outcome.Value, observer.EndSample, observer.AnchorArrivals,
            observer.StartSample ?? throw new InvalidDataException($"{bank} track {track} never started."),
            observer.TempoChanges);
    }

    /// <summary>The native sample position of <paramref name="tick"/> in the renderer's current frame.</summary>
    internal static long NativeSample(SpcAudioClipRenderer renderer, SpcDriverTick tick) =>
        (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame + tick.DspCycleOffset;

    /// <summary>The attached renderer; observers are attached before the first frame.</summary>
    internal static SpcAudioClipRenderer Required(SpcAudioClipRenderer? renderer) =>
        renderer ?? throw new InvalidOperationException("The observer was not attached to its renderer.");

    /// <summary>Records phrase-start states, the track's start and end, and its tempo timeline.</summary>
    private sealed class LoopObserver : ISpcDriverObserver
    {
        /// <summary>First arrival of each phrase state (tempo phase excluded), by state key.</summary>
        private readonly Dictionary<string, long> arrivals = [];
        /// <summary>Phrase starts per top-level record address, bounding a non-repeating search.</summary>
        private readonly Dictionary<ushort, int> passes = [];
        /// <summary>Reusable snapshot buffer.</summary>
        private readonly List<byte> state = [];
        /// <summary>The renderer whose frame count positions each tick.</summary>
        private SpcAudioClipRenderer? renderer;
        /// <summary>Native sample position of the tick being serviced.</summary>
        private long tickSample;

        /// <summary>The analysis result once known.</summary>
        internal MusicTrackShape? Outcome { get; private set; }
        /// <summary>Where the track returned the port to zero, if it ended.</summary>
        internal long? EndSample { get; private set; }
        /// <summary>E1 and E2 once the first repeated state has arrived twice.</summary>
        internal IReadOnlyList<long> AnchorArrivals { get; private set; } = [];
        /// <summary>The tick that started the track, once it has.</summary>
        internal long? StartSample { get; private set; }
        /// <summary>The tempo after the start tick and after each tick that changed it.</summary>
        internal List<(long Sample, byte Tempo)> TempoChanges { get; } = [];

        /// <summary>Binds the renderer once it exists.</summary>
        internal void Attach(SpcAudioClipRenderer value) => renderer = value;

        /// <inheritdoc />
        public void TickStarting(SpcDriverTick tick) =>
            tickSample = NativeSample(Required(renderer), tick);

        /// <inheritdoc />
        public void PhraseStarting()
        {
            if (Outcome is not null)
                return;
            ManagedSpcPlayer player = Required(renderer).Player;
            state.Clear();
            player.WriteSequencerState(state, includeTempoPhase: false);
            string key = Convert.ToBase64String([.. state]);
            if (arrivals.TryGetValue(key, out long first))
            {
                AnchorArrivals = [first, tickSample];
                Outcome = MusicTrackShape.Loops;
                return;
            }
            arrivals[key] = tickSample;
            ushort pointer = player.ReadMusicTopLevelPointer();
            int count = passes.GetValueOrDefault(pointer) + 1;
            passes[pointer] = count;
            if (count > NonRepeatingPassLimit)
                Outcome = MusicTrackShape.NoPeriodFound;
        }

        /// <inheritdoc />
        public void KeysPublished(byte keyOn, byte keyOff)
        {
            ManagedSpcPlayer player = Required(renderer).Player;
            if (player.musicPortToSnes != 0)
            {
                byte tempo = player.ReadTempo();
                StartSample ??= tickSample;
                if (TempoChanges.Count == 0 || TempoChanges[^1].Tempo != tempo)
                    TempoChanges.Add((tickSample, tempo));
            }
            else if (StartSample is not null && Outcome is null)
            {
                EndSample = tickSample;
                Outcome = MusicTrackShape.Ends;
            }
        }
    }
}
