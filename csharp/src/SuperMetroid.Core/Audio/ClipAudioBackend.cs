namespace SuperMetroid.Core.Audio;

/// <summary>
/// The clip player (#1305, #1478): plays the pre-rendered songs and sounds from the session's audio events.
/// <list type="bullet">
/// <item>A song change waits for the music's next beat: every sound clock step plays the clip frames it accounts for of
/// the beat, at the playing song's beat length (or, while none plays, the last song's), and the change takes effect
/// when the beat completes.</item>
/// <item>Requesting the song that plays changes nothing. A song that ends leaves none playing, so the same song can then
/// restart. A stop silences the music.</item>
/// <item>Each library accepts a sound by its clip's policy (priority, low-health mode, exempt sounds); the sound holds
/// the library until its logical end.</item>
/// <item>A clip-set switch stops the song; the sounds play on, each the same in every clip set (#2057).</item>
/// <item>A cut clip fades out over <see cref="ReleaseFrames"/> frames.</item>
/// </list>
/// Clips mix at their own rate (<see cref="AudioSession.SampleRate"/>), whose frames are the sound clock's samples, and the
/// host resamples the mix to its device. Audio output is presentation: nothing here reaches a gameplay update.
/// </summary>
public sealed partial class ClipAudioBackend
{
    /// <summary>Frames a cut clip fades out over: eight milliseconds.</summary>
    internal const int ReleaseFrames = 256;

    /// <summary>Clip frames in one update.</summary>
    private const int FramesPerUpdate = AudioSession.StereoFramesPerUpdate;

    /// <summary>Clip frames one sound clock step period accounts for.</summary>
    private const int FramesPerStep = SoundClockTiming.SamplesPerStep;

    /// <summary>The installed clips.</summary>
    private readonly ClipAudioLibrary catalog;

    /// <summary>Clip samples already read, by clip (a replacement may share a stock clip's relative path).</summary>
    private readonly Dictionary<AudioClipFile, short[]> samples = [];

    /// <summary>The update's mix, at the clips' own rate.</summary>
    private readonly short[] mix = new short[FramesPerUpdate * PcmSampleFormat.StereoChannelCount];

    /// <summary>The render's progress through an update's events, kept so an update allocates none (#2553).</summary>
    private readonly RenderPass renderPass = new();

    /// <summary>Creates a backend over <paramref name="catalog"/>, continuing <paramref name="state"/> (a restored session's) or starting from power-on.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is null.</exception>
    public ClipAudioBackend(ClipAudioLibrary catalog, ClipPlaybackState? state = null)
    {
        this.catalog = Ensure.NotNull(catalog);
        State = state ?? new ClipPlaybackState(catalog.InitialBeat);
    }

    /// <summary>The playback state this backend advances.</summary>
    internal ClipPlaybackState State { get; }

    /// <summary>Applies one update's events in order and writes its interleaved stereo PCM at the clips' rate.</summary>
    /// <exception cref="InvalidDataException">
    /// An event is malformed or out of order, a song is requested with no clip set loaded, or a
    /// requested sound has no clip in the clip set.
    /// </exception>
    public void Render(in AudioFrameEvents frame, Span<short> stereo)
    {
        IReadOnlyList<AudioEvent> events = Ensure.NotNull(frame.Events);
        RenderPass pass = renderPass;
        pass.Begin(State.Sample);
        var render = new Rendering(this, pass);
        foreach (AudioEvent audioEvent in events)
            audioEvent.React(RenderReactions, render);
        Mix(pass.FrameStart, pass.Cursor, FramesPerUpdate);
        State.RenderedTo(pass.FrameStart + FramesPerUpdate);
        mix.CopyTo(stereo[..AudioSession.PcmSamplesPerFrame]);
    }

