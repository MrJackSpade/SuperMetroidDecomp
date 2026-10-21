using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Developer measurements of exported clips (#1305): how the shipped music loop compares with the
/// cartridge's next pass, and how many passes a drifting track's tempo phase needs to recur.
/// </summary>
internal static class AudioClipMeasurements
{
    /// <summary>Most loop passes before an eight-bit tempo phase must recur.</summary>
    private const int MaximumPhasePasses = 256;

    /// <summary>
    /// Compares the shipped loop [E1, E2) with the pass the cartridge plays after it,
    /// [E2, E2 + (E2 - E1)): exact wherever the track's tempo phase recurs every pass.
    /// </summary>
    /// <exception cref="ArgumentException">The track does not loop.</exception>
    internal static SeamProfile ShippedLoopSeam(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track)
    {
        if (track.Shape != MusicTrackShape.Loops)
            throw new ArgumentException($"{track.Bank} track {track.Track} does not loop.", nameof(track));
        long first = track.Arrivals[0];
        long second = track.Arrivals[1];
        long length = second - first;
        short[] pcm = MusicTrackRendering.Render(catalog, track, second + length);
        return SeamProfile.Compare(pcm, first, second, length);
    }

    /// <summary>
    /// Counts the loop passes after which the full driver state at the loop start recurs including
    /// the sub-tick tempo phase: the shortest loop that would repeat sample-exactly.
    /// </summary>
    /// <exception cref="ArgumentException">The track does not loop.</exception>
    /// <exception cref="InvalidDataException">The phase did not recur within 256 passes.</exception>
    internal static int TempoPhasePeriod(ExtractedAudioAssetCatalog catalog, MusicTrackAnalysis track)
    {
        if (track.Shape != MusicTrackShape.Loops)
            throw new ArgumentException($"{track.Bank} track {track.Track} does not loop.", nameof(track));
        var observer = new PhasePeriodObserver();
        var renderer = new SpcAudioClipRenderer(catalog, observer);
        observer.Attach(renderer);
        MusicTrackAnalysis.QueueBanks(renderer, track.Bank);
        renderer.AdvanceSequencerFrame();
        renderer.WriteMusicPort(unchecked((byte)track.Track));
        while (observer.Period is null)
            renderer.AdvanceSequencerFrame();
        return observer.Period.Value;
    }

    /// <summary>Finds the first repeated phrase state, then counts its arrivals until the full state including tempo phase recurs.</summary>
    private sealed class PhasePeriodObserver : ISpcDriverObserver
    {
        /// <summary>Phrase-state keys seen, without tempo phase.</summary>
        private readonly HashSet<string> seen = [];
        /// <summary>Reusable snapshot buffer.</summary>
        private readonly List<byte> state = [];
        /// <summary>The renderer whose driver is observed.</summary>
        private SpcAudioClipRenderer? renderer;
        /// <summary>The first repeated phrase state.</summary>
        private string? anchor;
        /// <summary>The full state at that state's first repeat.</summary>
        private string? anchorFull;
        /// <summary>Anchor arrivals since that repeat.</summary>
        private int passes;

        /// <summary>The pass count once the full state recurs.</summary>
        internal int? Period { get; private set; }

        /// <summary>Binds the renderer once it exists.</summary>
        internal void Attach(SpcAudioClipRenderer value) => renderer = value;

        /// <inheritdoc />
        public void TickStarting(SoundClockStep tick)
        {
        }

        /// <inheritdoc />
        public void PhraseStarting()
        {
            if (Period is not null)
                return;
            ManagedSpcPlayer player = MusicTrackAnalysis.Required(renderer).Player;
            string key = Snapshot(player, includeTempoPhase: false);
            if (anchor is null)
            {
                if (!seen.Add(key))
                {
                    anchor = key;
                    anchorFull = Snapshot(player, includeTempoPhase: true);
                }
                return;
            }
            if (key != anchor)
                return;
            passes++;
            if (Snapshot(player, includeTempoPhase: true) == anchorFull)
                Period = passes;
            else if (passes > MaximumPhasePasses)
                throw new InvalidDataException($"The tempo phase did not recur within {MaximumPhasePasses} passes.");
        }

        /// <inheritdoc />
        public void KeysPublished(int keyOn, int keyOff)
        {
        }

        /// <summary>The player's sequencer state as a comparable key.</summary>
        private string Snapshot(ManagedSpcPlayer player, bool includeTempoPhase)
        {
            state.Clear();
            player.WriteSequencerState(state, includeTempoPhase);
            return Convert.ToBase64String([.. state]);
        }
    }
}
