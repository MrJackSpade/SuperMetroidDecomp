namespace SuperMetroid.Core.Audio;

/// <summary>
/// Plays pre-rendered clips (#1305) from the session's audio events, reproducing the driver's
/// command semantics from the session's events.
/// <list type="bullet">
/// <item>A music-port write is latched and consumed at the next music tick: a carry of the tempo
/// accumulator, which the driver advances by the playing clip's tempo on every driver tick.</item>
/// <item>Writing the playing track is ignored. A track that ends returns the port to zero, so the
/// same track can then restart. Zero stops the music.</item>
/// <item>Each library accepts a request by its clip's policy (priority, low-health mode, exempt
/// requests); the sound holds the library until its logical end.</item>
/// <item>An upload stops the music and silences each playing sound until its next key-on, then
/// continues it in the new bank context.</item>
/// <item>A cut clip fades over one key-off release.</item>
/// </list>
/// Clips mix in the native 32.04-kHz domain and resample to 48 kHz exactly as the emulated DSP's
/// output does, so a clip-only update matches it byte for byte.
/// </summary>
public sealed class ClipAudioBackend
{
    /// <summary>Frames of the key-off release a cut clip fades over: the S-DSP's 8-ms release from full envelope.</summary>
    internal const int ReleaseFrames = 256;

    /// <summary>Native stereo frames in one update.</summary>
    private const int FramesPerUpdate = SpcPortDefinitions.NativeStereoFramesPerVideoFrame;

    /// <summary>The installed clips.</summary>
    private readonly ClipAudioCatalog catalog;

    /// <summary>Clip samples already read, by clip (a replacement may share a stock clip's relative path).</summary>
    private readonly Dictionary<AudioClipFile, short[]> samples = [];

    /// <summary>The update's native mix.</summary>
    private readonly short[] native = new short[FramesPerUpdate * PcmSampleFormat.StereoChannelCount];

    /// <summary>Creates a backend over <paramref name="catalog"/>, continuing <paramref name="state"/> (a restored session's) or starting from power-on.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is null.</exception>
    public ClipAudioBackend(ClipAudioCatalog catalog, ClipPlaybackState? state = null)
    {
        this.catalog = Ensure.NotNull(catalog);
        State = state ?? new ClipPlaybackState();
    }

    /// <summary>The playback state this backend advances.</summary>
    public ClipPlaybackState State { get; }

    /// <summary>Applies one update's events in order and writes its interleaved 48-kHz stereo PCM.</summary>
    /// <exception cref="InvalidDataException">
    /// An event is malformed or out of order, a pause or resume command arrives, a track is
    /// requested with no music bank loaded, or a requested sound has no clip in the bank context.
    /// </exception>
    public void Render(in AudioFrameEvents frame, Span<short> stereo48k)
    {
        IReadOnlyList<AudioEvent> events = Ensure.NotNull(frame.Events);
        long frameStart = State.Sample;
        int cursor = 0;
        bool ticked = false;
        int lastTick = -1;
        bool lastPolled = false;
        foreach (AudioEvent audioEvent in events)
        {
            switch (audioEvent.Kind)
            {
                case AudioEventKind.LoadAudioBank:
                    RequireBeforeTicks(ticked, audioEvent);
                    Upload(audioEvent.Bank, frameStart);
                    break;
                case AudioEventKind.WriteMusicPort:
                    RequireBeforeTicks(ticked, audioEvent);
                    State.MusicLatch = LatchedMusicCommand(audioEvent.Music);
                    break;
                case AudioEventKind.DriverTick:
                    if (audioEvent.DspCycleOffset < cursor || audioEvent.DspCycleOffset >= FramesPerUpdate)
                        throw new InvalidDataException($"Driver tick at DSP cycle {audioEvent.DspCycleOffset} is out of order or outside the update.");
                    Mix(frameStart, cursor, audioEvent.DspCycleOffset);
                    cursor = audioEvent.DspCycleOffset;
                    ticked = true;
                    lastTick = audioEvent.DspCycleOffset;
                    lastPolled = audioEvent.SoundEffectsPolled;
                    Tick(frameStart + cursor, audioEvent.Ticks, audioEvent.SoundEffectsPolled);
                    break;
                case AudioEventKind.StartSoundEffect:
                    if (!lastPolled || lastTick != audioEvent.DspCycleOffset)
                        throw new InvalidDataException($"{audioEvent} does not follow its polling driver tick.");
                    StartSoundEffect(audioEvent.SoundEffect, frameStart + audioEvent.DspCycleOffset);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio event {audioEvent.Kind}.");
            }
        }
        Mix(frameStart, cursor, FramesPerUpdate);
        State.Sample = frameStart + FramesPerUpdate;
        PcmFrameResampler.ResampleStereoLinear(native, stereo48k[..CartridgeAudioSession.PcmSamplesPerFrame]);
    }

