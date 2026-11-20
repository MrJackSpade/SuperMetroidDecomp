namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How a shot that hits Mother Brain's brain in her second or third phase changes her walk credit (#2012), independent of
/// which object currently owns the credit.
/// </summary>
internal static class MotherBrainShotReaction
{
    /// <summary>
    /// The walk credit after a shot of <paramref name="projectileType"/> hits her in <paramref name="form"/>, and whether it
    /// starts the Hyper Beam recoil. In her third phase a beam spends the Hyper Beam's cost while the credit covers it and
    /// otherwise empties the credit and starts the recoil, so sustained Hyper Beam fire first uses up the credit she has
    /// gathered. Every other shot, and a beam before her third phase, changes the credit as before it
    /// (<see cref="BeforeThirdPhase"/>).
    /// </summary>
    public static MotherBrainShotReactionResult Resolve(
        MotherBrainForm form,
        MotherBrainProjectileType projectileType,
        MotherBrainWalkCredit walkCredit)
    {
        if (projectileType != MotherBrainProjectileType.Beam || form != MotherBrainForm.ThirdPhase)
            return new MotherBrainShotReactionResult(BeforeThirdPhase(projectileType, walkCredit), HyperBeamRecoil: false);
        return walkCredit.CoversShot(MotherBrainWalkCredit.HyperBeamShotCost)
            ? new MotherBrainShotReactionResult(walkCredit.SpendForShot(MotherBrainWalkCredit.HyperBeamShotCost), HyperBeamRecoil: false)
            : new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: true);
    }

    /// <summary>
    /// The walk credit after a shot of <paramref name="projectileType"/> hits her before her third phase, which starts no
    /// recoil: a missile or Super Missile empties the credit, and any other shot spends one step's credit, an overdraw
    /// leaving none.
    /// </summary>
    public static MotherBrainWalkCredit BeforeThirdPhase(MotherBrainProjectileType projectileType, MotherBrainWalkCredit walkCredit) =>
        projectileType is MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile
            ? MotherBrainWalkCredit.Empty
            : walkCredit.SpendForShot(MotherBrainWalkCredit.StepCredit);
}
