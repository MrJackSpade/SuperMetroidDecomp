using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #1305's clip backend. Confirmation 2: for one event stream, its PCM equals the
/// emulated driver's byte for byte, both for a Green Brinstar track over 600 updates and for one
/// library-1 sound with no music. Confirmation 3: scripted events select the clips the driver's
/// rules select: the same track is ignored, an ended track restarts, stop silences, priority,
/// low-health mode and exempt requests decide acceptance, and a clip-set switch interrupts a sound until
/// its next note.
/// </summary>
internal static class AudioClipBackendVerification
{
    /// <summary>The Green Brinstar room track.</summary>
    private const byte RoomTrack = 5;

    /// <summary>Updates compared for the music equivalence.</summary>
    private const int MusicUpdates = 600;

    /// <summary>Updates compared for the sound equivalence.</summary>
    private const int SoundUpdates = 120;

    /// <summary>The common item-fanfare track, which ends.</summary>
    private const byte EndingTrack = 2;

    /// <summary>Updates within which the music's beat completes at any nonzero tempo: a beat's parts in clock steps.</summary>
    private const int BeatUpdates = ClipPlaybackState.BeatParts * SoundClockTiming.SamplesPerStep / SoundClockTiming.SamplesPerUpdate + 1;

    /// <summary>Updates within which a request written now starts: the port model latches it at one poll and the driver acts at the next.</summary>
    private const int SoundStartUpdates = 3;

    /// <summary>Runs both confirmations over the installation's clips and extracted catalog.</summary>
    internal static void VerifyAudioClipBackend(GameInstallation installation)
    {
        ClipAudioLibrary clips = installation.LoadAudioClips();
        ExtractedAudioAssetCatalog extracted = installation.LoadAudio();
        VerifyEquivalence(clips, extracted, AudioRequest.ForTrack(new MusicTrack(RoomTrack)), MusicUpdates, "Green Brinstar room track");
        VerifyEquivalence(clips, extracted, new AudioRequest.RequestSound(Library1Sound.UnchargedPowerBeam), SoundUpdates, "Power Beam");
        VerifyMusicCommands(clips);
        VerifySoundCommands(clips);
        Console.WriteLine(
            "  Audio clip backend: PCM equals the emulated driver for a track and a sound; same-track, end restart, " +
            "stop, priority, low-health mode, exempt requests and upload silencing select the driver's clips.");
    }

