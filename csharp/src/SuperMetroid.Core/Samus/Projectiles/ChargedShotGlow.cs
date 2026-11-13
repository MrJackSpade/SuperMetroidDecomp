namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The glow Samus's suit shows after a charged shot (#1682), each kind with its own step count: an ordinary charged
/// shot's flash, which the charge palette counts down from four, showing white on steps three through one and restoring
/// the suit as it reaches zero; or the Hyper Beam's glow, counted down from twenty, which shows its frame on each even step
/// and holds it on the odd step after, and restores the suit on step zero, its twenty-first call.
/// </summary>
/// <remarks>
/// The glow's kind decides how it steps. The game decided by whether the Hyper Beam is equipped, and only a Hyper
/// Beam glow ever runs while it is: the Hyper Beam fires no ordinary charged shot and is never unequipped.
/// </remarks>
public readonly record struct ChargedShotGlow
{
    /// <summary>The steps an ordinary charged shot's flash starts with.</summary>
    private const int ChargedShotFlashSteps = 4;

    /// <summary>The steps the Hyper Beam's glow starts with: two for each of its ten frames.</summary>
    private const int HyperBeamGlowSteps = 20;

    /// <summary>Creates a glow of <paramref name="kind"/> with <paramref name="remainingSteps"/> left.</summary>
    private ChargedShotGlow(ChargedShotGlowKind kind, int remainingSteps)
    {
        Kind = kind;
        RemainingSteps = remainingSteps;
    }

    /// <summary>No glow.</summary>
    public static ChargedShotGlow None => default;

    /// <summary>An ordinary charged shot's flash as the shot starts it.</summary>
    public static ChargedShotGlow ChargedShotFlash { get; } = new(ChargedShotGlowKind.ChargedShotFlash, ChargedShotFlashSteps);

    /// <summary>The Hyper Beam's glow as its shot starts it.</summary>
    public static ChargedShotGlow HyperBeamGlow { get; } = new(ChargedShotGlowKind.HyperBeamGlow, HyperBeamGlowSteps);

    /// <summary>
    /// A glow of <paramref name="kind"/> part-way through: an ordinary flash with one through four steps left, or the
    /// Hyper Beam's glow with zero through twenty; no glow holds none.
    /// </summary>
    /// <param name="kind">The glow's kind.</param>
    /// <param name="remainingSteps">The steps it has left.</param>
    public static ChargedShotGlow Of(ChargedShotGlowKind kind, int remainingSteps) => kind switch
    {
        ChargedShotGlowKind.None => new(kind, Ensure.BetweenInclusive(0, 0, remainingSteps)),
        ChargedShotGlowKind.ChargedShotFlash => new(kind, Ensure.BetweenInclusive(1, ChargedShotFlashSteps, remainingSteps)),
        ChargedShotGlowKind.HyperBeamGlow => new(kind, Ensure.BetweenInclusive(0, HyperBeamGlowSteps, remainingSteps)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, $"Undefined {nameof(ChargedShotGlowKind)}."),
    };

    /// <summary>Which glow this is.</summary>
    public ChargedShotGlowKind Kind { get; }

    /// <summary>The steps the glow has left; see <see cref="Of"/> for each kind's range.</summary>
    [IntegerQuantity("charge palette calls the glow has left")]
    public int RemainingSteps { get; }

    /// <summary>The glow one step on; the step count stays within the kind's range, which a caller ends at zero.</summary>
    internal ChargedShotGlow Stepped() => new(Kind, Ensure.AtLeastZero(RemainingSteps - 1));
}
