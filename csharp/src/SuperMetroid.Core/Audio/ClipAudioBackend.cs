namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip player (#1305, #1478): plays the pre-rendered songs and sounds from the session's audio events.
/// <list type="bullet">
/// <item>A song change waits for the music's next beat: every sound clock step advances the beat by the playing song's
/// tempo (or, while none plays, the last song's), and the change takes effect when the beat completes.</item>
/// <item>Requesting the song that plays changes nothing. A song that ends leaves none playing, so the same song can then
/// restart. A stop silences the music.</item>
/// <item>Each library accepts a sound by its clip's policy (priority, low-health mode, exempt sounds); the sound holds
/// the library until its logical end.</item>
/// <item>A clip-set switch stops the song and interrupts each sound still holding its library until the sound's next
/// note, from which it continues in the new set's clips.</item>
/// <item>A cut clip fades out over <see cref="ReleaseFrames"/> frames.</item>
/// </list>
/// Clips mix at their own rate (<see cref="SoundClockTiming.SamplesPerUpdate"/> frames per update) and are resampled
/// to the host's 48 kHz. Audio output is presentation: nothing here reaches a gameplay update.
/// </summary>
public sealed class ClipAudioBackend
{
    /// <summary>Frames a cut clip fades out over: eight milliseconds.</summary>
    internal const int ReleaseFrames = 256;

    /// <summary>Clip frames in one update.</summary>
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
    /// An event is malformed or out of order, a song is requested with no clip set loaded, or a
    /// requested sound has no clip in the clip set.
    /// </exception>
    public void Render(in AudioFrameEvents frame, Span<short> stereo48k)
    {
        IReadOnlyList<AudioEvent> events = Ensure.NotNull(frame.Events);
        long frameStart = State.Sample;
        int cursor = 0;
        bool stepped = false;
        int lastStep = -1;
        bool lastChecked = false;
        foreach (AudioEvent audioEvent in events)
        {
            switch (audioEvent)
            {
                case AudioEvent.ChangeMusicDataSet change:
                    RequireBeforeSteps(stepped, audioEvent);
                    SwitchClipSet(change.DataSet, frameStart);
                    break;
                case AudioEvent.SelectMusicTrack selection:
                    RequireBeforeSteps(stepped, audioEvent);
                    State.RequestSongChange(new SongChange(selection.Track));
                    break;
                case AudioEvent.ClockStep step:
                    if (step.SampleOffset < cursor || step.SampleOffset >= FramesPerUpdate)
                        throw new InvalidDataException($"Clock step at sample {step.SampleOffset} is out of order or outside the update.");
                    Mix(frameStart, cursor, step.SampleOffset);
                    cursor = step.SampleOffset;
                    stepped = true;
                    lastStep = step.SampleOffset;
                    lastChecked = step.ChecksRequests;
                    Step(frameStart + cursor, step.Steps, step.ChecksRequests);
                    break;
                case AudioEvent.StartSoundEffect startSoundEffect:
                    if (!lastChecked || lastStep != startSoundEffect.SampleOffset)
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

    /// <summary>A clip-set switch stops the song, drops the waiting song change and interrupts each sound still holding its library until its next note in the new set.</summary>
    private void SwitchClipSet(MusicDataSet? clipSet, long now)
    {
        ReleaseMusic(now);
        State.SwitchClipSet(clipSet);
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
            SoundEffectClipEntry continued = catalog.GetSoundEffect(State.ClipSet, sound.Sound);
            long position = now - library.Start;
            library.Continue(continued, [.. continued.Layers.Select(layer => NextNote(layer, position))]);
        }
    }

    /// <summary>Plays one clock step: sounds past their logical end release their library at a request check, then the beat advances and, when it completes, the waiting song change takes effect.</summary>
    private void Step(long now, int steps, bool checksRequests)
    {
        if (checksRequests)
        {
            // A sound ends at the check after its logical end; a request at that check itself still meets it.
            foreach (ClipLibraryState library in State.Libraries)
                library.EndIfFinished(now);
        }

        if (State.Song is MusicClipEntry song && song.LoopStart is null && now - State.SongStart >= song.Clip.Frames)
            State.EndSong(TempoAt(song, song.Clip.Frames));

        if (State.AdvanceBeat(steps * CurrentTempo(now)))
            OnBeat(now);
    }

    /// <summary>Applies the song change waiting for this beat, unless it asks for what already plays.</summary>
    private void OnBeat(long now)
    {
        if (State.TakeSongChange() is not SongChange change || change.Track == State.PlayingTrack)
            return;
        if (State.Song is MusicClipEntry playing)
            State.HoldTempo(TempoAt(playing, now - State.SongStart));
        ReleaseMusic(now);
        if (change.Track is not MusicTrack track)
        {
            State.StopSong();
            return;
        }
        MusicDataSet clipSet = State.ClipSet ?? throw new InvalidDataException($"Song {track} was requested with no clip set loaded.");
        State.PlaySong(track, catalog.GetMusic(clipSet, track), now);
    }

    /// <summary>Applies a sound start at its request check: the library's policy decides acceptance; an accepted sound replaces the library's sound.</summary>
    private void StartSoundEffect(SoundEffectId soundEffect, long now)
    {
        int index = SoundEffectLibraries.ToQueueIndex(soundEffect.Library);
        ClipLibraryState library = State.Libraries[index];
        SoundEffectClipPolicy policy = catalog.GetPolicy(soundEffect);
        if (library.Rejects(policy))
            return;

        SoundEffectClipEntry sound = catalog.GetSoundEffect(State.ClipSet, soundEffect);
        ReleaseSound(library, now);
        library.Play(sound, now, policy);
    }

    /// <summary>Fades the playing song out.</summary>
    private void ReleaseMusic(long now)
    {
        if (State.Song is MusicClipEntry song && Position(song.Clip.Frames, song.LoopStart, now - State.SongStart) is not null)
            State.Releasing.Add(new ReleasingClip(song.Clip, song.LoopStart, State.SongStart, ReleaseFrames));
    }

    /// <summary>Fades each audible layer of a library's sound out.</summary>
    private void ReleaseSound(ClipLibraryState library, long now)
    {
        if (library.Sound is not SoundEffectClipEntry sound)
            return;
        for (int layer = 0; layer < sound.Layers.Count; layer++)
        {
            SoundEffectClipLayer clip = sound.Layers[layer];
            if (now - library.Start >= library.InterruptedUntil[layer] && Position(clip.Clip.Frames, clip.LoopStart, now - library.Start) is not null)
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
            if (State.Song is MusicClipEntry song)
                Add(song.Clip, song.LoopStart, now - State.SongStart, 1, 1, ref left, ref right);
            foreach (ClipLibraryState library in State.Libraries)
            {
                if (library.Sound is not SoundEffectClipEntry sound)
                    continue;
                long position = now - library.Start;
                for (int layer = 0; layer < sound.Layers.Count; layer++)
                {
                    if (position >= library.InterruptedUntil[layer])
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

    /// <summary>The unwrapped position of a layer's first note start at or after <paramref name="position"/>; never for a layer with none ahead.</summary>
    private static long NextNote(SoundEffectClipLayer layer, long position)
    {
        long frames = layer.Clip.Frames;
        if (Position(frames, layer.LoopStart, position) is not long frame)
            return long.MaxValue;
        foreach (long noteStart in layer.KeyOnFrames)
        {
            if (noteStart >= frame)
                return position + (noteStart - frame);
        }
        if (layer.LoopStart is long loopStart)
        {
            foreach (long noteStart in layer.KeyOnFrames)
            {
                if (noteStart >= loopStart)
                    return position + (frames - frame) + (noteStart - loopStart);
            }
        }
        return long.MaxValue;
    }

    /// <summary>The tempo of a clock step now: the playing song's tempo before this step, or the held tempo.</summary>
    private int CurrentTempo(long now) =>
        State.Song is MusicClipEntry song ? TempoAt(song, now - State.SongStart) : State.Tempo;

    /// <summary>
    /// The tempo a clock step at song position <paramref name="position"/> advances the beat by: the song's last
    /// tempo change before that frame, wrapped into the loop. The song at its loop start is as it was at its end, so the
    /// same lookup holds in every pass. Before the song's first change the held tempo applies.
    /// </summary>
    private int TempoAt(MusicClipEntry song, long position)
    {
        long frame = Position(song.Clip.Frames, song.LoopStart, position) ?? song.Clip.Frames;
        int tempo = State.Tempo;
        foreach (SongTempo segment in song.TempoSegments)
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

    /// <summary>Clip-set switches and song changes precede the update's clock steps; one after a step is malformed.</summary>
    private static void RequireBeforeSteps(bool stepped, AudioEvent audioEvent)
    {
        if (stepped)
            throw new InvalidDataException($"{audioEvent} follows a clock step; it must precede the update's steps.");
    }

    /// <summary>Saturates a mixed sample to signed 16 bits.</summary>
    private static short Clamp16(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue); // cartridge-encoding: allow(HostPcmSample) - the host's signed 16-bit PCM mix sample
}
