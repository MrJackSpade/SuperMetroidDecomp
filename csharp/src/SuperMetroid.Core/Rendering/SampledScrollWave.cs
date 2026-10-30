using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A plane rippled along one axis by a periodic waveform given as one period of samples (#1468), as the liquids' and the
/// heat haze's waves are: every line shows <see cref="BaseScroll"/>, and each line from <see cref="FirstLine"/> up to
/// <see cref="EndLine"/> is moved by the sample its line reaches times <see cref="Amplitude"/>. The waveform repeats every
/// <see cref="Period"/> lines, one sample a line. The backend's shader evaluates it line by line; nothing keeps a table of
/// lines.
/// </summary>
/// <remarks>
/// Screen line <c>y</c> reads sample <c>(Phase + y) mod Period</c>; its displacement is that sample times
/// <see cref="Amplitude"/> in 1/256ths of a pixel, rounded down.
/// </remarks>
/// <param name="BaseScroll">The scroll every line shows before its displacement, 0 through 65535.</param>
/// <param name="Samples">One period of the waveform, each sample a signed displacement in whole units of the amplitude.</param>
/// <param name="Amplitude">The displacement of a sample of one, in 1/256ths of a pixel, 0 through 65535.</param>
/// <param name="Phase">The sample screen line zero reaches, 0 through the period less one.</param>
/// <param name="FirstLine">The first screen line the wave moves, 0 through the screen's height.</param>
/// <param name="EndLine">The screen line after the last the wave moves, <paramref name="FirstLine"/> through the screen's height.</param>
public sealed record SampledScrollWave(int BaseScroll, ImmutableArray<int> Samples, int Amplitude, int Phase, int FirstLine, int EndLine)
    : LineScroll
{
    /// <summary>The scroll every line shows before its displacement, 0 through 65535.</summary>
    public override int BaseScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, BaseScroll);

    /// <summary>One period of the waveform, at least one sample.</summary>
    /// <exception cref="ArgumentException">The waveform has no samples.</exception>
    public ImmutableArray<int> Samples
    {
        get;
        init => field = RequirePeriod(value);
    } = RequirePeriod(Samples);

    /// <summary>The displacement of a sample of one, in 1/256ths of a pixel.</summary>
    public int Amplitude
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, Amplitude);

    /// <summary>The sample screen line zero reaches.</summary>
    public int Phase
    {
        get;
        init => field = Ensure.BetweenInclusive(0, Samples.Length - 1, value);
    } = Ensure.BetweenInclusive(0, RequirePeriod(Samples).Length - 1, Phase);

    /// <summary>The first screen line the wave moves.</summary>
    public int FirstLine
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, FirstLine);

    /// <summary>The screen line after the last the wave moves; lines from it down show <see cref="BaseScroll"/>.</summary>
    public int EndLine
    {
        get;
        init => field = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, EndLine);

    /// <summary>The screen lines of one whole period: one sample a line.</summary>
    public int Period => Samples.Length;

    /// <summary>
    /// The sample screen line zero reaches when line <paramref name="line"/> reaches sample <paramref name="sample"/>:
    /// the phase of a waveform whose position is counted from a line other than the screen's first.
    /// </summary>
    /// <param name="sample">The sample, any whole number; only its place within the period is read.</param>
    /// <param name="line">The screen line that reaches it, any whole number.</param>
    /// <param name="period">The waveform's samples, at least one.</param>
    public static int PhaseWhere(int sample, int line, int period)
    {
        int remainder = (sample - line) % Ensure.BetweenInclusive(1, int.MaxValue, period);
        return remainder < 0 ? remainder + period : remainder;
    }

    /// <summary>Requires a waveform of at least one sample.</summary>
    /// <exception cref="ArgumentException">The waveform is empty or default.</exception>
    private static ImmutableArray<int> RequirePeriod(ImmutableArray<int> samples) => !samples.IsDefaultOrEmpty ? samples
        : throw new ArgumentException("A sampled wave has at least one sample in its period.", nameof(samples));
}
