namespace SuperMetroid.Core.Samus;

/// <summary>
/// Hands Samus's movement from one owner to another by naming the new owner (<see cref="SamusMovementOwner"/>). The
/// shinespark, Crystal Flash, Draygon's hold and the drain keep their own state after another owner takes over, so the
/// owner that takes her movement, or the owner that gives it back to her pose, ends their hold on it: none of them may
/// resume once the newer owner lets go. Each owner ends exactly the holds it ends in the game
/// (<see cref="SamusMovementOwnerDefinitions"/>), which is not always every other hold: Draygon's hold leaves a
/// shinespark beneath it until her release ends it, and a knockback lets a stored shine stay stored.
/// </summary>
internal static partial class SamusMovementHandOver
{
    /// <summary>
    /// Gives Samus's movement to <paramref name="newOwner"/>, ending the holds it replaces
    /// (<see cref="SamusMovementOwnerDefinitions.HoldsEndedTaking"/>). The new owner then starts its own state: this ends
    /// only the holds of the owners it displaces.
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
        foreach (SamusMovementHoldEnd hold in Ensure.Listed(SamusMovementOwnerDefinitions.HoldsEndedTaking, newOwner))
            End(samus, hold);
    }

    /// <summary>
    /// Returns Samus's movement from <paramref name="endingOwner"/> to her pose, ending the holds that would otherwise
    /// resume beneath it (<see cref="SamusMovementOwnerDefinitions.HoldsEndedReturning"/>).
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
        foreach (SamusMovementHoldEnd hold in Ensure.Listed(SamusMovementOwnerDefinitions.HoldsEndedReturning, endingOwner))
            End(samus, hold);
    }

    /// <summary>Ends <paramref name="hold"/> on <paramref name="samus"/>'s movement.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The hold is undefined.</exception>
    private static void End(SamusState samus, SamusMovementHoldEnd hold)
    {
        switch (hold)
        {
            case SamusMovementHoldEnd.Shinespark:
                samus.Shinespark.RelinquishMovement();
                break;
            case SamusMovementHoldEnd.ShinesparkMovingHer:
                if (samus.Shinespark.OwnsMovement)
                    samus.Shinespark.RelinquishMovement();
                break;
            case SamusMovementHoldEnd.CrystalFlash:
                samus.CrystalFlash.RelinquishMovement();
                break;
            case SamusMovementHoldEnd.DraygonHold:
                samus.DraygonGrabbed.RelinquishMovement();
                break;
            case SamusMovementHoldEnd.Drain:
                samus.Drained.RelinquishMovement();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(hold), hold, $"Undefined {nameof(SamusMovementHoldEnd)}.");
        }
    }
}
