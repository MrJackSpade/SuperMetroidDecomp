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
    /// gathered. A missile or Super Missile empties the credit. Any other shot, and a beam before her third phase, spends
    /// one step's credit, an overdraw leaving none.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="projectileType"/> is not a shot kind.</exception>
    public static MotherBrainShotReactionResult Resolve(
        MotherBrainForm form,
        MotherBrainProjectileType projectileType,
        MotherBrainWalkCredit walkCredit) => projectileType switch
    {
        MotherBrainProjectileType.Beam when form == MotherBrainForm.ThirdPhase =>
            walkCredit.CoversShot(MotherBrainWalkCredit.HyperBeamShotCost)
                ? new MotherBrainShotReactionResult(walkCredit.SpendForShot(MotherBrainWalkCredit.HyperBeamShotCost), HyperBeamRecoil: false)
                : new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: true),
        MotherBrainProjectileType.Missile or MotherBrainProjectileType.SuperMissile =>
            new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: false),
        MotherBrainProjectileType.Beam or MotherBrainProjectileType.PowerBomb or MotherBrainProjectileType.Bomb
            or MotherBrainProjectileType.BeamExplosion =>
            new MotherBrainShotReactionResult(walkCredit.SpendForShot(MotherBrainWalkCredit.StepCredit), HyperBeamRecoil: false),
        _ => throw new ArgumentOutOfRangeException(nameof(projectileType), projectileType, "Undefined shot kind."),
    };
}
