namespace SuperMetroid.Core.Audio;

/// <summary>
/// Immutable timing and interpolation constants defined by the SNES S-DSP.
/// Keeping the hardware tables separate from the mixer makes their provenance explicit and
/// prevents register and envelope code from accumulating unexplained numeric literals.
/// </summary>
internal static class SnesDspTables
{
    /// <summary>
    /// Number of native DSP sample cycles between envelope or noise updates for each five-bit
    /// hardware rate selector. Selector zero disables the corresponding timer.
    /// </summary>
    /// <remarks>
    /// Pinned upstream-sm/src/snes/dsp.c rateValues: repeating 8:6:5 periods halve
    /// every three selectors. Integer shifts preserve the final truncated period.
    /// These are hardware periods, not cartridge data. Timer phase and admission
    /// remain the responsibility of the envelope and noise consumers.
    /// </remarks>
    internal static ushort RatePeriod(int selector)
    {
        if ((uint)selector >= 32)
            throw new IndexOutOfRangeException();
        if (selector == 0) return 0;
        if (selector == 31) return 1;
        int group = (selector - 1) / 3;
        int ratio = ((selector - 1) % 3) switch { 0 => 8, 1 => 6, _ => 5 };
        return (ushort)((ratio << 8) >> group);
    }
    /// <summary>Exact S-DSP interpolation coefficient for index 0..511.
    /// A Blackman-windowed sinc is normalized over each four-tap phase, then
    /// rounded to the nearest integer at scale 2048. Independently checked
    /// against every gaussValues coefficient in pinned upstream-sm/src/snes/dsp.c.</summary>
    /// <remarks>Uses deterministic decimal evaluation without stored samples or
    /// a generated cache. Invalid indices preserve the former array exception.</remarks>
    internal static ushort GaussianCoefficient(int index)
    {
        if ((uint)index >= 512) throw new IndexOutOfRangeException();
        int phase = index & 255;
        decimal sum = GaussianRaw(phase) + GaussianRaw(255 - phase)
            + GaussianRaw(256 + phase) + GaussianRaw(511 - phase);
        return (ushort)(2048 * GaussianRaw(index) / sum + .5m);
    }

    private static decimal GaussianRaw(int index)
    {
        const decimal pi = 3.1415926535897932384626433833m;
        decimal k = 511.5m - index;
        decimal window = .42m + .50m * Cosine(2 * pi * k / 1023)
            + .08m * Cosine(4 * pi * k / 1023);
        return Sine(pi * k / 800) * window / k;
    }

    private static decimal Cosine(decimal angle)
    {
        const decimal pi = 3.1415926535897932384626433833m;
        if (angle > pi) angle = 2 * pi - angle;
        if (angle == 0) return 1;
        if (angle == pi) return -1;
        return angle <= pi / 2 ? Sine(pi / 2 - angle) : -Sine(angle - pi / 2);
    }

    // All callers supply angles in [0,2*pi]. Twenty-four terms leave less than
    // 8e-24 omitted remainder; the independent proof bounds propagated rounding.
    private static decimal Sine(decimal angle)
    {
        decimal squared = angle * angle;
        decimal term = angle, sum = angle;
        for (int k = 1; k < 24; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }
}