    /// <summary>What <c>Render</c> does on each <see cref="AudioEvent"/> of an update, built once (#2553).</summary>
    private static AudioEventReactions<Rendering> RenderReactions { get; } = new(
        ChangeMusicDataSet: static (change, render) =>
        {
            RequireBeforeSteps(render.Pass.Stepped, change);
            render.Backend.SwitchClipSet(change.DataSet, render.Pass.FrameStart);
        },
        SelectMusicTrack: static (selection, render) =>
        {
            RequireBeforeSteps(render.Pass.Stepped, selection);
            render.Backend.State.SongChange.Hold(new SongChange.Play(selection.Track));
        },
        StopMusic: static (stop, render) =>
        {
            RequireBeforeSteps(render.Pass.Stepped, stop);
            render.Backend.State.SongChange.Hold(new SongChange.Stop());
        },
        ClockStep: static (step, render) =>
        {
            RenderPass pass = render.Pass;
            if (step.SampleOffset < pass.Cursor || step.SampleOffset >= FramesPerUpdate)
                throw new InvalidDataException($"Clock step at sample {step.SampleOffset} is out of order or outside the update.");
            render.Backend.Mix(pass.FrameStart, pass.Cursor, step.SampleOffset);
            pass.StepAt(step.SampleOffset, step.ChecksRequests);
            render.Backend.Step(pass.FrameStart + pass.Cursor, step.Steps, step.ChecksRequests);
        },
        StartSoundEffect: static (startSoundEffect, render) =>
        {
            RenderPass pass = render.Pass;
            if (!pass.LastChecked || pass.LastStep != startSoundEffect.SampleOffset)
                throw new InvalidDataException($"{startSoundEffect} does not follow its request-checking clock step.");
            render.Backend.StartSoundEffect(startSoundEffect.SoundEffect, pass.FrameStart + startSoundEffect.SampleOffset);
        });

    /// <summary>One update's render as its events' reactions read it: the backend rendering and its pass through the update.</summary>
    /// <param name="Backend">The backend rendering.</param>
    /// <param name="Pass">The backend's progress through the update's events.</param>
    private readonly partial record struct Rendering(ClipAudioBackend Backend, RenderPass Pass);

    /// <summary>
    /// How far one update's render has come through its events (#2553): where the update starts, how far it has mixed, and
    /// the clock step it met last. One pass is kept per backend and begun afresh each update.
    /// </summary>
    private sealed partial class RenderPass
    {
        /// <summary>The sample the update starts at.</summary>
        internal long FrameStart { get; private set; }

        /// <summary>The sample offset within the update mixed up to.</summary>
        internal int Cursor { get; private set; }

        /// <summary>Whether a clock step has run this update.</summary>
        internal bool Stepped { get; private set; }

        /// <summary>The sample offset of the update's clock step run last, none before its first.</summary>
        internal int? LastStep { get; private set; }

        /// <summary>Whether the clock step run last checked requests.</summary>
        internal bool LastChecked { get; private set; }

        /// <summary>Begins an update starting at <paramref name="frameStart"/>, with nothing mixed and no step run.</summary>
        internal void Begin(long frameStart)
        {
            FrameStart = frameStart;
            Cursor = 0;
            Stepped = false;
            LastStep = null;
            LastChecked = false;
        }

        /// <summary>Notes a clock step at <paramref name="sampleOffset"/>, mixed up to, that checks requests or not.</summary>
        internal void StepAt(int sampleOffset, bool checksRequests)
        {
            Cursor = sampleOffset;
            Stepped = true;
            LastStep = sampleOffset;
            LastChecked = checksRequests;
        }
    }

    /// <summary>A clip-set switch stops the song and drops the waiting song change; the sounds play on.</summary>
    private void SwitchClipSet(MusicDataSet? clipSet, long now)
    {
        ReleaseMusic(now);
        State.SwitchClipSet(clipSet);
    }

    /// <summary>Plays one clock step: sounds past their logical end release their library at a request check, then the beat advances and, when it completes, the waiting song change takes effect.</summary>
    private void Step(long now, int steps, bool checksRequests)
    {
        if (checksRequests)
        {
            // A sound ends at the check after its logical end; a request at that check itself still meets it.
            foreach (ClipLibraryState library in State.Libraries.Values)
                library.EndIfFinished(now);
        }

        if (AdvanceMusicBeat(now, steps))
            OnBeat(now);
    }

    /// <summary>
    /// Plays the music's beat over a clock step at <paramref name="now"/> that accounts for <paramref name="steps"/> step
    /// periods: a song past its end first stops, leaving its last beat length held, then the beat plays the step's clip
    /// frames at the beat length in force. Reports whether the beat completed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="steps"/> is negative.</exception>
    internal bool AdvanceMusicBeat(long now, int steps)
    {
        if (State.Song is MusicClipEntry song && song.LoopStart is null && now - State.SongStart >= song.Clip.Frames)
            State.EndSong(BeatLengthAt(song, song.Clip.Frames));
        return State.Beat.Advance((long)Ensure.AtLeastZero(steps) * FramesPerStep, CurrentBeatLength(now));
    }

