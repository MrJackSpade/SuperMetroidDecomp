using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which behaviour each beam shot runs.</summary>
internal static class SamusBeamShotBehaviors
{
    /// <summary>
    /// What firing a shot of <paramref name="combination"/> does, charged when <paramref name="charged"/>: the behaviour it
    /// runs, or, for a Spazer and Plasma combination, what follows the authored behaviours
    /// (<see cref="SamusBeamShotBehaviorDefinitions.Unauthored"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not one Samus's beams can make.</exception>
    public static SamusShotFiring Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaIce =>
            SamusShotBehaviors.Beam,
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? SamusShotBehaviors.WaveBeamTrail
            : SamusShotBehaviors.UnchargedWaveBeamTrail,
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            SamusShotBehaviors.WaveBeamTrail,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            SamusBeamShotBehaviorDefinitions.Unauthored.ShotBehavior(combination, charged),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };
}
