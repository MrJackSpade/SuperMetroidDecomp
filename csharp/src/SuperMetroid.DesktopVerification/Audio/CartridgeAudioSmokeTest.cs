using SuperMetroid.AssetExtraction.Audio;
namespace SuperMetroid.DesktopVerification.Audio;

/// <summary>Runs the real title music path without opening an audio device or game window.</summary>
public static class CartridgeAudioSmokeTest
{
    /// <summary>Renders title music for the requested update count, then checks the isolated power-beam sound's request/clear handshake and nonzero PCM.</summary>
    /// <param name="frames">Positive number of title-music audio updates, separate from the thirty-update power-beam check.</param>
    public static CartridgeAudioSmokeTestResult Run(int frames = 240)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frames);

        var queue = new AudioQueues();
        queue.QueueMusicDelayed8(MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
        queue.QueueMusicDelayed8(MusicRequest.SelectTrack(5));

        int nonZeroSamples = 0;
        int peakAmplitude = 0;
        SoundAcknowledgements<SoundEffectId> acknowledgements = default;
        var engine = DesktopAccess.CreateAudioSession();
        for (int frame = 0; frame < frames; frame++)
        {
            IReadOnlyList<AudioRequest> commands =
                queue.AdvanceFrame(acknowledgements);
            foreach (short sample in engine.RenderFrame(commands))
            {
                if (sample != 0)
                    nonZeroSamples++;
                int amplitude = sample == short.MinValue ? 32768 : Math.Abs(sample);
                peakAmplitude = Math.Max(peakAmplitude, amplitude);
            }
            acknowledgements = engine.Acknowledgements;
        }

        if (nonZeroSamples == 0 || peakAmplitude == 0)
        {
            throw new InvalidDataException(
                "The translated SPC/DSP engine generated silence for the title music sequence.");
        }

        int powerBeamNonZeroSamples = VerifyPowerBeamSoundAndHandshake();
        return new CartridgeAudioSmokeTestResult(
            frames,
            nonZeroSamples,
            peakAmplitude,
            powerBeamNonZeroSamples);
    }

    /// <summary>
    /// Proves library one's real request/clear protocol using the ROM-selected power-beam ID.
    /// </summary>
    private static int VerifyPowerBeamSoundAndHandshake()
    {
        const byte powerBeamSound = 0x0b;
        const int auditFrames = 30;

        // A fresh player receives only reset's common driver/sample upload.  Keeping title
        // music out of this pass means any nonzero output belongs to the cartridge's actual
        // library-one power-beam program instead of merely proving that the mixer is alive.
        var soundQueue = new AudioQueues();
        SoundEffectId powerBeam = SoundEffectCommands.Decode(SoundEffectLibrary.Library1, powerBeamSound);
        soundQueue.QueueSound(powerBeam, priority: SoundRequestPriority.Essential);
        SoundAcknowledgements<SoundEffectId> acknowledgements = default;
        bool requestAcknowledged = false;
        bool clearAcknowledgedAfterRequest = false;
        int nonZeroSamples = 0;
        var soundEngine = DesktopAccess.CreateAudioSession();

        for (int frame = 0; frame < auditFrames; frame++)
        {
            IReadOnlyList<AudioRequest> commands =
                soundQueue.AdvanceFrame(acknowledgements);
            foreach (short sample in soundEngine.RenderFrame(commands))
            {
                if (sample != 0)
                    nonZeroSamples++;
            }

            acknowledgements = soundEngine.Acknowledgements;
            if (acknowledgements[0].Acknowledges(powerBeam))
                requestAcknowledged = true;
            else if (requestAcknowledged && acknowledgements[0].IsNone)
                clearAcknowledgedAfterRequest = true;
        }

        if (!requestAcknowledged)
            throw new InvalidDataException("The SPC did not acknowledge power-beam SFX $0B.");
        if (!clearAcknowledgedAfterRequest)
            throw new InvalidDataException("The SPC did not acknowledge the power-beam SFX clear.");
        if (nonZeroSamples == 0)
            throw new InvalidDataException("The cartridge power-beam SFX program generated silence.");
        return nonZeroSamples;
    }
}
