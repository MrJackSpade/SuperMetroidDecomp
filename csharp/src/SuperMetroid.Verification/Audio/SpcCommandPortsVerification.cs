using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Audio;

/// <summary>Confirms the SPC command-port model and the semantic audio events reproduce the emulated driver (#1304).</summary>
internal static class SpcCommandPortsVerification
{
    /// <summary>Updates in the recorded queue scenario.</summary>
    private const int ScenarioFrames = 120;

    /// <summary>
    /// Recorded from the emulated driver before <see cref="SpcCommandPorts"/> was extracted (#1304 stage 1):
    /// SHA-256 of the scenario's four acknowledgement bytes per update.
    /// </summary>
    private const string EmulatedAcknowledgementsSha256 =
        "069F5A367FA36A896F9CEC9A45949DE1C517DA16DEC7B3195083898FFECBFF59";
    /// <summary>
    /// SHA-256 of the same scenario's 48-kHz PCM from the emulated driver, re-recorded when key-on
    /// began restarting the S-DSP interpolation position and envelope counter (#1305).
    /// </summary>
    private const string EmulatedPcmSha256 =
        "1EDAB958070306151583376ED9EE5E30BF1043ADE91AA6FD4011000FB2BD8CFF";

    /// <summary>Audio data bank index of the Green Brinstar music upload.</summary>
    private static byte GreenBrinstarDataIndex => AudioAssetCatalogDefinitions.Get(AudioBank.GreenBrinstar).DataIndex;

    /// <summary>
    /// Confirms the session's port model and its semantic events reproduce the emulated driver
    /// (#1304 stages 1 and 3). One scripted sequence runs through two dispatchers: one fed by the
    /// game session's <see cref="CartridgeAudioSession"/>, whose bound backend renders the
    /// session's events, and one fed by a standalone port model. Every frame must publish the same
    /// commands and acknowledgement bytes; the session's driver ticks and sound-effect starts must
    /// equal the standalone model's schedule and changed nonzero polls, so nothing starts during
    /// the music downtime; and the acknowledgements and PCM must match the recording made before
    /// the extraction.
    /// </summary>
    internal static void VerifySpcCommandPortsMatchEmulatedDriver()
    {
        var session = CartridgeAudioSessionAccess.CreateSynthesizing(RepositoryInstallation.Installation.LoadAudio());
        var rendered = new CartridgeAudioState();
        var ports = new SpcCommandPorts();
        var modeled = new CartridgeAudioState();
        var acknowledgementBytes = new List<byte>();
        using var pcm = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        CartridgeAudioAcknowledgements modeledAcknowledgements = default;
        var starts = new List<(int Frame, AudioEvent Start)>();
        int uploadFrame = -1, trackFrame = -1, downtimeRequestFrame = -1;
        for (int frame = 0; frame < ScenarioFrames; frame++)
        {
            QueueScenarioRequests(rendered, frame);
            QueueScenarioRequests(modeled, frame);
            IReadOnlyList<CartridgeAudioCommand> commands = rendered.AdvanceFrame(session.Acknowledgements);
            AssertSequenceEqual(commands, modeled.AdvanceFrame(modeledAcknowledgements),
                $"frame {frame} commands under modeled acknowledgements");
            foreach (CartridgeAudioCommand command in commands)
            {
                if (command.Kind == CartridgeAudioCommandKind.Upload && frame > 0)
                    uploadFrame = frame;
                else if (command == CartridgeAudioCommand.WritePort(ApuPort.Music, 5))
                    trackFrame = frame;
                else if (command == CartridgeAudioCommand.WritePort(ApuPort.SoundLibrary3,
                    SoundEffectLibrary3Sounds.SpeedBoosterEcho.Value))
                    downtimeRequestFrame = frame;
            }

            IReadOnlyList<AudioEvent> events = session.Advance(commands);
            session.CopyFrame(samples);
            pcm.AppendData(MemoryMarshal.AsBytes(samples.AsSpan()));
            CartridgeAudioAcknowledgements sessionAcknowledgements = session.Acknowledgements;
            // Port zero is the sequencer's music echo, which gameplay never reads; the recording took it from the driver.
            acknowledgementBytes.AddRange([session.PlayerForVerification.musicPortToSnes, sessionAcknowledgements.Port1,
                sessionAcknowledgements.Port2, sessionAcknowledgements.Port3]);

            List<AudioEvent> expected = ExpectedEvents(ports, commands, out modeledAcknowledgements);
            AssertSequenceEqual(expected, events, $"frame {frame} events equal the port model's writes, ticks and polls");
            foreach (ApuPort port in new[] { ApuPort.SoundLibrary1, ApuPort.SoundLibrary2, ApuPort.SoundLibrary3 })
            {
                AssertEqual(sessionAcknowledgements[port], modeledAcknowledgements[port],
                    $"frame {frame} {port} acknowledgement");
            }
            starts.AddRange(events.Where(audioEvent => audioEvent.Kind == AudioEventKind.StartSoundEffect)
                .Select(audioEvent => (frame, audioEvent)));
        }

        // The scripted sequence reaches the cases it names.
        AssertEqual(18, uploadFrame, "Green Brinstar data upload frame");
        AssertEqual(27, trackFrame, "track select frame, which starts a second SFX downtime");
        AssertEqual(26, downtimeRequestFrame, "library-3 request written one call before the downtime zeroes it");
        // Each start is the driver acting on a request latched at the previous poll, so it lands
        // an update after the CPU's write. The Speed Booster echo written one call before the
        // track's downtime is latched by that update's poll, so the driver still starts it after
        // the downtime zeroes the port. The hard landing queued during the downtime starts
        // nothing until the dispatcher sends it once the eight downtime calls end.
        AssertSequenceEqual(new[]
            {
                (3, SoundEffectLibrary1Sounds.MenuCursor),
                (5, SoundEffectLibrary2Sounds.CancelAll),
                (27, SoundEffectLibrary3Sounds.SpeedBoosterEcho),
                (39, SoundEffectLibrary3Sounds.HardLanding),
            }, starts.Select(start => (start.Frame, start.Start.SoundEffect)),
            "sound effects start only at the poll after the latch, and none during the music downtime");
        AssertTrue(!starts.Any(start => start.Frame > trackFrame && start.Frame <= trackFrame + AudioRomData.Queues.MusicAndSfxDowntimeFrames),
            "a request queued during the music downtime starts nothing until the downtime ends");
        AssertEqual(EmulatedAcknowledgementsSha256,
            Convert.ToHexString(SHA256.HashData(acknowledgementBytes.ToArray())),
            "session acknowledgements match the pre-extraction recording");
        AssertEqual(EmulatedPcmSha256, Convert.ToHexString(pcm.GetHashAndReset()),
            "backend PCM from the session's events matches the recorded PCM");
        Console.WriteLine($"  SPC command ports: {ScenarioFrames} frames of commands, acknowledgements, ticks and " +
            $"{starts.Count} sound starts match the port model; acknowledgements match the pre-extraction recording and PCM matches its recording.");
    }