    /// <summary>An upload stops the music, clears the music latch and silences each playing sound until its next key-on in the new context.</summary>
    private void Upload(AudioBank bank, long now)
    {
        ReleaseMusic(now);
        State.Music = null;
        State.MusicPort = 0;
        State.MusicLatch = null;
        State.Context = bank == AudioAssetCatalogDefinitions.Common.Bank ? null : bank;
        for (int index = 0; index < State.Libraries.Length; index++)
        {
            ClipLibraryState library = State.Libraries[index];
            if (library.Sound is not SoundEffectClipEntry sound)
                continue;
            ReleaseSound(library, now);
            if (!library.Active)
            {
                library.Sound = null;
                continue;
            }
            SoundEffectClipEntry continued = catalog.GetSoundEffect(State.Context, sound.Library, sound.Command);
            library.Sound = continued;
            long position = now - library.Start;
            library.SilencedUntil = [.. continued.Layers.Select(layer => NextKeyOn(layer, position))];
        }
    }

    /// <summary>Services one driver tick: sound logical ends at a poll, then the tempo timer and, on a carry, the music latch.</summary>
    private void Tick(long now, int ticks, bool polled)
    {
        if (polled)
        {
            foreach (ClipLibraryState library in State.Libraries)
            {
                // A sound ends at the poll after its logical end; a request at the end poll itself still meets it.
                if (library.Active && library.Sound?.ActiveFrames is long active && now - library.Start > active)
                {
                    library.Active = false;
                    library.HighPriority = false;
                }
            }
        }

        if (State.Music is MusicClipEntry music && music.LoopStart is null && now - State.MusicStart >= music.Clip.Frames)
        {
            // The track reached its $0000 terminator: the driver returned the port to zero.
            State.Tempo = TempoAt(music, music.Clip.Frames);
            State.Music = null;
            State.MusicPort = 0;
        }

        (State.TempoAccumulator, bool musicTick) = State.TempoAccumulator.Advance(ticks * CurrentTempo(now));
        if (musicTick)
            MusicTick(now);
    }

    /// <summary>Consumes the latched music command at a music tick.</summary>
    private void MusicTick(long now)
    {
        int? latched = State.MusicLatch;
        State.MusicLatch = null;
        if (latched is not int command || command == State.MusicPort)
            return;
        if (State.Music is MusicClipEntry playing)
            State.Tempo = TempoAt(playing, now - State.MusicStart);
        ReleaseMusic(now);
        State.MusicPort = command;
        if (command == 0)
        {
            State.Music = null;
            return;
        }
        AudioBank bank = State.Context ?? throw new InvalidDataException($"Track {command} was requested with no music bank loaded.");
        State.Music = catalog.GetMusic(bank, command);
        State.MusicStart = now;
    }

