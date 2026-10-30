using static SuperMetroid.Verification.Hardware.WordSamplesVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Enemies;

/// <summary>
/// Checks that the sine wave Phantoon's wave is described as (#1468) gives every screen line the scroll the cartridge's
/// table gives it, against native signed-sine samples and byte products.
/// </summary>
internal static class PhantoonWaveMathVerification
{
    /// <summary>
    /// Compares, for both wave modes, every even phase, every amplitude-word boundary class and the scroll's wrapping
    /// edges, the described wave's scroll on every screen line with the cartridge's mirrored cycle repeated down the
    /// screen; and checks that every mode word selects its period by bit zero alone. An odd phase, which only a restored
    /// phase can hold, shows the even phase's angle below it; the cartridge's read straddling two samples there is a
    /// presentation quirk the description does not keep.
    /// </summary>
    internal static void VerifyPhantoonWaveMath(SuperMetroidAddressSpace rom)
    {
        var samples = new short[512];
        for (int offset = 0; offset < samples.Length; offset += 2)
        {
            int address = EnemyMathReferenceData.SignedSine + offset;
            samples[offset] = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        }

        static int NativeDisplacement(short sample, ushort amplitude)
        {
            // Literal four byte products and word-width intermediates from
            // $88:E5CF-E63E/$E65A-E6C9, including the final AND $FF00 / XBA.
            int magnitude = Math.Abs((int)sample);
            int low = unchecked((byte)magnitude), high = LittleEndianBytes.HighByte(unchecked((ushort)magnitude));
            int amplitudeLow = LittleEndianBytes.LowByte(amplitude), amplitudeHigh = LittleEndianBytes.HighByte(amplitude);
            ushort temp16 = (ushort)((EightEight.Multiply(low, amplitudeLow)) + high * amplitudeLow);
            ushort temp18 = (ushort)(low * amplitudeHigh);
            temp16 = unchecked((ushort)(temp16 + temp18));
            int result = LittleEndianBytes.HighByte(unchecked((ushort)(temp16 + LittleEndianBytes.Word(0, unchecked((byte)(high * amplitudeHigh))))));
            return sample < 0 ? -result : result;
        }

        // Every even phase against every amplitude-word boundary class: the wave's displacement at its first line.
        for (int phase = 0; phase < 512; phase += 2)
        foreach (int amplitude in WordBoundarySamples())
        {
            ScrollWave wave = BossLineScrolls.Of(PhantoonWaveEffect.Describe(2, new PhantoonWaveShape(phase, amplitude, 0)));
            if (ScrollShapeLines.WaveDisplacement(wave, 0) != NativeDisplacement(samples[phase], (ushort)amplitude))
                throw new InvalidDataException($"Phantoon byte product differs: phase={phase}, amplitude={amplitude}.");
        }

        // Whole screens cover byte-offset wrapping, both mode lengths, the mirrored half, signed scroll wrap and
        // extreme products.
        int cycles = 0;
        foreach (ushort mode in new ushort[] { 1, 2 })
        foreach (ushort amplitude in new ushort[] { 0, 1, 255, 256, 3072, 32768, 65535 })
        foreach (ushort scroll in new ushort[] { 0, 32768, 65535 })
        for (int phase = 0; phase < 512; phase += 2)
        {
            // The cartridge builds one cycle, its second half the first's mirror, and repeats it from the top line.
            int half = mode == 1 ? 64 : 32;
            int step = 512 / (half * 2);
            ScrollWave wave = BossLineScrolls.Of(PhantoonWaveEffect.Describe(mode, new PhantoonWaveShape(phase, amplitude, scroll)));
            for (int line = 0; line < ScreenGeometry.ScreenHeightPixels; line++)
            {
                int index = line % (half * 2);
                int displacement = NativeDisplacement(samples[(phase + index % half * step) & 511], amplitude);
                ushort native = unchecked((ushort)(index < half ? scroll + displacement : scroll - displacement));
                if (ScrollShapeLines.WaveScroll(wave, line) != native)
                    throw new InvalidDataException($"Phantoon wave differs: mode={mode}, phase={phase}, amplitude={amplitude}, scroll={scroll}, line={line}.");
            }
            cycles++;
        }
        for (int mode = 1; mode <= ushort.MaxValue; mode++)
        {
            int expected = (mode & 1) != 0 ? 128 : 64;
            if (PhantoonWaveEffect.Describe(mode, new PhantoonWaveShape(0, 0, 0)).Period != expected)
                throw new InvalidDataException($"Phantoon mode bits differ: {mode:X4}.");
        }
        AssertEqual(PhantoonWaveEffect.Describe(2, new PhantoonWaveShape(510, 0, 0)).Phase,
            PhantoonWaveEffect.Describe(2, new PhantoonWaveShape(511, 0, 0)).Phase, "an odd phase shows the even phase's angle below it");
        Console.WriteLine($"Phantoon wave math: every even phase's byte products, {cycles} described waves giving every screen line the cartridge's mirrored, repeated cycle, and every mode word's period by bit zero match.");
    }
}
