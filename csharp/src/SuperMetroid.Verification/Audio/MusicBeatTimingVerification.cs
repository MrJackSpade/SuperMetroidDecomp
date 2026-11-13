using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #1725: the clip player, which times the music's beat in clip frames from the installed beat lengths, starts
/// every beat and every note on the frame the sound driver's tempo arithmetic gave before. The old arithmetic lives here
/// alone: a beat is 256 parts, and each clock step adds the tempo's parts, the tempo a beat length stands for being the
/// parts of 256 that one 64-frame step plays. Every installed song is played in turn from power-on, each song change
/// waiting for the beat, through its clip and one more pass of its loop (or past its end); at every clock step the
/// player's beat and the old arithmetic's must agree. Every sound-effect layer's resumed note after a clip-set switch is
/// compared with the old lookup over its note starts at every position around each note.
/// </summary>
internal static class MusicBeatTimingVerification
{
    /// <summary>The parts of a beat the old arithmetic counts.</summary>
    private const int BeatParts = 256;

    /// <summary>The tempo the old arithmetic held before any song had played.</summary>
    private const int InitialTempo = 16;

    /// <summary>A clock step that accounts for two step periods comes once in this many steps, so both counts are exercised.</summary>
    private const int DoubleStepInterval = 997;

    /// <summary>Clip frames an ending song is played past its end, so the beat it leaves held is compared too.</summary>
    private const long PastEndFrames = 8192;

    /// <summary>Plays every installed song and resumes every sound-effect layer through both timings.</summary>
    internal static void VerifyMusicBeatAndNoteTiming()
    {
        InstalledClipAudioLibrary clips = RepositoryInstallation.Installation.LoadAudioClips();
        AssertEqual(InitialTempo, TempoOf(clips.Manifest.InitialBeat), "the power-on beat lasts as long as the old initial tempo's");
        SongTiming songs = VerifySongs(clips);
        NoteTiming notes = VerifyNotes(clips.Manifest);
        Console.WriteLine(
            $"  Music beat timing: {clips.Manifest.Music.Count} songs over {songs.Steps} clock steps start their {songs.Beats} beats " +
            $"on the old tempo arithmetic's frames; {notes.Layers} sound-effect layers resume on the old note at {notes.Positions} positions.");
    }

    /// <summary>The old tempo a beat length stands for: the parts of a beat one clock step plays, exactly.</summary>
    private static int TempoOf(BeatLength beat)
    {
        long parts = BeatParts * SoundClockTiming.SamplesPerStep * beat.Beats;
        AssertEqual(0L, parts % beat.Frames, $"{beat} stands for a whole tempo");
        AssertTrue(parts / beat.Frames < BeatParts, $"{beat} stands for a tempo below one beat per step");
        return (int)(parts / beat.Frames);
    }

    /// <summary>Plays every installed song in turn through the player and the old arithmetic, comparing every clock step.</summary>
    /// <returns>The clock steps and beats compared.</returns>
    private static SongTiming VerifySongs(InstalledClipAudioLibrary clips)
    {
        var backend = new ClipAudioBackend(clips.Clips);
        ClipPlaybackState state = backend.State;
        var old = new TempoArithmetic();
        long now = 0;
        long steps = 0;
        long beats = 0;
        foreach (MusicClipEntry song in clips.Manifest.Music)
        {
            int[] tempos = [.. song.TempoSegments.Select(segment => TempoOf(segment.Beat))];
            // A beat that never completes would hold the next song change forever; no installed song has one.
            AssertTrue(tempos.All(tempo => tempo > 0), $"{song.AudioSet} {song.Track}'s every beat completes");
            if (state.ClipSet != song.AudioSet)
            {
                state.SwitchClipSet(song.AudioSet);
                old.Stop();
            }
            state.RequestSongChange(new SongChange(song.Track));
            old.Request(song, tempos);
            long until = long.MaxValue;
            while (now < until)
            {
                int periods = ++steps % DoubleStepInterval == 0 ? 2 : 1;
                now += periods * SoundClockTiming.SamplesPerStep;
                bool beat = backend.AdvanceMusicBeat(now, periods);
                bool oldBeat = old.Step(now, periods);
                if (beat != oldBeat)
                    AssertEqual(oldBeat, beat, $"{song.AudioSet} {song.Track}: the beat at frame {now} completes as the old arithmetic's");
                if (!beat)
                    continue;
                beats++;
                backend.OnBeat(now);
                old.OnBeat(now);
                if (until == long.MaxValue)
                {
                    AssertTrue(ReferenceEquals(state.Song, song), $"{song.AudioSet} {song.Track} starts on the first beat after its request");
                    AssertEqual(old.SongStart, state.SongStart, $"{song.AudioSet} {song.Track} starts on the old arithmetic's beat");
                    until = now + song.Clip.Frames + (song.LoopStart is long loop ? song.Clip.Frames - loop : PastEndFrames);
                }
            }
        }
        return new SongTiming(steps, beats);
    }