    /// <summary>
    /// Applies one frame's commands to a standalone port model and walks the frame's driver
    /// ticks, returning the events the session must publish: bank loads and music-port writes in
    /// command order, then each tick followed by the sound effects its poll starts (a changed
    /// nonzero latched command). Port zero's music echo belongs to the sequencer, which this path
    /// does not run; the dispatcher reads only the three sound-effect ports.
    /// </summary>
    private static List<AudioEvent> ExpectedEvents(SpcCommandPorts ports,
        IReadOnlyList<CartridgeAudioCommand> commands, out CartridgeAudioAcknowledgements acknowledgements)
    {
        var expected = new List<AudioEvent>();
        foreach (CartridgeAudioCommand command in commands)
        {
            if (command.Kind == CartridgeAudioCommandKind.Upload)
            {
                ports.ResetForUpload();
                expected.Add(AudioEvent.LoadAudioBank(command.Bank));
                continue;
            }
            ports.WritePort(command.InputPort, command.Value);
            if (command.InputPort == ApuPort.Music)
                expected.Add(AudioEvent.WriteMusicPort(MusicCommand.FromCartridge(command.Value)));
        }
        Span<SpcDriverTick> schedule = stackalloc SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];
        int ticks = ports.AdvanceFrame(schedule);
        foreach (SpcDriverTick tick in schedule[..ticks])
        {
            expected.Add(AudioEvent.DriverTick(tick.DspCycleOffset, tick.Ticks, tick.SoundEffectsPolled));
            if (!tick.SoundEffectsPolled)
                continue;
            (SoundEffectLibrary Library, SpcSoundEffectPoll Poll)[] polls =
                [(SoundEffectLibrary.Library1, tick.Library1), (SoundEffectLibrary.Library2, tick.Library2),
                 (SoundEffectLibrary.Library3, tick.Library3)];
            foreach ((SoundEffectLibrary library, SpcSoundEffectPoll poll) in polls)
            {
                if (poll.Changed && poll.Command != 0)
                    expected.Add(AudioEvent.StartSoundEffect(new SoundEffectId(library, poll.Command), tick.DspCycleOffset));
            }
        }
        acknowledgements = new CartridgeAudioAcknowledgements(0,
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary1),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary2),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary3));
        return expected;
    }

    /// <summary>
    /// The scripted sequence: a library-1 request, library 2's <c>$71</c> cancel, a Green
    /// Brinstar data upload followed by a track select, and library-3 requests timed around
    /// the eight-call music downtime that zeroes every SFX port.
    /// </summary>
    private static void QueueScenarioRequests(CartridgeAudioState audio, int frame)
    {
        switch (frame)
        {
            case 2:
                audio.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                break;
            case 4:
                audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 6);
                break;
            case 10:
                audio.QueueMusicDelayed8(MusicCommand.LoadData(GreenBrinstarDataIndex));
                audio.QueueMusicDelayed8(MusicCommand.SelectTrack(5));
                break;
            case 24:
                // Sent on the first call after the upload's downtime, one call before the
                // track select starts another downtime that zeroes the port again.
                audio.QueueSound(SoundEffectLibrary3Sounds.SpeedBoosterEcho, maximumQueued: 6);
                break;
            case 30:
                // Queued during the track select's downtime; sent once it ends.
                audio.QueueSound(SoundEffectLibrary3Sounds.HardLanding, maximumQueued: 6);
                break;
        }
    }
}
