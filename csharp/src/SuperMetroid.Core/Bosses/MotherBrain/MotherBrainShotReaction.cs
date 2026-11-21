using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How a shot that hits Mother Brain's brain in her second or third phase changes her walk credit (#2012), independent of
/// which object currently owns the credit.
/// </summary>
internal static class MotherBrainShotReaction
{
    /// <summary>
    /// The walk credit after a shot of <paramref name="family"/> hits her in <paramref name="form"/>, and whether it
    /// starts the Hyper Beam recoil. In her third phase a shot that reacts as a beam spends the Hyper Beam's cost while the
    /// credit covers it and otherwise empties the credit and starts the recoil, so sustained Hyper Beam fire first uses up
    /// the credit she has gathered. Every other shot, and a beam before her third phase, changes the credit as before it
    /// (<see cref="BeforeThirdPhase"/>).
    /// </summary>
    public static MotherBrainShotReactionResult Resolve(
        MotherBrainForm form,
        SamusProjectileFamily family,
        MotherBrainWalkCredit walkCredit)
    {
        if (!ReactsAsBeam(family) || form != MotherBrainForm.ThirdPhase)
            return new MotherBrainShotReactionResult(BeforeThirdPhase(family, walkCredit), HyperBeamRecoil: false);
        return walkCredit.CoversShot(MotherBrainWalkCredit.HyperBeamShotCost)
            ? new MotherBrainShotReactionResult(walkCredit.SpendForShot(MotherBrainWalkCredit.HyperBeamShotCost), HyperBeamRecoil: false)
            : new MotherBrainShotReactionResult(MotherBrainWalkCredit.Empty, HyperBeamRecoil: true);
    }

    /// <summary>
    /// The walk credit after a shot of <paramref name="family"/> hits her before her third phase, which starts no recoil: a
    /// missile or Super Missile empties the credit, and any other shot spends one step's credit, an overdraw leaving none.
    /// </summary>
    public static MotherBrainWalkCredit BeforeThirdPhase(SamusProjectileFamily family, MotherBrainWalkCredit walkCredit) =>
        family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile
            ? MotherBrainWalkCredit.Empty
            : walkCredit.SpendForShot(MotherBrainWalkCredit.StepCredit);

    /// <summary>Whether a shot of <paramref name="family"/> reacts as a beam does: a beam shot, or a missile's explosion.</summary>
    private static bool ReactsAsBeam(SamusProjectileFamily family) =>
        family is SamusProjectileFamily.Beam or SamusProjectileFamily.MissileExplosion;
}
