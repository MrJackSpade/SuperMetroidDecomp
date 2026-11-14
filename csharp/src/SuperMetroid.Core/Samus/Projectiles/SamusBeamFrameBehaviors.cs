using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which frame behaviour each beam projectile runs.</summary>
internal static class SamusBeamFrameBehaviors
{
    /// <summary>
    /// The frame behaviour a shot of <paramref name="combination"/> runs, charged when <paramref name="charged"/>. A
    /// Spazer and Plasma combination runs what follows the authored frame behaviours
    /// (<see cref="SamusBeamFrameBehaviorDefinitions.Unauthored"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not one Samus's beams can make.</exception>
    public static SamusBeamFrameBehaviorDefinition Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaIce =>
            Runs(SamusBeamFrameBehaviorChoice.NoWave, SamusProjectileFrameBehavior.NoWaveBeam),
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? Runs(SamusBeamFrameBehaviorChoice.WaveFourFrameTrail, SamusProjectileFrameBehavior.WaveBeamFourFrameTrail)
            : Runs(SamusBeamFrameBehaviorChoice.WaveThreeFrameTrail, SamusProjectileFrameBehavior.WaveBeamThreeFrameTrail),
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            Runs(SamusBeamFrameBehaviorChoice.WaveFourFrameTrail, SamusProjectileFrameBehavior.WaveBeamFourFrameTrail),
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            SamusBeamFrameBehaviorDefinitions.Unauthored.FrameBehavior(combination, charged),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    /// <summary>Pairs a beam's choice with the projectile frame behaviour it runs.</summary>
    private static SamusBeamFrameBehaviorDefinition Runs(
        SamusBeamFrameBehaviorChoice choice,
        SamusProjectileFrameBehavior frameBehavior) =>
        new(choice, frameBehavior);
}
