using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPhantoonWaveMath(SuperMetroidAddressSpace rom)
    {
        var samples = new short[512];
        for (int offset = 0; offset < samples.Length; offset++)
        {
            int address = EnemyMathReferenceData.PhantoonSineByteRange + offset;
            samples[offset] = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }

        static int NativeDisplacement(short sample, ushort amplitude)
        {
            // Literal four byte products and word-width intermediates from
            // $88:E5CF-E63E/$E65A-E6C9, including the final AND $FF00 / XBA.
            int magnitude = Math.Abs((int)sample);
            int low = magnitude & 255, high = magnitude >> 8;
            int amplitudeLow = amplitude & 255, amplitudeHigh = amplitude >> 8;
            ushort temp16 = (ushort)((low * amplitudeLow >> 8) + high * amplitudeLow);
            ushort temp18 = (ushort)(low * amplitudeHigh);
            temp16 = unchecked((ushort)(temp16 + temp18));
            int result = ((temp16 + ((high * amplitudeHigh & 255) << 8)) & 65280) >> 8;
            return sample < 0 ? -result : result;
        }

        for (int phase = 0; phase <= ushort.MaxValue; phase++)
            AssertEqual(samples[phase & 511], PhantoonWaveRomData.ReadSineAtBytePhase((ushort)phase),
                "Phantoon byte-phase read and wrapping");
        var displacementReader = typeof(PhantoonWaveTable)
            .GetMethod("CalculateDisplacement", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ushort, ushort, int>>();
        for (int phase = 0; phase < 512; phase++)
        for (int amplitude = 0; amplitude <= ushort.MaxValue; amplitude++)
        {
            if (displacementReader((ushort)phase, (ushort)amplitude) !=
                NativeDisplacement(samples[phase], (ushort)amplitude))
                throw new InvalidDataException($"Phantoon byte product differs: phase={phase}, amplitude={amplitude}.");
        }

        var actual = new ushort[128];
        // Full cycles cover byte-offset wrapping, both mode lengths, mirrored half,
        // signed scroll wrap and extreme products, including odd restored phases.
        foreach (ushort mode in new ushort[] { 1, 2 })
        foreach (ushort amplitude in new ushort[] { 0, 1, 255, 256, 3072, 32768, 65535 })
        foreach (ushort scroll in new ushort[] { 0, 32768, 65535 })
        for (int phase = 0; phase < 512; phase++)
        {
            int half = mode == 1 ? 64 : 32;
            int step = 512 / (half * 2);
            PhantoonWaveTable.Build(mode, (ushort)(phase | 65024), amplitude, scroll, actual.AsSpan(0, half * 2));
            for (int i = 0; i < half; i++)
            {
                int displacement = NativeDisplacement(samples[(phase + i * step) & 511], amplitude);
                if (actual[i] != unchecked((ushort)(scroll + displacement)) ||
                    actual[i + half] != unchecked((ushort)(scroll - displacement)))
                    throw new InvalidDataException($"Phantoon wave differs: mode={mode}, phase={phase}, amplitude={amplitude}, scroll={scroll}, index={i}.");
            }
        }
        var longCycle = new ushort[128];
        var shortCycle = new ushort[64];
        PhantoonWaveTable.Build(1, 511, 65535, 65535, longCycle);
        PhantoonWaveTable.Build(2, 511, 65535, 65535, shortCycle);
        for (int mode = 1; mode <= ushort.MaxValue; mode++)
        {
            ushort[] expected = (mode & 1) != 0 ? longCycle : shortCycle;
            PhantoonWaveTable.Build((ushort)mode, 511, 65535, 65535, actual.AsSpan(0, expected.Length));
            if (!actual.AsSpan(0, expected.Length).SequenceEqual(expected))
                throw new InvalidDataException($"Phantoon mode bits differ: {mode:X4}.");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonWaveTable.Build(0, 0, 0, 0, actual),
            "inactive wave mode cannot build a cycle");
        AssertThrows<ArgumentException>(() => PhantoonWaveTable.Build(2, 0, 0, 0, actual),
            "short wave requires exactly 64 output words");
        AssertThrows<ArgumentException>(() => PhantoonWaveTable.Build(1, 0, 0, 0, shortCycle),
            "long wave requires exactly 128 output words");
        Console.WriteLine("Phantoon wave math: 33,554,432 native byte products, all phase/mode words and 21,504 complete cycles match odd/even reads, mirroring and scroll wrapping without a bus.");
    }
}