    /// <summary>Resumes every sound-effect layer at every position around each of its notes, by the player and the old lookup.</summary>
    /// <returns>The layers and positions compared.</returns>
    private static NoteTiming VerifyNotes(AudioClipManifest manifest)
    {
        int layers = 0;
        long positions = 0;
        foreach (InstalledSoundEffectClip entry in manifest.SoundEffects)
        {
            foreach (InstalledSoundEffectLayer installed in entry.Layers)
            {
                SoundEffectClipLayer layer = installed.PlayedLayer();
                long frames = layer.Clip.Frames;
                var around = new List<long> { 0, frames - 1, frames, frames + 1 };
                foreach (long note in layer.NoteStarts)
                {
                    around.AddRange([note - 1, note, note + 1]);
                    if (layer.LoopStart is long loop && note >= loop)
                        around.AddRange([frames + (note - loop) - 1, frames + (note - loop), frames + (note - loop) + 1]);
                }
                foreach (long position in around.Where(position => position >= 0))
                {
                    long expected = OldNextNote(layer.NoteStarts, frames, layer.LoopStart, position);
                    long resumed = ClipAudioBackend.NextNote(layer, position);
                    if (resumed != expected)
                        AssertEqual(expected, resumed, $"{entry.Sound} in {entry.Context}: the note resumed at position {position} is the old lookup's");
                    positions++;
                }
                layers++;
            }
        }
        return new NoteTiming(layers, positions);
    }

    /// <summary>The old lookup: the unwrapped position of a layer's first note start at or after <paramref name="position"/>.</summary>
    private static long OldNextNote(IReadOnlyList<long> keyOnFrames, long frames, long? loopStart, long position)
    {
        if (Position(frames, loopStart, position) is not long frame)
            return long.MaxValue;
        foreach (long keyOn in keyOnFrames)
        {
            if (keyOn >= frame)
                return position + (keyOn - frame);
        }
        if (loopStart is long loop)
        {
            foreach (long keyOn in keyOnFrames)
            {
                if (keyOn >= loop)
                    return position + (frames - frame) + (keyOn - loop);
            }
        }
        return long.MaxValue;
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

    /// <summary>What the song comparison covered.</summary>
    /// <param name="Steps">Clock steps compared.</param>
    /// <param name="Beats">Beats compared.</param>
    private sealed record SongTiming(long Steps, long Beats);

    /// <summary>What the note comparison covered.</summary>
    /// <param name="Layers">Sound-effect layers compared.</param>
    /// <param name="Positions">Positions compared.</param>
    private sealed record NoteTiming(int Layers, long Positions);

    /// <summary>The player's beat as the old arithmetic counted it: parts of a beat, a held tempo, and the song playing.</summary>
    private sealed class TempoArithmetic
    {
        /// <summary>Parts of the current beat already played.</summary>
        private int beat;

        /// <summary>The tempo while no song plays.</summary>
        private int held = InitialTempo;

        /// <summary>The song playing, or null.</summary>
        private MusicClipEntry? playing;

        /// <summary>The playing song's tempo per segment.</summary>
        private int[] playingTempos = [];

        /// <summary>The song waiting for the next beat, or null.</summary>
        private MusicClipEntry? pending;

        /// <summary>The waiting song's tempo per segment.</summary>
        private int[] pendingTempos = [];

        /// <summary>The frame the playing song started at.</summary>
        internal long SongStart { get; private set; }

        /// <summary>Loading a clip set stops the song without holding its tempo.</summary>
        internal void Stop()
        {
            playing = null;
            pending = null;
        }

        /// <summary>Holds <paramref name="song"/> until the next beat.</summary>
        internal void Request(MusicClipEntry song, int[] tempos)
        {
            pending = song;
            pendingTempos = tempos;
        }

        /// <summary>One clock step: an ending song past its end stops, holding its last tempo, then the beat advances.</summary>
        internal bool Step(long now, int periods)
        {
            if (playing is MusicClipEntry song && song.LoopStart is null && now - SongStart >= song.Clip.Frames)
            {
                held = TempoAt(song, playingTempos, song.Clip.Frames);
                playing = null;
            }
            int parts = periods * (playing is MusicClipEntry current ? TempoAt(current, playingTempos, now - SongStart) : held);
            int reached = beat + parts;
            beat = reached % BeatParts;
            return reached >= BeatParts;
        }

        /// <summary>The waiting song starts on this beat, the playing one leaving its tempo held.</summary>
        internal void OnBeat(long now)
        {
            if (pending is not MusicClipEntry song)
                return;
            if (playing is MusicClipEntry current)
                held = TempoAt(current, playingTempos, now - SongStart);
            playing = song;
            playingTempos = pendingTempos;
            SongStart = now;
            pending = null;
        }

        /// <summary>The tempo at song position <paramref name="position"/>: the last change before that frame, wrapped into the loop.</summary>
        private int TempoAt(MusicClipEntry song, int[] tempos, long position)
        {
            long frame = Position(song.Clip.Frames, song.LoopStart, position) ?? song.Clip.Frames;
            int tempo = held;
            for (int segment = 0; segment < tempos.Length && song.TempoSegments[segment].Frame < frame; segment++)
                tempo = tempos[segment];
            return tempo;
        }
    }

    /// <summary>Runs the beat and note timing confirmation over the repository installation's clip index.</summary>
    [VerificationCommand("--music-beat-timing")]
    internal static int MusicBeatTimingCommand()
    {
        Suite(nameof(VerifyMusicBeatAndNoteTiming), () => VerifyMusicBeatAndNoteTiming());
        return 0;
    }
}
