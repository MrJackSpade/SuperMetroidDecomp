using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One music track's structure, measured by running the translated driver offline (#1305):
/// whether it loops or ends, its phrase-state repeat points and its echo and noise use. Sample
/// positions count native stereo frames from the frame that loaded the banks; the track is
/// selected on the following frame.
/// </summary>
/// <param name="Bank">Music bank the track belongs to.</param>
/// <param name="Track">Track number written to the music port.</param>
/// <param name="Pointer">The track's top-level record address in the bank's track table.</param>
/// <param name="Shape">Whether the track loops, ends, or showed no repeat within the search bound.</param>
/// <param name="EndSample">Where an ending track returned the music port to zero, or null.</param>
/// <param name="Arrivals">
/// For a looping track, the successive arrivals E1, E2 and E3 at the first repeated sequencer
/// state; the clip is intro [0, E2) and loop [E2, E3). Empty otherwise.
/// </param>
/// <param name="TempoPhaseRepeats">Whether the sub-tick tempo accumulator also matched at E2 and E3, making the seam tick-exact.</param>
/// <param name="PhraseStarts">Top-level phrase starts observed before the analysis stopped.</param>
/// <param name="NoiseVoices">Voices the track keyed on with noise enabled.</param>
/// <param name="EchoVoices">Voices the track ever enabled echo on.</param>
/// <param name="MaximumEchoDelay">The largest echo delay the track selected, in 16-ms units.</param>
internal sealed record MusicTrackAnalysis(
    AudioBank Bank,
    int Track,
    ushort Pointer,
    MusicTrackShape Shape,
    long? EndSample,
    IReadOnlyList<long> Arrivals,
    bool TempoPhaseRepeats,
    int PhraseStarts,
    byte NoiseVoices,
    byte EchoVoices,
    byte MaximumEchoDelay)
{
    /// <summary>The intro's length: E2 for a looping track, the end for an ending one.</summary>
    internal long IntroSamples => Shape switch
    {
        MusicTrackShape.Loops => Arrivals[1],
        MusicTrackShape.Ends => EndSample ?? throw new InvalidOperationException("An ending track has an end sample."),
        MusicTrackShape.NoPeriodFound => 0,
        _ => throw new InvalidOperationException($"Unknown track shape {Shape}."),
    };

    /// <summary>The loop body's length, E3 - E2, or zero when the track does not loop.</summary>
    internal long LoopSamples => Shape == MusicTrackShape.Loops ? Arrivals[2] - Arrivals[1] : 0;

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
    /// Runs the track's sequencer alone until its first repeated phrase state has arrived three
    /// times, or it ends, or <see cref="NonRepeatingPassLimit"/> passes over one top-level record
    /// show no repeat. The DSP is not clocked: the driver never reads it back.
    /// </summary>
    internal static MusicTrackAnalysis Analyze(ExtractedAudioAssetCatalog catalog, AudioBank bank, int track, ushort pointer)
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
            bank, track, pointer, observer.Outcome.Value, observer.EndSample, observer.AnchorArrivals,
            observer.TempoPhaseRepeats, observer.PhraseStarts, observer.NoiseVoices, observer.EchoVoices,
            observer.MaximumEchoDelay);
    }

    /// <summary>
    /// Passes over one top-level record without any repeated phrase state after which a track is
    /// reported as having no finite period: one full wrap of any eight-bit field changed once per
    /// pass. It bounds the search, not elapsed time, and its result goes to the owner (#1305 gate).
    /// </summary>
    internal const int NonRepeatingPassLimit = 256;

    /// <summary>Records phrase-start states, the end of the track, and its echo and noise use.</summary>
    private sealed class LoopObserver : ISpcDriverObserver
    {
        /// <summary>Phrase-state arrivals by phrase-state key (tempo phase excluded).</summary>
        private readonly Dictionary<string, List<(long Sample, string FullState)>> arrivals = [];
        /// <summary>Phrase starts per top-level record address, bounding a non-repeating search.</summary>
        private readonly Dictionary<ushort, int> passes = [];
        /// <summary>Reusable snapshot buffer.</summary>
        private readonly List<byte> state = [];
        /// <summary>The renderer whose frame count positions each tick.</summary>
        private SpcAudioClipRenderer? renderer;
        /// <summary>Native sample position of the tick being serviced.</summary>
        private long tickSample;
        /// <summary>The first phrase state seen twice, once found.</summary>
        private string? anchor;
        /// <summary>Whether the music port has held a track yet.</summary>
        private bool started;

        /// <summary>The analysis result once known.</summary>
        internal MusicTrackShape? Outcome { get; private set; }
        /// <summary>Where the track returned the port to zero, if it ended.</summary>
        internal long? EndSample { get; private set; }
        /// <summary>E1, E2 and E3 once the anchor state has arrived three times.</summary>
        internal IReadOnlyList<long> AnchorArrivals { get; private set; } = [];
        /// <summary>Whether the full state including tempo phase matched at E2 and E3.</summary>
        internal bool TempoPhaseRepeats { get; private set; }
        /// <summary>Phrase starts observed.</summary>
        internal int PhraseStarts { get; private set; }
        /// <summary>Voices keyed on with noise enabled.</summary>
        internal byte NoiseVoices { get; private set; }
        /// <summary>Voices with echo enabled at any tick.</summary>
        internal byte EchoVoices { get; private set; }
        /// <summary>Largest echo delay selected.</summary>
        internal byte MaximumEchoDelay { get; private set; }

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
            PhraseStarts++;
            string key = Snapshot(player, includeTempoPhase: false);
            string full = Snapshot(player, includeTempoPhase: true);
            if (!arrivals.TryGetValue(key, out List<(long Sample, string FullState)>? list))
                arrivals[key] = list = [];
            list.Add((tickSample, full));
            if (anchor is null && list.Count == 2)
                anchor = key;
            if (anchor == key && list.Count == 3)
            {
                AnchorArrivals = [list[0].Sample, list[1].Sample, list[2].Sample];
                TempoPhaseRepeats = list[1].FullState == list[2].FullState;
                Outcome = MusicTrackShape.Loops;
                return;
            }
            int count = passes.GetValueOrDefault(player.ReadMusicTopLevelPointer()) + 1;
            passes[player.ReadMusicTopLevelPointer()] = count;
            if (anchor is null && count > NonRepeatingPassLimit)
                Outcome = MusicTrackShape.NoPeriodFound;
        }

        /// <inheritdoc />
        public void KeysPublished(byte keyOn, byte keyOff)
        {
            ManagedSpcPlayer player = Required(renderer).Player;
            NoiseVoices |= unchecked((byte)(player.noiseEnable & keyOn));
            EchoVoices |= player.echoEnable;
            MaximumEchoDelay = Math.Max(MaximumEchoDelay, player.echoDelay);
            if (player.musicPortToSnes != 0)
                started = true;
            else if (started && Outcome is null)
            {
                EndSample = tickSample;
                Outcome = MusicTrackShape.Ends;
            }
        }

        /// <summary>The player's sequencer state as a comparable key.</summary>
        private string Snapshot(ManagedSpcPlayer player, bool includeTempoPhase)
        {
            state.Clear();
            player.WriteSequencerState(state, includeTempoPhase);
            return Convert.ToBase64String([.. state]);
        }
    }

    /// <summary>The native sample position of <paramref name="tick"/> in the renderer's current frame.</summary>
    internal static long NativeSample(SpcAudioClipRenderer renderer, SpcDriverTick tick) =>
        (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame + tick.DspCycleOffset;

    /// <summary>The attached renderer; observers are attached before the first frame.</summary>
    internal static SpcAudioClipRenderer Required(SpcAudioClipRenderer? renderer) =>
        renderer ?? throw new InvalidOperationException("The observer was not attached to its renderer.");
}
