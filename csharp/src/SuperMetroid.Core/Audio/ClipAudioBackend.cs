namespace SuperMetroid.Core.Audio;

/// <summary>
/// Plays pre-rendered clips (#1305) from the session's audio events, reproducing the driver's
/// command semantics from the session's events.
/// <list type="bullet">
/// <item>A track selection is latched and consumed at the next music tick: a carry of the tempo
/// accumulator, which advances by the playing clip's tempo on every clock step.</item>
/// <item>Selecting the playing track is ignored. A track that ends leaves no track selected, so
/// the same track can then restart. Selecting no track stops the music.</item>
/// <item>Each library accepts a request by its clip's policy (priority, low-health mode, exempt
/// requests); the sound holds the library until its logical end.</item>
/// <item>A data set change stops the music and silences each playing sound until its next key-on,
/// then continues it in the new data set's clips.</item>
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
    private const int FramesPerUpdate = SoundClockTiming.SamplesPerUpdate;

    /// <summary>The installed clips.</summary>
    private readonly ClipAudioLibrary catalog;

    /// <summary>Clip samples already read, by clip (a replacement may share a stock clip's relative path).</summary>
    private readonly Dictionary<AudioClipFile, short[]> samples = [];

    /// <summary>The update's mix, at the clips' own rate.</summary>
    private readonly short[] mix = new short[FramesPerUpdate * PcmSampleFormat.StereoChannelCount];

    /// <summary>Creates a backend over <paramref name="catalog"/>, continuing <paramref name="state"/> (a restored session's) or starting from power-on.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is null.</exception>
    public ClipAudioBackend(ClipAudioLibrary catalog, ClipPlaybackState? state = null)
    {
        this.catalog = Ensure.NotNull(catalog);
        State = state ?? new ClipPlaybackState();
    }

    /// <summary>The playback state this backend advances.</summary>
    public ClipPlaybackState State { get; }

    /// <summary>Applies one update's events in order and writes its interleaved 48-kHz stereo PCM.</summary>
    /// <exception cref="InvalidDataException">
    /// An event is malformed or out of order, a pause or resume command arrives, a track is
    /// requested with no music bank loaded, or a requested sound has no clip in the data set.
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
            switch (audioEvent)
            {
                case AudioEvent.ChangeMusicDataSet change:
                    RequireBeforeTicks(ticked, audioEvent);
                    ChangeDataSet(change.DataSet, frameStart);
                    break;
                case AudioEvent.SelectMusicTrack selection:
                    RequireBeforeTicks(ticked, audioEvent);
                    State.LatchMusic(selection.Track?.Number ?? 0);
                    break;
                case AudioEvent.ClockStep step:
                    if (step.SampleOffset < cursor || step.SampleOffset >= FramesPerUpdate)
                        throw new InvalidDataException($"Clock step at sample {step.SampleOffset} is out of order or outside the update.");
                    Mix(frameStart, cursor, step.SampleOffset);
                    cursor = step.SampleOffset;
                    ticked = true;
                    lastTick = step.SampleOffset;
                    lastPolled = step.ChecksRequests;
                    Tick(frameStart + cursor, step.Steps, step.ChecksRequests);
                    break;
                case AudioEvent.StartSoundEffect startSoundEffect:
                    if (!lastPolled || lastTick != startSoundEffect.SampleOffset)
                        throw new InvalidDataException($"{audioEvent} does not follow its request-checking clock step.");
                    StartSoundEffect(startSoundEffect.SoundEffect, frameStart + startSoundEffect.SampleOffset);
                    break;
                default:
                    throw new InvalidDataException($"Unknown audio event {audioEvent}.");
            }
        }
        Mix(frameStart, cursor, FramesPerUpdate);
        State.RenderedTo(frameStart + FramesPerUpdate);
        PcmFrameResampler.ResampleStereoLinear(mix, stereo48k[..AudioSession.PcmSamplesPerFrame]);
    }

    /// <summary>A data set change stops the music, clears the music latch and silences each playing sound until its next key-on in the new data set.</summary>
    private void ChangeDataSet(MusicDataSet? dataSet, long now)
    {
        ReleaseMusic(now);
        State.Upload(dataSet);
        for (int index = 0; index < State.Libraries.Length; index++)
        {
            ClipLibraryState library = State.Libraries[index];
            if (library.Sound is not SoundEffectClipEntry sound)
                continue;
            ReleaseSound(library, now);
            if (!library.Active)
            {
                library.Clear();
                continue;
            }
            SoundEffectClipEntry continued = catalog.GetSoundEffect(State.Context, sound.Sound);
            long position = now - library.Start;
            library.Continue(continued, [.. continued.Layers.Select(layer => NextKeyOn(layer, position))]);
        }
    }

    /// <summary>Services one clock step: sound logical ends at a request check, then the tempo timer and, on a carry, the music latch.</summary>
    private void Tick(long now, int ticks, bool polled)
    {
        if (polled)
        {
            // A sound ends at the poll after its logical end; a request at the end poll itself still meets it.
            foreach (ClipLibraryState library in State.Libraries)
                library.EndIfFinished(now);
        }

        if (State.Music is MusicClipEntry music && music.LoopStart is null && now - State.MusicStart >= music.Clip.Frames)
        {
            // The track reached its $0000 terminator: the driver returned the port to zero.
            State.EndTrack(TempoAt(music, music.Clip.Frames));
        }

        if (State.AdvanceTempo(ticks * CurrentTempo(now)))
            MusicTick(now);
    }

    /// <summary>Consumes the latched music command at a music tick.</summary>
    private void MusicTick(long now)
    {
        if (State.TakeMusicLatch() is not int command || command == State.MusicPort)
            return;
        if (State.Music is MusicClipEntry playing)
            State.HoldTempo(TempoAt(playing, now - State.MusicStart));
        ReleaseMusic(now);
        if (command == 0)
        {
            State.SelectTrack(command, null, now);
            return;
        }
        MusicDataSet loadedSet = State.Context ?? throw new InvalidDataException($"Track {command} was requested with no music data set loaded.");
        State.SelectTrack(command, catalog.GetMusic(loadedSet, command), now);
    }

    /// <summary>Applies a sound start at its poll: the library's policy decides acceptance; an accepted sound replaces the library's sound.</summary>
    private void StartSoundEffect(SoundEffectId soundEffect, long now)
    {
        int index = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        ClipLibraryState library = State.Libraries[index];
        SoundEffectClipPolicy policy = catalog.GetPolicy(soundEffect);
        if (library.Rejects(policy))
            return;

        SoundEffectClipEntry sound = catalog.GetSoundEffect(State.Context, soundEffect);
        ReleaseSound(library, now);
        library.Play(sound, now, policy);
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
            mix[index * 2] = Clamp16(left);
            mix[index * 2 + 1] = Clamp16(right);
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

    /// <summary>Data set changes and track selections precede the update's clock steps; one after a step is malformed.</summary>
    private static void RequireBeforeTicks(bool ticked, AudioEvent audioEvent)
    {
        if (ticked)
            throw new InvalidDataException($"{audioEvent} follows a clock step; it must precede the update's steps.");
    }

    /// <summary>Saturates a mixed sample to signed 16 bits, as the DSP's output stage does.</summary>
    private static short Clamp16(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);
}
