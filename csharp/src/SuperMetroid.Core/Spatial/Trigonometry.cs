namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The game's sine and cosine over trigonometry angles, counted in 256ths of a turn, and the multiplications that use
/// them (#1422): the signed sine in 256ths of its peak and the fine one peaking at 32767, their coarse and unsigned
/// half-waves and the negative-cosine view, exactly as every enemy, boss, projectile and screen effect computes them. These are engine math definitions, not editable presentation assets.
/// </summary>
public static class Trigonometry
{

    /// <summary>The fine sine's steps in each step of its coarse reading (<see cref="CoarseSizeOfFineSine"/>).</summary>
    private const int FineStepsPerCoarseStep = 256;

    /// <summary>The game's fine signed sine over the whole turn, peaking at +/-32767, unlike
    /// <see cref="SignedSine"/>. The positive half equals the unsigned half-wave
    /// (<see cref="UnsignedHalfWave"/>) halved; sign is restored only after that truncation.</summary>
    /// <remarks>For angle a in 256ths of a turn, let n = a mod 128 and
    /// U(n) = floor(65535*sin(n*pi/128)). The result is floor(U(n)/2),
    /// negated only when a &gt;= 128: a half-unit scale of 32767.5, not 32767.
    /// The values come from bounded decimal arithmetic, not Math.Sin, so every
    /// machine gives the same sine.
    /// </remarks>
    public static int FineSignedSine(TrigonometryAngle angle)
    {
        int magnitude = UnsignedHalfWave(angle.StepsIntoHalfTurn) / 2;
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>
    /// The velocity <paramref name="angle"/>'s sine gives at <paramref name="peakPixels"/> pixels an update (#2482): the signed
    /// sine's 256ths of its peak taken as 256ths of a pixel, times the peak, exactly with nothing dropped. A cosine is the sine
    /// a quarter-turn on, a negative cosine the sine a quarter-turn back.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The velocity is faster, either way, than a velocity can be.</exception>
    public static SignedEightEight SineVelocity(TrigonometryAngle angle, int peakPixels) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(peakPixels * SignedSine(angle));

    /// <summary>
    /// The displacement <paramref name="angle"/>'s sine gives at <paramref name="peakPixels"/> pixels (#2491), as a liquid's
    /// tide lifts and lowers its surface: the signed sine's 256ths of its peak taken as 256ths of a pixel, times the peak,
    /// exactly with nothing dropped. A negative cosine is the sine a quarter-turn back.
    /// </summary>
    public static SignedSixteenSixteen SineDisplacement(TrigonometryAngle angle, int peakPixels) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(peakPixels * SignedSine(angle));


    /// <summary>
    /// The size of the fine sine (<see cref="FineSignedSine"/>) at <paramref name="angle"/> read coarsely, 0 through 127:
    /// its magnitude counted in whole steps of 256 fine steps, every finer step dropped, which is its size in 128ths of
    /// its peak rounded down. The Bull and the Yapping Maw scale their motion by this reading and take the direction
    /// from the fine sine's sign.
    /// </summary>
    public static int CoarseSizeOfFineSine(TrigonometryAngle angle) => Math.Abs(FineSignedSine(angle)) / FineStepsPerCoarseStep;

    /// <summary>The game's signed sine in 256ths of its peak over the whole turn.
    /// Unlike the coarse half-wave (<see cref="CoarseHalfWave"/>), peaks are +/-256.</summary>
    /// <remarks>Keep this algorithm: sign(a)*floor(256*sin((a mod 128)*pi/128)),
    /// with positive sign for a &lt; 128, reproduces the game's values. Treat the
    /// quarter-turn magnitude as exactly 256; the coarse half-wave saturates it to 255.
    /// The game's negative cosine and full sine read these same values. The backend's scroll-wave shader (#1468) draws
    /// ripples from
    /// these same samples.</remarks>
    public static int SignedSine(TrigonometryAngle angle)
    {
        int steps = angle.StepsIntoHalfTurn;
        int magnitude = steps == 64 ? 256 : CoarseHalfWave(steps);
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>The game's coarse half-wave, the sine in 256ths of its peak over the first half turn:
    /// min(255, floor(256*sin(step*pi/128))) for <paramref name="step"/> 256ths of a turn, 0..127.</summary>
    /// <remarks>
    /// Each of the half turn's 128 steps gives the game's own value.
    /// Scale 256 before truncation; only the exact quarter-turn saturates to 255.
    /// A step outside the half turn throws ArgumentOutOfRangeException.
    /// Sign, radius truncation and separate whole/fraction negation belong to the motion that uses the wave.
    /// </remarks>
    public static int CoarseHalfWave(int step)
    {
        if ((uint)step >= 128) throw new ArgumentOutOfRangeException(nameof(step));
        return Math.Min(255, (int)(256 * UnitHalfWave(step)));
    }

    /// <summary>The game's unsigned half-wave, the sine in 65535ths of its peak over the first half turn:
    /// floor(65535*sin(step*pi/128)) for <paramref name="step"/> 256ths of a turn, 0..127.</summary>
    /// <remarks>
    /// Each of the half turn's 128 steps gives the game's own value.
    /// Scale 65535, not saturated 65536; preserve quantization
    /// before multiplication and before <see cref="FineSignedSine"/>'s halving and sign.
    /// A step outside the half turn throws ArgumentOutOfRangeException.
    /// </remarks>
    public static int UnsignedHalfWave(int step)
    {
        if ((uint)step >= 128) throw new ArgumentOutOfRangeException(nameof(step));
        return (int)(65535 * UnitHalfWave(step));
    }

    /// <summary>
    /// The game's unsigned half-wave at <paramref name="step"/> as a share of one whole (#2555): from none up to just short
    /// of the whole, every fraction finer than the half-wave's 65536th of the whole dropped, as the game's multiplications
    /// by it take it (<see cref="UnsignedHalfWave"/>).
    /// </summary>
    /// <param name="step">The step into the half turn, 0 through 127.</param>
    public static UnsignedSixteenSixteen HalfWaveShare(int step) => UnsignedSixteenSixteen.FromRaw((uint)UnsignedHalfWave(step));

    /// <summary>Reflects into the first quadrant and evaluates sine with exact endpoints and twelve decimal Taylor terms.</summary>
    /// <remarks>The omitted term is below 5.2e-21, and a margin of 1e-19 covers rounding and the 28-place pi constant, so the integer outputs need no libm or correction tables.</remarks>
    internal static decimal UnitHalfWave(int step)
    {
        if ((uint)step >= 128) throw new ArgumentOutOfRangeException(nameof(step));
        int phase = Math.Min(step, 128 - step);
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
