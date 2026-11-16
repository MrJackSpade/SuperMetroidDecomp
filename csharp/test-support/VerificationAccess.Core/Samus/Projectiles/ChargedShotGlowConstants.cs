namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="ChargedShotGlow"/> that only verification reads.</summary>
internal static class ChargedShotGlowConstants
{
    /// <summary>
    /// A glow of <paramref name="kind"/> part-way through: an ordinary flash with one through four steps left, or the
    /// Hyper Beam's glow with zero through twenty; no glow holds none.
    /// </summary>
    /// <param name="kind">The glow's kind.</param>
    /// <param name="remainingSteps">The steps it has left.</param>
    public static ChargedShotGlow Of(ChargedShotGlowKind kind, int remainingSteps) => kind switch
    {
        ChargedShotGlowKind.None => new(kind, Ensure.BetweenInclusive(0, 0, remainingSteps)),
        ChargedShotGlowKind.ChargedShotFlash => new(kind, Ensure.BetweenInclusive(1, ChargedShotGlow.ChargedShotFlashSteps, remainingSteps)),
        ChargedShotGlowKind.HyperBeamGlow => new(kind, Ensure.BetweenInclusive(0, ChargedShotGlow.HyperBeamGlowSteps, remainingSteps)),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, $"Undefined {nameof(ChargedShotGlowKind)}."),
    };
}
