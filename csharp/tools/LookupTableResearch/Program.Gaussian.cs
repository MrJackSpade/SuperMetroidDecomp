using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifyGaussian()
    {
        // Independently transcribed analytic recipe, credited to Mednafen/nocash:
        // https://forums.nesdev.org/viewtopic.php?start=15&t=10586
        // The expected integers come ONLY from the pinned local native reference.
        int[] native = NativeArray("upstream-sm/src/snes/dsp.c", "gaussValues", 512);
        decimal globalSum = Enumerable.Range(0, 512).Sum(GaussianRaw);
        int globalMismatches = 0;
        for (int i = 0; i < 512; i++)
        {
            Equal(native[i], (int)SnesDspTables.GaussianValues[i], $"compiled DSP Gaussian {i}");
            Equal(native[i], Gaussian(i), $"analytic DSP Gaussian {i}");
            if ((int)decimal.Floor(262144 * GaussianRaw(i) / globalSum + 0.5m) != native[i]) globalMismatches++;
        }
        Equal(23, globalMismatches, "global normalization is not phase normalization");
        CheckBounds(Gaussian, 511);
        Console.WriteLine("PASS: 512/512 DSP Gaussian coefficients from phase-normalized windowed sinc, with deterministic rounding intervals.");
    }

    private static int Gaussian(int index)
    {
        Bound(index, 511);
        int p = index % 256;
        decimal raw = GaussianRaw(index);
        decimal sum = GaussianRaw(p) + GaussianRaw(255 - p) + GaussianRaw(256 + p) + GaussianRaw(511 - p);
        // Raw absolute error < 1e-17 (conservative propagation of FullSine's
        // 1e-19 bound, cosine reduction, window products and division by k>=0.5).
        const decimal e = 0.00000000000000001m;
        decimal lower = decimal.Floor(2048 * Math.Max(0, raw - e) / (sum + 4 * e) + 0.5m);
        decimal upper = decimal.Floor(2048 * (raw + e) / (sum - 4 * e) + 0.5m);
        Equal(lower, upper, $"Gaussian rounding interval {index}");
        return (int)lower;
    }

    private static decimal GaussianRaw(int index)
    {
        decimal k = 511.5m - index;
        decimal window = 0.42m + 0.50m * FullCosine(2 * ResearchData.Pi * k / 1023)
            + 0.08m * FullCosine(4 * ResearchData.Pi * k / 1023);
        return FullSine(ResearchData.Pi * k / 800) * window / k;
    }

    private static decimal FullCosine(decimal x)
    {
        if (x < 0 || x > 2 * ResearchData.Pi) throw new ArgumentOutOfRangeException(nameof(x));
        if (x > ResearchData.Pi) x = 2 * ResearchData.Pi - x;
        if (x == 0) return 1;
        if (x == ResearchData.Pi) return -1;
        return x <= ResearchData.Pi / 2 ? FullSine(ResearchData.Pi / 2 - x) : -FullSine(x - ResearchData.Pi / 2);
    }
}