    /// <summary>Applies a sound start at its poll: the library's policy decides acceptance; an accepted sound replaces the library's sound.</summary>
    private void StartSoundEffect(SoundEffectId soundEffect, long now)
    {
        int index = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        ClipLibraryState library = State.Libraries[index];
        SoundEffectClipPolicy policy = catalog.GetPolicy(soundEffect.Library, soundEffect.Value);
        bool rejected =
            (!policy.IgnoresLowHealthMode && library.InLowHealthMode) ||
            (!policy.IgnoresPriority && library.HighPriority);
        if (rejected)
            return;

        SoundEffectClipEntry sound = catalog.GetSoundEffect(State.Context, soundEffect.Library, soundEffect.Value);
        if (library.Sound is not null)
        {
            ReleaseSound(library, now);
            if (library.Active)
                library.HighPriority = false;
        }
        library.Sound = sound;
        library.Start = now;
        library.Active = true;
        library.SilencedUntil = new long[sound.Layers.Count];
        library.HighPriority = policy.Priority switch
        {
            SoundEffectPriorityEffect.Low => false,
            SoundEffectPriorityEffect.High => true,
            SoundEffectPriorityEffect.Unchanged => library.HighPriority,
            _ => throw new InvalidDataException($"Unknown priority effect {policy.Priority}."),
        };
        library.InLowHealthMode = policy.LowHealthMode switch
        {
            LowHealthModeEffect.Unchanged => library.InLowHealthMode,
            LowHealthModeEffect.Clear => false,
            LowHealthModeEffect.Enter => true,
            _ => throw new InvalidDataException($"Unknown low-health mode effect {policy.LowHealthMode}."),
        };
    }

    /// <summary>Fades the playing music clip over one release.</summary>
    private void ReleaseMusic(long now)
    {
        if (State.Music is MusicClipEntry music && Position(music.Clip.Frames, music.LoopStart, now - State.MusicStart) is not null)
            State.Releasing.Add(new ReleasingClip(music.Clip, music.LoopStart, State.MusicStart, ReleaseFrames));
    }

    /// <summary>Fades each audible layer of a library's sound over one release.</summary>
    private void ReleaseSound(ClipLibraryState library, long now)
    {
        if (library.Sound is not SoundEffectClipEntry sound)
            return;
        for (int layer = 0; layer < sound.Layers.Count; layer++)
        {
            SoundEffectClipLayer clip = sound.Layers[layer];
            if (now - library.Start >= library.SilencedUntil[layer] && Position(clip.Clip.Frames, clip.LoopStart, now - library.Start) is not null)
                State.Releasing.Add(new ReleasingClip(clip.Clip, clip.LoopStart, library.Start, ReleaseFrames));
        }
    }

    /// <summary>Mixes the music, the sounds and the releasing clips over [<paramref name="from"/>, <paramref name="to"/>) of the update.</summary>
    private void Mix(long frameStart, int from, int to)
    {
        for (int index = from; index < to; index++)
        {
            long now = frameStart + index;
            int left = 0;
            int right = 0;
            if (State.Music is MusicClipEntry music)
                Add(music.Clip, music.LoopStart, now - State.MusicStart, 1, 1, ref left, ref right);
            foreach (ClipLibraryState library in State.Libraries)
            {
                if (library.Sound is not SoundEffectClipEntry sound)
                    continue;
                long position = now - library.Start;
                for (int layer = 0; layer < sound.Layers.Count; layer++)
                {
                    if (position >= library.SilencedUntil[layer])
                        Add(sound.Layers[layer].Clip, sound.Layers[layer].LoopStart, position, 1, 1, ref left, ref right);
                }
            }
            for (int release = State.Releasing.Count - 1; release >= 0; release--)
            {
                ReleasingClip clip = State.Releasing[release];
                Add(clip.Clip, clip.LoopStart, now - clip.Start, clip.Remaining, ReleaseFrames, ref left, ref right);
                if (clip.Remaining == 1)
                    State.Releasing.RemoveAt(release);
                else
                    State.Releasing[release] = clip with { Remaining = clip.Remaining - 1 };
            }
            native[index * 2] = Clamp16(left);
            native[index * 2 + 1] = Clamp16(right);
        }
    }

