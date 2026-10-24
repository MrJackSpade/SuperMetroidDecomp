using SuperMetroid.Core.Compatibility;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Follows one sound-effect request through the driver (#1305): the poll that accepts it, its
/// library's logical end, its key-ons and each allocated channel's first repeated state.
/// Positions are native stereo frames from the accepting poll.
/// </summary>
/// <param name="libraryIndex">The library's zero-based queue index.</param>
/// <param name="command">The request byte.</param>
internal sealed class SoundEffectObserver(int libraryIndex, int command) : ISpcDriverObserver
{
    /// <summary>Most channels any library allocates (library 1).</summary>
    private const int MaximumChannels = 4;

    /// <summary>Reusable snapshot buffer.</summary>
    private readonly List<byte> state = [];
    /// <summary>Key-ons of the sound's voices.</summary>
    private readonly List<KeyOnSample> keyOns = [];
    /// <summary>First arrival of each channel's own poll states, by state key.</summary>
    private readonly Dictionary<string, long>[] channelStates = [[], [], [], []];
    /// <summary>Each channel's first repeated state and voice, once found.</summary>
    private readonly ChannelLoop?[] channelLoops = new ChannelLoop?[MaximumChannels];
    /// <summary>The renderer whose frame count positions each tick.</summary>
    private SpcAudioClipRenderer? renderer;
    /// <summary>Native sample position of the tick being serviced.</summary>
    private long tickSample;
    /// <summary>Whether the tick being serviced polled the sound libraries.</summary>
    private bool polled;
    /// <summary>Whether the tick being serviced carries the request as a changed poll.</summary>
    private bool requested;
    /// <summary>Whether a poll has processed the sound since its start, allocating its channels.</summary>
    private bool processed;

    /// <summary>Absolute sample of the tick at which the driver accepted the request.</summary>
    internal long? StartSample { get; private set; }

    /// <summary>Absolute sample of the tick at which the library's logical end came.</summary>
    internal long? EndSample { get; private set; }

    /// <summary>Key-ons of the sound's voices, relative to the start.</summary>
    internal IReadOnlyList<KeyOnSample> KeyOns => keyOns;

    /// <summary>Each allocated channel's first repeated state (E1, E2) and its voice.</summary>
    internal IReadOnlyList<ChannelLoop> ChannelLoops => [.. FoundChannelLoops()];

    /// <summary>The channels' loops found so far, in channel order.</summary>
    private IEnumerable<ChannelLoop> FoundChannelLoops()
    {
        foreach (ChannelLoop? loop in channelLoops)
        {
            if (loop is { } found)
                yield return found;
        }
    }

    /// <summary>Whether every channel the sound allocated has repeated its own state.</summary>
    internal bool AllChannelLoopsFound { get; private set; }

    /// <summary>Binds the renderer once it exists.</summary>
    internal void Attach(SpcAudioClipRenderer value) => renderer = value;

    /// <inheritdoc />
    public void TickStarting(SoundClockStep<SoundEffectId> tick)
    {
        tickSample = MusicTrackAnalysis.NativeSample(MusicTrackAnalysis.Required(renderer), tick);
        polled = tick.ChecksRequests;
        SoundRequestCheck<SoundEffectId> poll = tick.Checks[libraryIndex];
        requested = polled && poll.Changed && poll.Request is SoundEffectId sound && SoundEffectCommands.Command(sound) == command;
    }

    /// <inheritdoc />
    public void PhraseStarting()
    {
    }

    /// <inheritdoc />
    public void KeysPublished(int keyOn, int keyOff)
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
            keyOns.Add(new KeyOnSample(relative, voices));
        if (EndSample is null && library.CurrentSound == 0)
            EndSample = tickSample;
        if (!polled || EndSample is not null)
            return;
        processed = true;
        RecordChannelStates(player, relative);
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
                channelLoops[index] = new ChannelLoop(first, relative, channels[index].VoiceBitset);
            else
                channelStates[index][key] = relative;
        }
        AllChannelLoopsFound = all;
    }

}
