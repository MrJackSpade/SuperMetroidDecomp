using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Follows one sound-effect request through the driver (#1305): the poll that accepts it, its
/// library's logical end, its key-ons, the first repeated state of the whole driver (E1, E2, E3)
/// and of each allocated channel. Positions are native stereo frames from the accepting poll.
/// </summary>
/// <param name="libraryIndex">The library's zero-based queue index.</param>
/// <param name="command">The request byte.</param>
internal sealed class SoundEffectObserver(int libraryIndex, byte command) : ISpcDriverObserver
{
    /// <summary>Most channels any library allocates (library 1).</summary>
    private const int MaximumChannels = 4;

    /// <summary>Whole-driver poll-state arrivals by state key.</summary>
    private readonly Dictionary<string, List<long>> states = [];
    /// <summary>Reusable snapshot buffer.</summary>
    private readonly List<byte> state = [];
    /// <summary>Key-ons of the sound's voices.</summary>
    private readonly List<(long Sample, byte Voices)> keyOns = [];
    /// <summary>First arrival of each channel's own poll states, by state key.</summary>
    private readonly Dictionary<string, long>[] channelStates = [[], [], [], []];
    /// <summary>Each channel's first repeated state and voice, once found.</summary>
    private readonly (long First, long Second, byte Voice)?[] channelLoops = new (long, long, byte)?[MaximumChannels];
    /// <summary>The renderer whose frame count positions each tick.</summary>
    private SpcAudioClipRenderer? renderer;
    /// <summary>Native sample position of the tick being serviced.</summary>
    private long tickSample;
    /// <summary>Whether the tick being serviced polled the sound libraries.</summary>
    private bool polled;
    /// <summary>Whether the tick being serviced carries the request as a changed poll.</summary>
    private bool requested;
    /// <summary>The first whole-driver state seen twice, once found.</summary>
    private string? anchor;
    /// <summary>Whether a poll has processed the sound since its start, allocating its channels.</summary>
    private bool processed;

    /// <summary>Absolute sample of the tick at which the driver accepted the request.</summary>
    internal long? StartSample { get; private set; }

    /// <summary>Absolute sample of the tick at which the library's logical end came.</summary>
    internal long? EndSample { get; private set; }

    /// <summary>E1, E2 and E3 of the whole driver's first repeated poll state, once found.</summary>
    internal List<long> Arrivals { get; } = [];

    /// <summary>Key-ons of the sound's voices, relative to the start.</summary>
    internal IReadOnlyList<(long Sample, byte Voices)> KeyOns => keyOns;

    /// <summary>Each allocated channel's first repeated state (E1, E2) and its voice.</summary>
    internal IReadOnlyList<(long First, long Second, byte Voice)> ChannelLoops =>
        [.. channelLoops.OfType<(long First, long Second, byte Voice)>()];

    /// <summary>Whether every channel the sound allocated has repeated its own state.</summary>
    internal bool AllChannelLoopsFound { get; private set; }

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
            2 => tick.Library3,
            _ => throw new InvalidOperationException($"Unknown sound library index {libraryIndex}."),
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
            return;
        }
        long relative = tickSample - StartSample.Value;
        byte voices = unchecked((byte)(keyOn & library.EnabledVoices));
        if (voices != 0)
            keyOns.Add((relative, voices));
        if (EndSample is null && library.CurrentSound == 0)
            EndSample = tickSample;
        if (!polled || EndSample is not null)
            return;
        processed = true;
        RecordChannelStates(player, relative);
        if (Arrivals.Count < 3)
            RecordDriverState(player, relative);
    }

    /// <summary>Records each allocated channel's state and notes its first repeat.</summary>
    private void RecordChannelStates(ManagedSpcPlayer player, long relative)
    {
        ManagedSpcSoundChannel[] channels = player.soundChannels[libraryIndex];
        bool all = processed;
        for (int index = 0; index < channels.Length; index++)
        {
            if (channels[index].Disabled == SpcDriverDefinitions.SoundEffects.Active)
                continue;
            if (channelLoops[index] is not null)
                continue;
            all = false;
            state.Clear();
            channels[index].WriteState(state);
            string key = Convert.ToBase64String([.. state]);
            if (channelStates[index].TryGetValue(key, out long first))
                channelLoops[index] = (first, relative, channels[index].VoiceBitset);
            else
                channelStates[index][key] = relative;
        }
        AllChannelLoopsFound = all;
    }

    /// <summary>Records the whole driver's state and notes E1, E2 and E3 of its first repeat.</summary>
    private void RecordDriverState(ManagedSpcPlayer player, long relative)
    {
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
