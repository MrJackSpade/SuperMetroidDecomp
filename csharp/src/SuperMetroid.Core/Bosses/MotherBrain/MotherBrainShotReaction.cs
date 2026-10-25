namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Ports the walk-counter arithmetic of Mother Brain's phase-two/three shot reaction
/// <c>$A9:B562-$B5C4</c>, independent of which object currently owns the counter.
/// </summary>
internal static class MotherBrainShotReaction
{
    /// <summary>The eight projectile classes of the reaction table, in the order the masked type word counts them.</summary>
    internal static readonly IdentitySequence<MotherBrainProjectileType> ReactionClasses = new(
        MotherBrainProjectileType.Beam, MotherBrainProjectileType.Missile, MotherBrainProjectileType.SuperMissile,
        MotherBrainProjectileType.PowerBomb, MotherBrainProjectileType.ClassFour, MotherBrainProjectileType.Bomb,
        MotherBrainProjectileType.ClassSix, MotherBrainProjectileType.BeamExplosion);

    /// <summary>Updates phase-two/three walk credit after a shot and signals recoil only on form-four Hyper Beam underflow.</summary>
    public static MotherBrainShotReactionResult Resolve(
        MotherBrainForm form,
        MotherBrainProjectileType projectileType,
        MotherBrainWalkCredit walkCredit)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot smuggle a larger value past it.
        if (!Enum.IsDefined(projectileType))
            throw new ArgumentOutOfRangeException(nameof(projectileType));

        // The table returns two for beams, one for missiles/supers, and zero for every
        // remaining projectile class. Form four gives only reaction type two the special
        // Hyper Beam path; ordinary phase-two beams continue through the generic branch.
        int reactionType = projectileType switch
        {
            MotherBrainProjectileType.Beam => 2,
            MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile => 1,
            MotherBrainProjectileType.PowerBomb or MotherBrainProjectileType.ClassFour or
                MotherBrainProjectileType.Bomb or MotherBrainProjectileType.ClassSix or
                MotherBrainProjectileType.BeamExplosion => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(projectileType), projectileType, "Undefined projectile class."),
        };
        if (form == MotherBrainForm.ThirdPhase && reactionType == 2)
        {
            // `$B5A9`: BPL keeps the nonnegative remainder without recoil, which is why
            // sustained Hyper Beam fire first consumes accumulated walk credit. On underflow
            // the counter is replaced by zero, not the wrapped subtraction.
            return walkCredit.CoversShot(MotherBrainWalkCredit.HyperBeamShotCost)
                ? new MotherBrainShotReactionResult(
                    walkCredit.SpendForShot(MotherBrainWalkCredit.HyperBeamShotCost), HyperBeamRecoil: false)
                : new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: true);
        }

        // DEC turns reaction one into zero, sending either missile kind directly to the
        // zero label; only that zero is tested. Reaction zero wraps to `$FFFF` and reaction
        // two outside form four leaves one: both nonzero cases subtract `$0100` and clamp
        // signed underflow to zero.
        if (reactionType == 1)
            return new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: false);

        return new MotherBrainShotReactionResult(
            walkCredit.SpendForShot(MotherBrainWalkCredit.StepCredit),
            HyperBeamRecoil: false);
    }
}
