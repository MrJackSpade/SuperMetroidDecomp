using System.Security.Cryptography;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;

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
/// start. The sound's joint period is their least common multiple; channels on separate voices
/// could loop as separate clips.
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
    IReadOnlyList<(long First, long Second)> ChannelLoops,
    string PcmSha256)
{
    /// <summary>
    /// Renders the request on <paramref name="library"/> after the common upload and
    /// <paramref name="context"/>'s upload. The request is written on frame 1; the port model
    /// latches it at a poll and the driver starts it at the next.
    /// </summary>
    internal static SoundEffectAnalysis Analyze(
        ExtractedAudioAssetCatalog catalog,
        AudioBank? context,
        SoundEffectLibrary library,
        byte command,
        bool repeatsForever)
    {
        var observer = new SoundObserver(SoundEffectLibraries.ToQueueIndex(library), command);
        var renderer = new SpcAudioClipRenderer(catalog, observer);
        observer.Attach(renderer);
        renderer.LoadBank(AudioAssetCatalogDefinitions.Common.Bank);
        if (context is not null)
            renderer.LoadBank(context.Value);
        short[] frame = new short[SpcAudioClipRenderer.NativeSamplesPerFrame];
        renderer.RenderFrame(frame);
        renderer.WriteSoundEffectPort(library, command);

        var pcm = new List<short>();
        bool silentAfterEnd = false;
        while (true)
        {
            renderer.RenderFrame(frame);
            if (observer.StartSample is long start)
            {
                long frameStart = (renderer.Frame - 1) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame;
                int skip = (int)Math.Max(0, start - frameStart) * 2;
                pcm.AddRange(frame.AsSpan(skip).ToArray());
            }
            if (repeatsForever)
            {
                if (observer.EndSample is not null)
                    throw new InvalidDataException($"Forever-repeating sound {library} ${command:X2} reached its logical end.");
                if (observer.Arrivals.Count == 3 && pcm.Count >= (2 * observer.Arrivals[2] - observer.Arrivals[1]) * 2)
                    break;
            }
            else if (observer.EndSample is not null)
            {
                if (silentAfterEnd)
                    break;
                silentAfterEnd = Array.TrueForAll(frame, sample => sample == 0);
            }
        }

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

    /// <summary>Records the sound's start, logical end, key-ons and repeated poll states.</summary>
    private sealed class SoundObserver(int libraryIndex, byte command) : ISpcDriverObserver
    {
        /// <summary>Poll-state arrivals by state key.</summary>
        private readonly Dictionary<string, List<long>> states = [];
        /// <summary>Reusable snapshot buffer.</summary>
        private readonly List<byte> state = [];
        /// <summary>Key-ons of the sound's voices, relative to its start.</summary>
        private readonly List<(long Sample, byte Voices)> keyOns = [];
        /// <summary>The renderer whose frame count positions each tick.</summary>
        private SpcAudioClipRenderer? renderer;
        /// <summary>Native sample position of the tick being serviced.</summary>
        private long tickSample;
        /// <summary>Whether the tick being serviced polled the sound libraries.</summary>
        private bool polled;
        /// <summary>The first state seen twice, once found.</summary>
        private string? anchor;
        /// <summary>Whether the tick being serviced carries the request as a changed poll.</summary>
        private bool requested;
        /// <summary>Poll-state arrivals of each library channel's own state.</summary>
        private readonly Dictionary<string, long>[] channelStates = [[], [], [], []];
        /// <summary>Each library channel's first repeated state, once found.</summary>
        private readonly (long First, long Second)?[] channelLoops = new (long, long)?[4];

        /// <summary>Sample of the tick at which the driver accepted the request.</summary>
        internal long? StartSample { get; private set; }
        /// <summary>Sample of the tick at which the library's logical end came.</summary>
        internal long? EndSample { get; private set; }
        /// <summary>E1, E2 and E3 relative to the start, once the anchor has arrived three times.</summary>
        internal List<long> Arrivals { get; } = [];
        /// <summary>Key-ons of the sound's voices.</summary>
        internal IReadOnlyList<(long Sample, byte Voices)> KeyOns => keyOns;

        /// <summary>Each allocated channel's first repeated state (E1, E2), once found.</summary>
        internal IReadOnlyList<(long First, long Second)> ChannelLoops =>
            [.. channelLoops.OfType<(long First, long Second)>()];

        /// <summary>Binds the renderer once it exists.</summary>
        internal void Attach(SpcAudioClipRenderer value) => renderer = value;

        /// <inheritdoc />
        public void TickStarting(SpcDriverTick tick)
        {
            tickSample = MusicTrackAnalysis.NativeSample(MusicTrackAnalysis.Required(renderer), tick);
            polled = tick.SoundEffectsPolled;
            SpcSoundEffectPoll poll = libraryIndex switch
            {
                0 => tick.Library1,
                1 => tick.Library2,
                _ => tick.Library3,
            };
            requested = polled && poll.Changed && poll.Command == command;
        }

        /// <inheritdoc />
        public void PhraseStarting()
        {
        }

        /// <inheritdoc />
        public void KeysPublished(byte keyOn, byte keyOff)
        {
            ManagedSpcPlayer player = MusicTrackAnalysis.Required(renderer).Player;
            ManagedSpcSoundLibrary library = player.soundLibraries[libraryIndex];
            if (StartSample is null)
            {
                if (library.CurrentSound != command)
                {
                    if (requested)
                        throw new InvalidDataException($"Sound library {libraryIndex + 1} rejected ${command:X2} from the power-on state.");
                    return;
                }
                StartSample = tickSample;
            }
            long relative = tickSample - StartSample.Value;
            byte voices = unchecked((byte)(keyOn & library.EnabledVoices));
            if (voices != 0)
                keyOns.Add((relative, voices));
            if (EndSample is null && library.CurrentSound == 0)
                EndSample = tickSample;
            if (!polled || Arrivals.Count == 3 || EndSample is not null)
                return;
            ManagedSpcSoundChannel[] channels = player.soundChannels[libraryIndex];
            for (int index = 0; index < channels.Length; index++)
            {
                if (channelLoops[index] is not null || channels[index].Disabled == SpcDriverDefinitions.SoundEffects.Active)
                    continue;
                state.Clear();
                channels[index].WriteState(state);
                string channelKey = Convert.ToBase64String([.. state]);
                if (channelStates[index].TryGetValue(channelKey, out long first))
                    channelLoops[index] = (first, relative);
                else
                    channelStates[index][channelKey] = relative;
            }
            state.Clear();
            player.WriteSequencerState(state, includeTempoPhase: false);
            string key = Convert.ToBase64String([.. state]);
            if (!states.TryGetValue(key, out List<long>? list))
                states[key] = list = [];
            list.Add(relative);
            if (anchor is null && list.Count == 2)
                anchor = key;
            if (anchor == key)
            {
                Arrivals.Clear();
                Arrivals.AddRange(list);
            }
        }
    }
}
