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
    /// <remarks>Uses double precision without stored samples or a generated cache.
    /// Every rounded coefficient is checked against the independent native table.
    /// Invalid indices preserve the former array exception.</remarks>
    internal static ushort GaussianCoefficient(int index)
    {
        if ((uint)index >= 512) throw new IndexOutOfRangeException();
        var taps = GaussianCoefficients(index & 255);
        return index < 256 ? taps.Tap3 : taps.Tap2;
    }

    /// <summary>Calculates the four S-DSP interpolation taps for one 8-bit sample phase.</summary>
    /// <remarks>The native order is indices 255-phase, 511-phase, 256+phase, phase.
    /// Share the four raw values and their normalization across the sample. Rebuilding
    /// them separately with decimal Taylor series dominated real-time frame processing.</remarks>
    internal static (ushort Tap0, ushort Tap1, ushort Tap2, ushort Tap3) GaussianCoefficients(int phase)
    {
        if ((uint)phase >= 256) throw new IndexOutOfRangeException();
        double tap0 = GaussianRaw(255 - phase);
        double tap1 = GaussianRaw(511 - phase);
        double tap2 = GaussianRaw(256 + phase);
        double tap3 = GaussianRaw(phase);
        double scale = 2048 / (tap0 + tap1 + tap2 + tap3);
        return ((ushort)(tap0 * scale + .5), (ushort)(tap1 * scale + .5),
            (ushort)(tap2 * scale + .5), (ushort)(tap3 * scale + .5));
    }

    private static double GaussianRaw(int index)
    {
        double k = 511.5 - index;
        double window = .42 + .50 * Math.Cos(2 * Math.PI * k / 1023)
            + .08 * Math.Cos(4 * Math.PI * k / 1023);
        return Math.Sin(Math.PI * k / 800) * window / k;
    }
}