    /// <summary>Feeds one event stream to both backends: uploads on update 0, <paramref name="request"/> on update 1.</summary>
    private static void VerifyEquivalence(ClipAudioLibrary clips, ExtractedAudioAssetCatalog extracted, AudioRequest request, int updates, string name)
    {
        var source = new AudioEventSource(new SoundAcceptanceClock<SoundEffectId>());
        var clipBackend = new ClipAudioBackend(clips);
        var synthesis = new ManagedSpcAudioBackend(extracted);
        short[] fromClips = new short[AudioSession.PcmSamplesPerFrame];
        short[] fromSynthesis = new short[AudioSession.PcmSamplesPerFrame];
        bool audible = false;
        for (int update = 0; update < updates; update++)
        {
            IReadOnlyList<AudioRequest> commands = update switch
            {
                0 => [new AudioRequest.ChangeMusicDataSet(null), new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)],
                1 => [request],
                _ => [],
            };
            var frame = new AudioFrameEvents(source.Advance(commands));
            clipBackend.Render(frame, fromClips);
            synthesis.Render(frame, fromSynthesis);
            AssertSequenceEqual(fromSynthesis, fromClips, $"{name} update {update} PCM equals the emulated driver");
            audible |= fromClips.Any(sample => sample != 0);
        }
        AssertTrue(audible, $"{name} is audible");
    }

    /// <summary>The playing song requested again, an ended song restarting, and stop.</summary>
    private static void VerifyMusicCommands(ClipAudioLibrary clips)
    {
        var script = new Script(clips);
        script.Advance([new AudioRequest.ChangeMusicDataSet(null), new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        script.Advance([AudioRequest.ForTrack(new MusicTrack(RoomTrack))]);
        script.AdvanceUntil(() => script.State.Song is not null, BeatUpdates);
        long start = script.State.SongStart;
        AssertEqual(clips.GetMusic(MusicDataSet.GreenBrinstar, new MusicTrack(RoomTrack)), script.State.Song, "the room song's clip starts");

        script.Advance([AudioRequest.ForTrack(new MusicTrack(RoomTrack))]);
        script.Advance(60);
        AssertEqual(start, script.State.SongStart, "requesting the playing song does not restart it");

        script.Advance([new AudioRequest.StopMusic()]);
        script.AdvanceUntil(() => script.State.Song is null, BeatUpdates);
        AssertEqual<MusicTrack?>(null, script.State.PlayingTrack, "stop leaves no song playing");

        MusicClipEntry ending = clips.GetMusic(MusicDataSet.GreenBrinstar, new MusicTrack(EndingTrack));
        AssertTrue(ending.LoopStart is null, "track 2 ends");
        script.Advance([AudioRequest.ForTrack(new MusicTrack(EndingTrack))]);
        script.AdvanceUntil(() => script.State.Song is not null, BeatUpdates);
        long firstStart = script.State.SongStart;
        script.AdvanceUntil(() => script.State.Song is null, (int)(ending.Clip.Frames / ManagedSnesDsp.NativeStereoFramesPerVideoFrame) + 2);
        AssertEqual<MusicTrack?>(null, script.State.PlayingTrack, "an ended song leaves no song playing");
        AssertTrue(script.State.Sample - firstStart >= ending.Clip.Frames, "the song ended after its whole clip");
        script.Advance([AudioRequest.ForTrack(new MusicTrack(EndingTrack))]);
        script.AdvanceUntil(() => script.State.Song is not null, BeatUpdates);
        AssertTrue(script.State.SongStart > firstStart, "the same song restarts after it ended");
    }

    /// <summary>Priority, low-health mode, exempt requests and clip-set switch interruption.</summary>
    private static void VerifySoundCommands(ClipAudioLibrary clips)
    {
        SoundEffectClipEntry high = clips.Manifest.SoundEffects.First(entry =>
            entry.Context == MusicDataSet.GreenBrinstar && entry.Sound.Library == SoundEffectLibrary.Library2 &&
            entry.Policy is { Priority: SoundEffectPriorityEffect.High, IgnoresPriority: false } &&
            entry.ActiveFrames > (SoundStartUpdates + 6) * ManagedSnesDsp.NativeStereoFramesPerVideoFrame);
        SoundEffectClipEntry low = clips.Manifest.SoundEffects.First(entry =>
            entry.Context == MusicDataSet.GreenBrinstar && entry.Sound.Library == SoundEffectLibrary.Library2 &&
            entry.Policy is { Priority: SoundEffectPriorityEffect.Low, IgnoresPriority: false } && entry.Sound != high.Sound);
        var script = new Script(clips);
        script.Advance([new AudioRequest.ChangeMusicDataSet(null), new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        ClipLibraryState library2 = script.State.Libraries[1];

        script.Advance([new AudioRequest.RequestSound(high.Sound)]);
        script.AdvanceUntil(() => library2.Sound is not null, SoundStartUpdates);
        AssertEqual(high, library2.Sound, "the high-priority sound starts");
        AssertEqual(script.LastStart(SoundEffectLibrary.Library2), library2.Start, "it starts at its accepting poll");
        script.Advance([new AudioRequest.RequestSound(low.Sound)]);
        script.Advance(4);
        AssertEqual(high, library2.Sound, "a low-priority request is rejected while the high-priority sound holds the library");
        script.Advance([new AudioRequest.RequestSound(Library2Sound.Silence)]);
        AssertTrue(library2.Active && library2.HighPriority, "the high-priority sound still holds library 2");
        script.AdvanceUntil(() => library2.Sound?.Sound == Library2Sound.Silence, SoundStartUpdates);

        ClipLibraryState library3 = script.State.Libraries[2];
        script.Advance([new AudioRequest.RequestSound(Library3Sound.LowHealthBeep)]);
        script.AdvanceUntil(() => library3.Sound?.Sound == Library3Sound.LowHealthBeep, SoundStartUpdates);
        AssertTrue(library3.InLowHealthMode, "the low-health beep enters low-health mode");
        script.Advance([new AudioRequest.RequestSound(Library3Sound.SpeedBooster)]);
        script.Advance(4);
        AssertEqual<SoundEffectId?>(Library3Sound.LowHealthBeep, library3.Sound?.Sound, "low-health mode rejects another library-3 sound");

        long position = script.State.Sample - library3.Start;
        script.Advance([new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        SoundEffectClipLayer beep = library3.Sound?.Layers[0] ?? throw new InvalidOperationException("The beep has a layer.");
        AssertTrue(library3.InterruptedUntil[0] > position && beep.KeyOnFrames.Count != 0, "a clip-set switch interrupts the looping beep until a later note");

        script.Advance([new AudioRequest.RequestSound(Library3Sound.Silence)]);
        script.AdvanceUntil(() => library3.Sound?.Sound == Library3Sound.Silence, SoundStartUpdates);
        AssertTrue(!library3.InLowHealthMode, "$01 is accepted in low-health mode and clears it");
    }

    /// <summary>Drives a clip backend with the session's event source, one update at a time.</summary>
    /// <param name="clips">The installed clips.</param>
    private sealed class Script(ClipAudioLibrary clips)
    {
        /// <summary>The port model producing the events.</summary>
        private readonly AudioEventSource source = new(new SoundAcceptanceClock<SoundEffectId>());
        /// <summary>The backend under test.</summary>
        private readonly ClipAudioBackend backend = new(clips);
        /// <summary>Reusable PCM output.</summary>
        private readonly short[] pcm = new short[AudioSession.PcmSamplesPerFrame];
        /// <summary>Absolute frame of each library's most recent start event.</summary>
        private readonly long[] lastStarts = new long[3];

        /// <summary>The backend's playback state.</summary>
        internal ClipPlaybackState State => backend.State;

        /// <summary>The absolute frame of the library's most recent start event.</summary>
        internal long LastStart(SoundEffectLibrary library) => lastStarts[SoundEffectLibraries.ToQueueIndex(library)];

        /// <summary>Advances one update with <paramref name="commands"/>.</summary>
        internal void Advance(IReadOnlyList<AudioRequest> commands)
        {
            long frameStart = backend.State.Sample;
            AudioEvent[] events = source.Advance(commands);
            foreach (AudioEvent audioEvent in events)
            {
                if (audioEvent is AudioEvent.StartSoundEffect started)
                    lastStarts[SoundEffectLibraries.ToQueueIndex(started.SoundEffect.Library)] = frameStart + started.SampleOffset;
            }
            backend.Render(new AudioFrameEvents(events), pcm);
        }

        /// <summary>Advances <paramref name="updates"/> updates without commands.</summary>
        internal void Advance(int updates)
        {
            for (int update = 0; update < updates; update++)
                Advance([]);
        }

        /// <summary>Advances until <paramref name="condition"/> holds, failing once the driver's own timing (<paramref name="updates"/>) has passed without it.</summary>
        internal void AdvanceUntil(Func<bool> condition, int updates)
        {
            for (int update = 0; !condition(); update++)
            {
                if (update == updates)
                    throw new InvalidOperationException($"The scripted condition did not hold within the {updates} updates the driver needs.");
                Advance([]);
            }
        }
    }

    /// <summary>Runs the clip-backend confirmations over the repository installation.</summary>
    [VerificationCommand("--audio-clip-backend")]
    internal static int AudioClipBackendCommand()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        Suite(nameof(VerifyAudioClipBackend), () => VerifyAudioClipBackend(installation));
        return 0;
    }
}
