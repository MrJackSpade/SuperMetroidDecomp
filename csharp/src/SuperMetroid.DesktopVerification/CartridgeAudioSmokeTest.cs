using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Result of the non-GUI cartridge audio diagnostic.</summary>
public readonly record struct CartridgeAudioSmokeTestResult(
    int FramesGenerated,
    int NonZeroSamples,
    int PeakAmplitude,
    int PowerBeamNonZeroSamples);

/// <summary>Runs the real title music path without opening an audio device or game window.</summary>
public static class CartridgeAudioSmokeTest
{
    public static CartridgeAudioSmokeTestResult Run(string romPath, int frames = 240)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frames);

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var queue = new CartridgeAudioState();
        queue.QueueMusicDelayed8(MusicCommand.LoadData(0x03));
        queue.QueueMusicDelayed8(MusicCommand.SelectTrack(5));

        int nonZeroSamples = 0;
        int peakAmplitude = 0;
        CartridgeAudioAcknowledgements acknowledgements = default;
        using var engine = new SpcAudioEngine();
        for (int frame = 0; frame < frames; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands =
                queue.AdvanceFrame(bus, acknowledgements);
            foreach (short sample in engine.RenderFrame(commands))
            {
                if (sample != 0)
                    nonZeroSamples++;
                int amplitude = sample == short.MinValue ? 32768 : Math.Abs(sample);
                peakAmplitude = Math.Max(peakAmplitude, amplitude);
            }
            acknowledgements = engine.ReadAcknowledgements();
        }

        if (nonZeroSamples == 0 || peakAmplitude == 0)
        {
            throw new InvalidDataException(
                "The translated SPC/DSP engine generated silence for the title music sequence.");
        }

        int powerBeamNonZeroSamples = VerifyPowerBeamSoundAndHandshake(bus);
        return new CartridgeAudioSmokeTestResult(
            frames,
            nonZeroSamples,
            peakAmplitude,
            powerBeamNonZeroSamples);
    }

    /// <summary>
    /// Proves library one's real request/clear protocol using the ROM-selected power-beam ID.
    /// </summary>
    private static int VerifyPowerBeamSoundAndHandshake(ISnesAddressSpace bus)
    {
        const byte powerBeamSound = 0x0b;
        const int auditFrames = 30;

        // A fresh player receives only reset's common driver/sample upload.  Keeping title
        // music out of this pass means any nonzero output belongs to the cartridge's actual
        // library-one power-beam program instead of merely proving that the mixer is alive.
        var soundQueue = new CartridgeAudioState();
        soundQueue.QueueSound(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, powerBeamSound),
            maximumQueued: 15);
        CartridgeAudioAcknowledgements acknowledgements = default;
        bool requestAcknowledged = false;
        bool clearAcknowledgedAfterRequest = false;
        int nonZeroSamples = 0;
        using var soundEngine = new SpcAudioEngine();

        for (int frame = 0; frame < auditFrames; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands =
                soundQueue.AdvanceFrame(bus, acknowledgements);
            foreach (short sample in soundEngine.RenderFrame(commands))
            {
                if (sample != 0)
                    nonZeroSamples++;
            }

            acknowledgements = soundEngine.ReadAcknowledgements();
            if (acknowledgements[1] == powerBeamSound)
                requestAcknowledged = true;
            else if (requestAcknowledged && acknowledgements[1] == 0)
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
