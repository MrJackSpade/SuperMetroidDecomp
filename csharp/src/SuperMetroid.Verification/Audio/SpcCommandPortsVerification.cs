using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>Confirms the extracted SPC command-port model reproduces the emulated driver (#1304).</summary>
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
    /// <summary>SHA-256 of the same scenario's 48-kHz PCM from the emulated driver.</summary>
    private const string EmulatedPcmSha256 =
        "5D07590F2219F6AC6C69D61BF3B9BFDB69E988A2C0E34AD8C4037FD35C6B8633";

    /// <summary>Audio data bank index of the Green Brinstar music upload.</summary>
    private static byte GreenBrinstarDataIndex => AudioAssetCatalogData.Music
        .Single(bank => bank.Name == "Music_GreenBrinstar").DataIndex;

    /// <summary>
    /// Confirms the synthesis-free <see cref="SpcCommandPorts"/> reproduces the emulated
    /// driver's sound-effect acknowledgements (#1304 stage 1). One scripted sequence runs
    /// through two dispatchers: one fed back by the emulated SPC renderer, as hosts do today,
    /// and one fed back only by a standalone port model. Every frame must publish the same
    /// commands and the same acknowledgement bytes, and the renderer's acknowledgements and
    /// PCM must match the pre-extraction recording.
    /// </summary>
    internal static void VerifySpcCommandPortsMatchEmulatedDriver(ISnesAddressSpace bus)
    {
        var renderer = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());
        var rendered = new CartridgeAudioState();
        var ports = new SpcCommandPorts();
        var modeled = new CartridgeAudioState();
        var acknowledgementBytes = new List<byte>();
        using var pcm = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        CartridgeAudioAcknowledgements renderedAcknowledgements = default;
        CartridgeAudioAcknowledgements modeledAcknowledgements = default;
        int uploadFrame = -1, trackFrame = -1, downtimeRequestFrame = -1;
        for (int frame = 0; frame < ScenarioFrames; frame++)
        {
            QueueScenarioRequests(rendered, frame);
            QueueScenarioRequests(modeled, frame);
            IReadOnlyList<CartridgeAudioCommand> commands = rendered.AdvanceFrame(bus, renderedAcknowledgements);
            AssertSequenceEqual(commands, modeled.AdvanceFrame(bus, modeledAcknowledgements),
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

            short[] samples = renderer.RenderFrame(commands);
            pcm.AppendData(MemoryMarshal.AsBytes(samples.AsSpan()));
            renderedAcknowledgements = renderer.ReadAcknowledgements();
            acknowledgementBytes.AddRange([renderedAcknowledgements.Port0, renderedAcknowledgements.Port1,
                renderedAcknowledgements.Port2, renderedAcknowledgements.Port3]);

            modeledAcknowledgements = AdvanceWithoutSynthesis(ports, commands);
            foreach (ApuPort port in new[] { ApuPort.SoundLibrary1, ApuPort.SoundLibrary2, ApuPort.SoundLibrary3 })
            {
                AssertEqual(renderedAcknowledgements[port], modeledAcknowledgements[port],
                    $"frame {frame} {port} acknowledgement");
            }
        }

        // The scripted sequence reaches the cases it names.
        AssertEqual(18, uploadFrame, "Green Brinstar data upload frame");
        AssertEqual(27, trackFrame, "track select frame, which starts a second SFX downtime");
        AssertEqual(26, downtimeRequestFrame, "library-3 request written one call before the downtime zeroes it");
        AssertEqual(EmulatedAcknowledgementsSha256,
            Convert.ToHexString(SHA256.HashData(acknowledgementBytes.ToArray())),
            "emulated driver acknowledgements match the pre-extraction recording");
        AssertEqual(EmulatedPcmSha256, Convert.ToHexString(pcm.GetHashAndReset()),
            "emulated driver PCM matches the pre-extraction recording");
        Console.WriteLine($"  SPC command ports: {ScenarioFrames} frames of commands and SFX acknowledgements match the emulated driver; acknowledgements and PCM match the pre-extraction recording.");
    }

    /// <summary>
    /// Applies one frame's commands to the port model alone, then walks the frame's driver
    /// ticks. Port zero's music echo belongs to the sequencer, which this path does not run;
    /// the dispatcher reads only the three sound-effect ports.
    /// </summary>
    private static CartridgeAudioAcknowledgements AdvanceWithoutSynthesis(
        SpcCommandPorts ports, IReadOnlyList<CartridgeAudioCommand> commands)
    {
        foreach (CartridgeAudioCommand command in commands)
        {
            if (command.Kind == CartridgeAudioCommandKind.Upload)
                ports.ResetForUpload();
            else
                ports.WritePort(command.InputPort, command.Value);
        }
        Span<SpcDriverTick> schedule = stackalloc SpcDriverTick[SpcCommandPorts.MaximumTicksPerFrame];
        ports.AdvanceFrame(schedule);
        return new CartridgeAudioAcknowledgements(0,
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary1),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary2),
            ports.ReadSoundEffectPort(ApuPort.SoundLibrary3));
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
