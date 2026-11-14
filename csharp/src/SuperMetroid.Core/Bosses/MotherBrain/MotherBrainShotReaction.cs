namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Runs the walk-counter arithmetic of Mother Brain's phase-two/three shot reaction
/// Independent of which object currently owns the counter.
/// </summary>
internal static class MotherBrainShotReaction
{
    /// <summary>Updates phase-two/three walk credit after a shot and signals recoil only when a form-four Hyper Beam shot takes the credit below zero.</summary>
    public static MotherBrainShotReactionResult Resolve(
        MotherBrainForm form,
        MotherBrainProjectileType projectileType,
        MotherBrainWalkCredit walkCredit)
    {
        // The shot reaction knows only the projectile types it reacts to. Validate the
        // projectile type so a caller cannot smuggle a larger value past it.
        if (!Enum.IsDefined(projectileType))
            throw new ArgumentOutOfRangeException(nameof(projectileType));

        // The table returns two for beams, one for missiles/supers, and zero for every
        // remaining projectile class. Form four gives only reaction type two the special
        // Hyper Beam path; ordinary phase-two beams continue through the generic branch.
        int reactionType = projectileType switch
        {
            MotherBrainProjectileType.Beam => 2,
            MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile => 1,
            MotherBrainProjectileType.PowerBomb or MotherBrainProjectileType.Bomb or
                MotherBrainProjectileType.BeamExplosion => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(projectileType), projectileType, "Undefined projectile class."),
        };
        if (form == MotherBrainForm.ThirdPhase && reactionType == 2)
        {
            // The below-zero check keeps the nonnegative remainder without recoil, which is why
            // sustained Hyper Beam fire first consumes accumulated walk credit. Below zero
            // the counter is replaced by zero, not the wrapped subtraction.
            return walkCredit.CoversShot(MotherBrainWalkCredit.HyperBeamShotCost)
                ? new MotherBrainShotReactionResult(
                    walkCredit.SpendForShot(MotherBrainWalkCredit.HyperBeamShotCost), HyperBeamRecoil: false)
                : new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: true);
        }

        // The decrement turns reaction one into zero, sending either missile kind directly to the
        // no-reaction case; only that zero is tested. Reaction zero wraps to -1 and reaction
        // two outside form four leaves one: both nonzero cases subtract 256 and clamp
        // a result below zero to zero.
        if (reactionType == 1)
            return new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: false);

        return new MotherBrainShotReactionResult(
            walkCredit.SpendForShot(MotherBrainWalkCredit.StepCredit),
            HyperBeamRecoil: false);
    }
}
