namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// X-ray refuses to start while Samus reads as the Baby Metroid's drain holds her (#1692): on an update her shared
/// projectile cooldown stands at seven, five bombs are counted and her horizontal speed is divided by two. The drain
/// writes a cooldown of eight, five bombs and the halved speed on every update it drains her; the test reads only those
/// values, so it also refuses a Samus no Baby drains whose values match: one slowed by two of Draygon's goops, with five
/// bombs counted (a bomb spread counts five at once) and a cooldown counting down through seven. Every other admission
/// test of X-ray is ordinary gameplay and stays with X-ray.
/// </summary>
/// <remarks>
/// <para>
/// Consulted once per X-ray start, before every other admission test (the landing poses, the game's state, a power bomb's
/// explosion, Samus's vertical speed and her movement). A stateless rule.
/// </para>
/// <para>
/// Evidence: the X-ray setup ($91:E16D) refuses when the projectile cooldown equals 7, the bomb counter equals 5 and the
/// speed divisor equals 2, ahead of its other tests; the Baby Metroid's drain ($A9:F21B) stores a cooldown of 8, a bomb
/// counter of 5 and a speed divisor of 2 on every update it drains Samus; each Draygon goop that sticks to Samus raises
/// the speed divisor by one, to at most five ($86:8D99); the bomb spread ($90:D849) sets the bomb counter to 5.
/// </para>
/// </remarks>
[CompatibilityService("X-ray refuses to start on any update Samus's cooldown stands at seven with five bombs counted and her speed halved, the values the Baby Metroid's drain holds her at")]
public static class XrayRefusedAsDrained
{
    /// <summary>The projectile cooldown, in updates, at which X-ray refuses.</summary>
    private const int DrainedCooldownUpdates = 7;

    /// <summary>The bombs the drain counts.</summary>
    private const int DrainedBombs = 5;

    /// <summary>What the drain divides Samus's horizontal speed by.</summary>
    private const int DrainedSpeedDivisor = 2;

    /// <summary>Whether X-ray refuses to start because Samus reads as drained.</summary>
    /// <param name="projectileCooldown">Samus's shared projectile cooldown.</param>
    /// <param name="bombs">The bombs counted as Samus's.</param>
    /// <param name="speedDivisor">What Samus's horizontal speed is divided by.</param>
    public static bool Refuses(WrappingCountdown projectileCooldown, int bombs, int speedDivisor) =>
        projectileCooldown.IsAt(DrainedCooldownUpdates) && bombs == DrainedBombs && speedDivisor == DrainedSpeedDivisor;
}
