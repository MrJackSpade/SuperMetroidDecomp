using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which behaviour each beam shot runs.</summary>
internal static class SamusBeamShotBehaviors
{
    /// <summary>
    /// The behaviour a shot of <paramref name="combination"/> runs, charged when <paramref name="charged"/>. A Spazer and
    /// Plasma combination runs what follows the authored behaviours (<see cref="SamusBeamShotBehaviorDefinitions.Unauthored"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not one Samus's beams can make.</exception>
    public static SamusShotBehavior Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaIce =>
            SamusShotBehaviors.Beam,
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? SamusShotBehaviors.WaveBeamFourUpdateTrail
            : SamusShotBehaviors.WaveBeamThreeUpdateTrail,
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            SamusShotBehaviors.WaveBeamFourUpdateTrail,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            SamusBeamShotBehaviorDefinitions.Unauthored.ShotBehavior(combination, charged),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };
}