    /// <summary>Applies the song change waiting for this beat, unless it asks for what already plays.</summary>
    internal void OnBeat(long now)
    {
        if (State.SongChange.Take() is not SongChange change)
            return;
        // A change to what already plays, the same track or the silence, changes nothing.
        if (change is SongChange.Play { Track: var asked } ? asked == State.PlayingTrack : State.Song is null)
            return;
        if (State.Song is MusicClipEntry playing)
            State.Beat.Hold(BeatLengthAt(playing, now - State.SongStart));
        ReleaseMusic(now);
        if (change is not SongChange.Play { Track: var track })
        {
            State.StopSong();
            return;
        }
        MusicDataSet clipSet = State.ClipSet ?? throw new InvalidDataException($"Song {track} was requested with no clip set loaded.");
        State.PlaySong(catalog.GetMusic(clipSet, track), now);
    }

    /// <summary>Applies a sound start at its request check: the library's policy decides acceptance; an accepted sound replaces the library's sound.</summary>
    private void StartSoundEffect(SoundEffectId soundEffect, long now)
    {
        ClipLibraryState library = State.Libraries[soundEffect.Library];
        SoundEffectClipEntry sound = catalog.GetSoundEffect(soundEffect);
        if (library.Rejects(sound.Policy))
            return;

        ReleaseSound(library, now);
        library.Play(sound, now, sound.Policy);
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
        if (library.Playback is not { } playback)
            return;
        switch (playback.Sound.Shape)
        {
            case OneShotSoundEffectClip { Clip: { } clip }:
                Release(clip, null);
                break;
            case LoopingSoundEffectClip looping:
                foreach (LoopingSoundEffectLayer layer in looping.Layers)
                    Release(layer.Clip, layer.LoopStart);
                break;
            case OneShotSoundEffectClip:
                break;
            default:
                throw new InvalidOperationException($"Undefined sound effect shape {playback.Sound.Shape}.");
        }

        void Release(AudioClipFile clip, long? loopStart)
        {
            if (Position(clip.Frames, loopStart, now - playback.Start) is not null)
                State.Releasing.Add(new ReleasingClip(clip, loopStart, playback.Start, ReleaseFrames));
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
            foreach (ClipLibraryState library in State.Libraries.Values)
            {
                if (library.Playback is not { } playback)
                    continue;
                long position = now - playback.Start;
                if (playback.Sound.Shape is OneShotSoundEffectClip { Clip: { } clip })
                    Add(clip, null, position, 1, 1, ref left, ref right);
                else if (playback.Sound.Shape is LoopingSoundEffectClip looping)
                {
                    foreach (LoopingSoundEffectLayer layer in looping.Layers)
                        Add(layer.Clip, layer.LoopStart, position, 1, 1, ref left, ref right);
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

    /// <summary>How long the beat lasts at a clock step now: the playing song's beat length before this step, or the held one.</summary>
    private BeatLength CurrentBeatLength(long now) =>
        State.Song is MusicClipEntry song ? BeatLengthAt(song, now - State.SongStart) : State.Beat.Held;

    /// <summary>
    /// How long the beat lasts at a clock step at song position <paramref name="position"/>: the song's last beat length
    /// change before that frame, wrapped into the loop. The song at its loop start is as it was at its end, so the same
    /// lookup holds in every pass. Before the song's first change the held beat length applies.
    /// </summary>
    private BeatLength BeatLengthAt(MusicClipEntry song, long position)
    {
        long frame = Position(song.Clip.Frames, song.LoopStart, position) ?? song.Clip.Frames;
        BeatLength length = State.Beat.Held;
        foreach (SongTempo segment in song.TempoSegments)
        {
            if (segment.Frame >= frame)
                break;
            length = segment.Beat;
        }
        return length;
    }

    /// <summary>A clip's samples, read and checked against the manifest's frame count on first use.</summary>
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

    /// <summary>Clamps a mixed sample to the sample range.</summary>
    private static short Clamp16(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue); // cartridge-encoding: allow(HostPcmSample) - the host's signed PCM mix sample, -32768 through 32767
}
