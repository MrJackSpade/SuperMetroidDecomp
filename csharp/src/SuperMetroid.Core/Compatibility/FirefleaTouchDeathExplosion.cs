namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which explosion a Fireflea dies with after Samus touches it (#1592). Every touch kills a Fireflea, even one the
/// touch left alive, and the explosion that death starts is not chosen by the Fireflea: one that survived the touch
/// explodes with the explosion its remaining health names in the explosions' authored order (small, Samus contact,
/// normal, Mini-Kraid's, big), so a Fireflea with one to four health left dies in the contact, normal, Mini-Kraid or
/// big explosion and one with more in the small one; one the touch already killed dies a second time in the small
/// explosion.
/// </summary>
/// <remarks>
/// Consulted once per touch, after the ordinary touch reaction, as the Fireflea's death starts. Evidence: the Fireflea's
/// touch reaction (<c>$A3:8E6B</c>) calls the enemy death routine straight after the common touch reaction, with the
/// accumulator still holding the health that reaction last loaded, or zero (or the respawn placeholder's header, past
/// every explosion) when the reaction already ran the death and cleared the enemy; the death routine takes zero through
/// four as its explosions in their authored order and any other number as the first.
/// </remarks>
[CompatibilityService("a Fireflea Samus touches dies in the explosion its remaining health names")]
public static class FirefleaTouchDeathExplosion
{
    /// <summary>
    /// The place, in the death explosions' authored order, of the explosion a Fireflea dies with when Samus's touch left
    /// it <paramref name="remainingHealth"/> health; a place past the last explosion selects the small one.
    /// </summary>
    /// <param name="remainingHealth">The Fireflea's health after the touch reaction.</param>
    public static int AuthoredExplosionAfterSurvivedTouch(int remainingHealth) => remainingHealth;

    /// <summary>
    /// The place, in the death explosions' authored order, of the explosion a Fireflea the touch already killed dies with
    /// a second time: the small explosion's.
    /// </summary>
    public const int AuthoredExplosionAfterKillingTouch = 0;
}
