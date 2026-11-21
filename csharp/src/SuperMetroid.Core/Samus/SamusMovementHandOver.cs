namespace SuperMetroid.Core.Samus;

/// <summary>
/// Hands Samus's movement from one owner to another (<see cref="SamusMovementOwner"/>), each owner that ends another's
/// hold through its own operation (#2553). The shinespark, Crystal Flash, Draygon's hold and the drain keep their own state
/// after another owner takes over, so the owner that takes her movement, or the owner that gives it back to her pose, ends
/// their hold on it: none of them may resume once the newer owner lets go. Each owner ends exactly the holds it ends in
/// the game, which is not always every other hold: Draygon's hold leaves a shinespark beneath it until her release ends
/// it, and a knockback lets a stored shine stay stored. Her pose, the grapple and a knockback take her movement by their
/// own state, ending no other hold, so they have no operation here.
/// </summary>
internal static partial class SamusMovementHandOver
{
    /// <summary>
    /// The shinespark's windup takes Samus's movement from Crystal Flash. The new owner then starts its own state: this
    /// ends only the holds of the owners it displaces.
    /// </summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToShinespark(SamusState samus) => Ensure.NotNull(samus).CrystalFlash.RelinquishMovement();

    /// <summary>
    /// Draygon's grab takes Samus's movement from Crystal Flash. A shinespark Draygon grabs keeps flying beneath the hold
    /// until the release ends it.
    /// </summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToDraygonHold(SamusState samus) => Ensure.NotNull(samus).CrystalFlash.RelinquishMovement();

    /// <summary>
    /// Crystal Flash takes Samus's movement from every other hold that keeps its state: a shinespark, Draygon's hold, whose
    /// escape check still counts presses, and the drain, which would lock her again after the bubble.
    /// </summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToCrystalFlash(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Shinespark.RelinquishMovement();
        samus.DraygonGrabbed.RelinquishMovement();
        samus.Drained.RelinquishMovement();
    }

    /// <summary>Ceres Ridley's shove takes Samus's movement from a shinespark and from Crystal Flash.</summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToCeresRidleyEjection(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>A bomb jump takes Samus's movement from a shinespark and from Crystal Flash.</summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToBombJump(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>The drained fall takes Samus's movement from a shinespark and from Crystal Flash.</summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToDrained(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>The X-ray scope takes Samus's movement from a shinespark and from Crystal Flash.</summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToXray(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>An elevator ride takes Samus's movement from a shinespark and from Crystal Flash.</summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    internal static void ToElevator(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>
    /// Draygon's release (and death, which runs it even when Samus was never held) returns Samus's movement to her pose,
    /// ending a spark or Crystal Flash that the hold kept beneath it, without resetting the spark's boost or colouring.
    /// </summary>
    /// <param name="samus">The Samus whose movement returns to her pose.</param>
    internal static void FromDraygonHold(SamusState samus) => EndSparkAndCrystalFlash(samus);

    /// <summary>
    /// A knockback's end returns Samus's movement to her pose. A shinespark windup can begin after the knockback's end was
    /// decided; the end takes her from it but leaves a stored shine stored, with its colouring and timer.
    /// </summary>
    /// <param name="samus">The Samus whose movement returns to her pose.</param>
    internal static void FromKnockback(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (samus.Shinespark.OwnsMovement)
            samus.Shinespark.RelinquishMovement();
    }

    /// <summary>The end of the shinespark's crash returns Samus's movement to her pose, freeing a Samus drained mid-crash too.</summary>
    /// <param name="samus">The Samus whose movement returns to her pose.</param>
    internal static void FromShinespark(SamusState samus) => Ensure.NotNull(samus).Drained.RelinquishMovement();

    /// <summary>
    /// The X-ray scope's closing returns Samus's movement to her pose. X-Mode can leave an active spark holding her as the
    /// scope closes; the closing ends it and the spark's colouring timer survives, which is the Blue Suit.
    /// </summary>
    /// <param name="samus">The Samus whose movement returns to her pose.</param>
    internal static void FromXray(SamusState samus) => Ensure.NotNull(samus).Shinespark.RelinquishMovement();

    /// <summary>
    /// Ends a shinespark's and Crystal Flash's hold on Samus's movement. An uncrashed spark keeps its boost as the Blue
    /// Suit, and Crystal Flash keeps its suit colouring, but neither moves her again.
    /// </summary>
    private static void EndSparkAndCrystalFlash(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Shinespark.RelinquishMovement();
        samus.CrystalFlash.RelinquishMovement();
    }
}
