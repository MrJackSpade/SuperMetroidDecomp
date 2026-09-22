using System.Numerics;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyAudio(byte[] rom)
    {
        int[] pitches = NativeArray("upstream-sm/src/spc_player.c", "kBaseNoteFreqs", 13);
        int[] volumes = NativeArray("upstream-sm/src/spc_player.c", "kNoteVol", 16);
        int[] gates = NativeArray("upstream-sm/src/spc_player.c", "kNoteGateOffPct", 8);
        int[] rates = NativeArray("upstream-sm/src/snes/dsp.c", "rateValues", 32);
        FindRomArray(rom, pitches, 2, "SPC pitches");
        FindRomArray(rom, volumes, 1, "SPC note volumes");
        FindRomArray(rom, gates, 1, "SPC gates");
        for (int i = 0; i < pitches.Length; i++)
        {
            Equal(pitches[i], (int)SpcMusicTables.BaseNoteFrequencies[i], $"compiled pitch {i}");
            Equal(pitches[i], Pitch(i), $"A440 pitch {i}");
        }
        for (int i = 0; i < volumes.Length; i++)
        {
            Equal(volumes[i], (int)SpcMusicTables.NoteVolumes[i], $"compiled note volume {i}");
            Equal(volumes[i], NoteVolume(i), $"quantized note volume {i}");
        }
        for (int i = 0; i < gates.Length; i++)
        {
            Equal(gates[i], (int)SpcMusicTables.NoteGateOffPercentages[i], $"compiled gate {i}");
            Equal(gates[i], NoteGate(i), $"quantized gate {i}");
        }
        for (int i = 0; i < rates.Length; i++)
        {
            Equal(rates[i], (int)SnesDspTables.RateValues[i], $"compiled DSP rate {i}");
            Equal(rates[i], DspRate(i), $"counter DSP rate {i}");
        }
        // Test the quantization explanation over the entire percentage domain,
        // not just the authored selections. Q16 truncation and the lower-side
        // rational quantizer coincide on all integer percentages 1..100.
        for (int p = 1; p <= 100; p++)
            Equal((255 * p - 1) / 100, Percentage(p), $"Q16 percentage identity {p}");
        Equal(50, Percentage(20), "20 percent lower-side quantization");
        Equal(51, 255 * 20 / 100, "unquantized multiplication counterexample");
        CheckBounds(Pitch, 12);
        CheckBounds(NoteVolume, 15);
        CheckBounds(NoteGate, 7);
        CheckBounds(DspRate, 31);
        Console.WriteLine("PASS: 69/69 audio values: 13 exact A440 pitches, 16 note volumes, 8 gate fractions, 32 DSP periods.");
    }

    private static int Pitch(int i)
    {
        Bound(i, 12);
        // floor(440*8.192*2^((i-9)/12)), proved using an integer twelfth root.
        // The scale is 2 * 4096 / 1000. No Math.Pow or fitted starting C value.
        BigInteger numerator = BigInteger.Pow(440 * 8192, 12) << i;
        BigInteger denominator = BigInteger.Pow(1000, 12) << 9;
        int low = 0, high = 8192;
        while (low + 1 < high)
        {
            int middle = (low + high) / 2;
            if (BigInteger.Pow(middle, 12) * denominator <= numerator) low = middle;
            else high = middle;
        }
        if (BigInteger.Pow(low, 12) * denominator > numerator ||
            BigInteger.Pow(low + 1, 12) * denominator <= numerator)
            throw new InvalidDataException("Pitch root interval was not proven.");
        return low;
    }

    private static int Percentage(int p)
    {
        if (p < 1 || p > 100) throw new ArgumentOutOfRangeException(nameof(p));
        // Truncate 255/100 to sixteen fractional bits BEFORE multiplication.
        int q16Scale = (255 << 16) / 100;
        return q16Scale * p >> 16;
    }

    private static int NoteVolume(int i)
    {
        Bound(i, 15);
        int percentage = i < 4 ? 10 * (i + 1) : i == 15 ? 99 : 5 * (i + 5);
        return Percentage(percentage);
    }

    private static int NoteGate(int i)
    {
        Bound(i, 7);
        int percentage = i < 2 ? 20 * (i + 1) : i == 7 ? 99 : 10 * (i + 3);
        return Percentage(percentage);
    }

    private static int DspRate(int i)
    {
        Bound(i, 31);
        if (i == 0) return 0;
        if (i == 31) return 1;
        int group = (i - 1) / 3, phase = (i - 1) % 3;
        return ((8 - 2 * phase + phase / 2) << 8) >> group;
    }

    private static int[] NativeArray(string file, string symbol, int count)
    {
        string source = File.ReadAllText(file);
        Match match = Regex.Match(source, @"\b" + Regex.Escape(symbol) + @"\[" + count + @"\]\s*=\s*\{([^}]+)\}");
        if (!match.Success) throw new InvalidDataException($"Missing native reference {symbol}.");
        int[] result = Regex.Matches(match.Groups[1].Value, @"0x[\da-fA-F]+|\d+")
            .Select(m => m.Value.StartsWith("0x", StringComparison.Ordinal)
                ? Convert.ToInt32(m.Value[2..], 16) : int.Parse(m.Value)).ToArray();
        Equal(count, result.Length, $"native array length {symbol}");
        return result;
    }

    private static void FindRomArray(byte[] rom, int[] oracle, int size, string name)
    {
        byte[] data = oracle.SelectMany(n => Enumerable.Range(0, size).Select(b => (byte)(n >> (8 * b)))).ToArray();
        int offset = rom.AsSpan().IndexOf(data);
        if (offset < 0) throw new InvalidDataException($"Pinned native {name} missing from supported cartridge.");
        Console.WriteLine($"ROM oracle: {name}, {oracle.Length} entries at file offset ${offset:X6}.");
    }
}
