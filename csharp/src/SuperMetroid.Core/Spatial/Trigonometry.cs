namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The game's sine and cosine samples over trigonometry angles and the multiplications that use them (#1422): the signed
/// 1/256-step sine wave and the one peaking at 32767, their unsigned half-waves and the negative-cosine view, exactly as every enemy, boss,
/// projectile and screen effect samples them. These are engine math definitions, not editable presentation assets.
/// </summary>
public static class Trigonometry
{
    /// <summary>The game's sine table peaking at 32767 and its three
    /// quadrant continuations. These signed samples peak at +/-32767, unlike
    /// the signed 1/256-step table. The positive half equals the stored unsigned
    /// half-wave halved; sign is restored only after that truncation.</summary>
    /// <remarks>Independently reviewed for #1165; keep this existing algorithm: for angle a in 256ths of a turn, let n = a mod 128 and
    /// U(n) = floor(65535*sin(n*pi/128)). The exact result is floor(U(n)/2),
    /// negated only when a &gt;= 128. All 256 values match the game's table
    /// exactly. This preserves
    /// the half-unit scale (32767.5), rather than assuming a scale of 32767.
    /// The #1165 research evaluator used bounded decimal arithmetic, not Math.Sin.
    /// Bull adds a quarter-turn for X, while Yapping Maw negates and narrows its
    /// input angle before sampling. Individual investigation: #625 / #909.
    /// </remarks>
    public static int FineSignedSine(TrigonometryAngle angle)
    {
        int magnitude = UnsignedHalfWave(angle.HalfWaveIndex) >> 1;
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>
    /// The sine or cosine product: <paramref name="speed"/>, read unsigned as the game's multiply reads it, scaled by
    /// <paramref name="angle"/>'s signed 1/256-step sine (<see cref="ScaledBySine"/>), the result wrapping as a velocity does.
    /// Callers add a quarter-turn themselves when selecting cosine.
    /// </summary>
    public static SignedEightEight MultiplySignedSine(SignedEightEight speed, TrigonometryAngle angle) =>
        SignedEightEight.Wrapped(ScaledBySine(speed.UnsignedTwoHundredFiftySixthsOfPixel, angle));

    /// <summary>
    /// <paramref name="magnitude"/>, 0 through 65535 in any unit, times <paramref name="angle"/>'s signed 1/256-step sine: the
    /// product of the magnitude and the sine's size with its 256ths dropped, then the sine's sign restored, so the result
    /// rounds toward zero in the magnitude's own unit.
    /// </summary>
    public static int ScaledBySine(int magnitude, TrigonometryAngle angle)
    {
        int sample = SignedSine(angle);
        int product = EightEight.WholeOf(Ensure.BetweenInclusive(0, ushort.MaxValue, magnitude) * Math.Abs(sample));
        return sample < 0 ? -product : product;
    }

    /// <summary>The game's signed 1/256-step sine table and
    /// its three quadrant continuations. Unlike the small table, peaks are +/-256.</summary>
    /// <remarks>Independently reviewed for #1165; keep this existing algorithm: sign(a)*floor(256*sin((a &amp; 127)*pi/128)),
    /// with positive sign for a &lt; 128, reproduces the game's values. Treat the
    /// quarter-turn magnitude as exactly 256; the small table saturates it to 255.
    /// The #1165 research also checked the entire 320-entry negative-cosine prefix/full
    /// sine view. The backend's scroll-wave shader (#1468) draws ripples from
    /// these same samples.
    /// Individual investigation: #625 / #910.</remarks>
    public static int SignedSine(TrigonometryAngle angle)
    {
        int halfWaveIndex = angle.HalfWaveIndex;
        int magnitude = halfWaveIndex == 64 ? 256 : EightBitHalfWave(halfWaveIndex);
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>The game's small sine table and cosine continuation:
    /// min(255, floor(256*sin(index*pi/128))) for index 0..127.</summary>
    /// <remarks>
    /// Independently checked for #1165 against all 128 of the game's values.
    /// Scale 256 before truncation; only the exact quarter-turn saturates to 255.
    /// Invalid indices throw ArgumentOutOfRangeException.
    /// Sign, radius truncation and separate whole/fraction negation stay in callers.
    /// </remarks>
    public static int EightBitHalfWave(int index)
    {
        if ((uint)index >= 128) throw new ArgumentOutOfRangeException(nameof(index));
        return Math.Min(255, (int)(256 * UnitHalfWave(index)));
    }

    /// <summary>The game's unsigned sine table:
    /// floor(65535*sin(index*pi/128)) for index 0..127.</summary>
    /// <remarks>
    /// Independently checked for #1165 against all 128 of the game's values.
    /// Scale 65535, not saturated 65536; preserve quantization
    /// before multiplication and before <see cref="FineSignedSine"/>'s halving and sign.
    /// This identifies the exact numerical convention, not the historical authoring tool.
    /// Invalid indices throw ArgumentOutOfRangeException.
    /// </remarks>
    public static int UnsignedHalfWave(int index)
    {
        if ((uint)index >= 128) throw new ArgumentOutOfRangeException(nameof(index));
        return (int)(65535 * UnitHalfWave(index));
    }

    /// <summary>Reflects into the first quadrant and evaluates sine with exact endpoints and twelve decimal Taylor terms.</summary>
    /// <remarks>The omitted term is below 5.2e-21; the reviewed 1e-19 margin covers rounding and the 28-place pi constant. Full-domain comparisons with the game's tables establish the integer outputs without libm or correction tables.</remarks>
    internal static decimal UnitHalfWave(int index)
    {
        if ((uint)index >= 128) throw new ArgumentOutOfRangeException(nameof(index));
        int phase = Math.Min(index, 128 - index);
        if (phase == 0) return 0;
        if (phase == 64) return 1;
        decimal x = phase * 3.1415926535897932384626433833m / 128;
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 12; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }
}
