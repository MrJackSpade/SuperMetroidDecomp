using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Result of the non-GUI cartridge audio diagnostic.</summary>
public readonly record struct CartridgeAudioSmokeTestResult(
    int FramesGenerated,
    int NonZeroSamples,
    int PeakAmplitude);

/// <summary>Runs the real title music path without opening an audio device or game window.</summary>
public static class CartridgeAudioSmokeTest
{
    public static CartridgeAudioSmokeTestResult Run(string romPath, int frames = 240)
    {
        if (frames <= 0)
            throw new ArgumentOutOfRangeException(nameof(frames));

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var queue = new CartridgeAudioState();
        queue.QueueMusicDelayed8(0xff03);
        queue.QueueMusicDelayed8(5);

        int nonZeroSamples = 0;
        int peakAmplitude = 0;
        CartridgeAudioAcknowledgements acknowledgements = default;
        using var engine = new SpcAudioEngine(bus);
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
        return new CartridgeAudioSmokeTestResult(frames, nonZeroSamples, peakAmplitude);
    }
}
