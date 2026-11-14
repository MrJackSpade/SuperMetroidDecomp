namespace SuperMetroid.Core.Samus;

/// <summary>
/// Hands Samus's movement from one owner to another by naming the new owner (<see cref="SamusMovementOwner"/>). The
/// shinespark, Crystal Flash, Draygon's hold and the drain keep their own state after another owner takes over, so the
/// owner that takes her movement, or the owner that gives it back to her pose, ends their hold on it: none of them may
/// resume once the newer owner lets go. Each owner ends exactly the holds it ends in the game, which is not always every
/// other hold: Draygon's hold leaves a shinespark beneath it until her release ends it, and a knockback lets a stored
/// shine stay stored.
/// </summary>
internal static class SamusMovementHandOver
{
    /// <summary>
    /// Gives Samus's movement to <paramref name="newOwner"/>, ending the holds it replaces. The new owner then starts its
    /// own state: this ends only the holds of the owners it displaces.
    /// </summary>
    /// <param name="samus">The Samus whose movement changes hands.</param>
    /// <param name="newOwner">
    /// The owner taking her movement: the shinespark's windup, Crystal Flash, Draygon's hold, Ceres Ridley's shove, a bomb
    /// jump, the drained fall, the X-ray scope or an elevator ride.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The owner takes her movement without ending another's hold.</exception>
    internal static void To(SamusState samus, SamusMovementOwner newOwner)
    {
        Ensure.NotNull(samus);
        switch (newOwner)
        {
            case SamusMovementOwner.Shinespark:
            case SamusMovementOwner.DraygonHold:
                // The windup and Draygon's grab take her from Crystal Flash. A shinespark Draygon grabs keeps flying
                // beneath the hold until the release ends it.
                samus.CrystalFlash.RelinquishMovement();
                break;

            case SamusMovementOwner.CrystalFlash:
                // Crystal Flash takes her from every other hold that keeps its state: a shinespark, Draygon's hold, whose
                // escape check still counts presses, and the drain, which would lock her again after the bubble.
                samus.Shinespark.RelinquishMovement();
                samus.DraygonGrabbed.RelinquishMovement();
                samus.Drained.RelinquishMovement();
                break;

            case SamusMovementOwner.CeresRidleyEjection:
            case SamusMovementOwner.BombJump:
            case SamusMovementOwner.Drained:
            case SamusMovementOwner.Xray:
            case SamusMovementOwner.Elevator:
                // These take her from a shinespark and from Crystal Flash. An uncrashed spark keeps its boost as the Blue
                // Suit, and Crystal Flash keeps its suit colouring, but neither moves her again.
                samus.Shinespark.RelinquishMovement();
                samus.CrystalFlash.RelinquishMovement();
                break;

            // Her pose, the grapple and a knockback take her movement by their own state, ending no other hold, and
            // nothing is no owner at all.
            case SamusMovementOwner.None:
            case SamusMovementOwner.Pose:
            case SamusMovementOwner.Grapple:
            case SamusMovementOwner.Knockback:
            default:
                throw new ArgumentOutOfRangeException(nameof(newOwner), newOwner,
                    "This owner takes Samus's movement without ending another owner's hold.");
        }
    }

    /// <summary>
    /// Returns Samus's movement from <paramref name="endingOwner"/> to her pose, ending the holds that would otherwise
    /// resume beneath it.
    /// </summary>
    /// <param name="samus">The Samus whose movement returns to her pose.</param>
    /// <param name="endingOwner">
    /// The owner letting go: Draygon's hold at her release, a knockback at its end, the shinespark at the end of its crash,
    /// or the X-ray scope as it closes.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The owner returns her movement without ending another's hold.</exception>
    internal static void BackToPose(SamusState samus, SamusMovementOwner endingOwner)
    {
        Ensure.NotNull(samus);
        switch (endingOwner)
        {
            case SamusMovementOwner.DraygonHold:
                // The release (and death, which runs it even when Samus was never held) ends a spark or Crystal Flash
                // that the hold kept beneath it, without resetting the spark's boost or colouring.
                samus.Shinespark.RelinquishMovement();
                samus.CrystalFlash.RelinquishMovement();
                break;

            case SamusMovementOwner.Knockback:
                // A shinespark windup can begin after the knockback's end was decided; the end takes her from it but
                // leaves a stored shine stored, with its colouring and timer.
                if (samus.Shinespark.OwnsMovement)
                    samus.Shinespark.RelinquishMovement();
                break;

            case SamusMovementOwner.Shinespark:
                // The end of the crash frees a Samus drained mid-crash too.
                samus.Drained.RelinquishMovement();
                break;

            case SamusMovementOwner.Xray:
                // X-Mode can leave an active spark holding her as the scope closes; the closing ends it and the spark's
                // colouring timer survives, which is the Blue Suit.
                samus.Shinespark.RelinquishMovement();
                break;

            // The others give her movement back by their own state alone, ending no other hold.
            case SamusMovementOwner.None:
            case SamusMovementOwner.Pose:
            case SamusMovementOwner.Grapple:
            case SamusMovementOwner.CeresRidleyEjection:
            case SamusMovementOwner.BombJump:
            case SamusMovementOwner.CrystalFlash:
            case SamusMovementOwner.Drained:
            case SamusMovementOwner.Elevator:
            default:
                throw new ArgumentOutOfRangeException(nameof(endingOwner), endingOwner,
                    "This owner returns Samus's movement without ending another owner's hold.");
        }
    }
}