    /// <summary>Adds one clip frame, scaled by <paramref name="numerator"/>/<paramref name="denominator"/>, when the clip covers <paramref name="position"/>.</summary>
    private void Add(AudioClipFile clip, long? loopStart, long position, int numerator, int denominator, ref int left, ref int right)
    {
        if (Position(clip.Frames, loopStart, position) is not long frame)
            return;
        short[] pcm = Samples(clip);
        left += pcm[frame * 2] * numerator / denominator;
        right += pcm[frame * 2 + 1] * numerator / denominator;
    }

    /// <summary>The clip frame at an unwrapped position: looping clips repeat [loop start, end); others end.</summary>
    private static long? Position(long frames, long? loopStart, long position)
    {
        if (position < 0)
            return null;
        if (position < frames)
            return position;
        if (loopStart is not long loop)
            return null;
        return loop + (position - loop) % (frames - loop);
    }

    /// <summary>The unwrapped position of a layer's first key-on at or after <paramref name="position"/>; never for a layer with none ahead.</summary>
    private static long NextKeyOn(SoundEffectClipLayer layer, long position)
    {
        long frames = layer.Clip.Frames;
        if (Position(frames, layer.LoopStart, position) is not long frame)
            return long.MaxValue;
        foreach (long keyOn in layer.KeyOnFrames)
        {
            if (keyOn >= frame)
                return position + (keyOn - frame);
        }
        if (layer.LoopStart is long loopStart)
        {
            foreach (long keyOn in layer.KeyOnFrames)
            {
                if (keyOn >= loopStart)
                    return position + (frames - frame) + (keyOn - loopStart);
            }
        }
        return long.MaxValue;
    }

    /// <summary>The tempo at a driver tick: the playing clip's tempo before this tick, or the held tempo.</summary>
    private int CurrentTempo(long now) =>
        State.Music is MusicClipEntry music ? TempoAt(music, now - State.MusicStart) : State.Tempo;

    /// <summary>
    /// The tempo a driver tick at clip position <paramref name="position"/> adds: the clip's last
    /// tempo change before that frame, wrapped into the loop. The driver state at the loop start
    /// equals the state at the clip's end, so the same lookup holds in every pass. Before the
    /// clip's first change the held tempo applies.
    /// </summary>
    private int TempoAt(MusicClipEntry music, long position)
    {
        long frame = Position(music.Clip.Frames, music.LoopStart, position) ?? music.Clip.Frames;
        int tempo = State.Tempo;
        foreach (MusicTempoSegment segment in music.TempoSegments)
        {
            if (segment.Frame >= frame)
                break;
            tempo = segment.Tempo;
        }
        return tempo;
    }

    /// <summary>A clip's samples, read and hash-checked on first use.</summary>
    private short[] Samples(AudioClipFile clip)
    {
        if (!samples.TryGetValue(clip, out short[]? pcm))
            samples[clip] = pcm = catalog.ReadClip(clip);
        return pcm;
    }

    /// <summary>
    /// The value a music-port write leaves in the driver's latch: the port carries the command's low byte,
    /// and the driver's empty marker in that byte means no command is pending.
    /// </summary>
    private static int? LatchedMusicCommand(MusicCommand command)
    {
        int written = command.DataIndex;
        return written == SpcPortDefinitions.MusicLatchEmpty ? null : written;
    }

    /// <summary>Bank loads and music commands precede the update's ticks; one after a tick is malformed.</summary>
    private static void RequireBeforeTicks(bool ticked, AudioEvent audioEvent)
    {
        if (ticked)
            throw new InvalidDataException($"{audioEvent} follows a driver tick; it must precede the update's ticks.");
    }

    /// <summary>Saturates a mixed sample to signed 16 bits, as the DSP's output stage does.</summary>
    private static short Clamp16(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
}
