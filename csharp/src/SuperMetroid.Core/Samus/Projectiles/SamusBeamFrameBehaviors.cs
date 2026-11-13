using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which frame behaviour each beam projectile runs.</summary>
internal static class SamusBeamFrameBehaviors
{
    /// <summary>Resolves every combination for one beam producer.</summary>
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
        // The four glitched combinations read past the twelve authored frame behaviours.
        SamusBeamCombination.SpazerPlasma => charged
            ? OutsideScope(SamusBeamFrameBehaviorChoice.ChargedCombinationTwelveOverrun)
            : OutsideScope(SamusBeamFrameBehaviorChoice.UnchargedCombinationTwelveOverrun),
        SamusBeamCombination.SpazerPlasmaWave => charged
            ? Runs(SamusBeamFrameBehaviorChoice.ChargedChainsawOverrun,
                SamusProjectileFrameBehavior.ChargedChainsawOverrun)
            : Runs(SamusBeamFrameBehaviorChoice.ChainsawPowerBomb,
                SamusProjectileFrameBehavior.ChainsawPowerBomb),
        SamusBeamCombination.SpazerPlasmaIce => charged
            ? Runs(SamusBeamFrameBehaviorChoice.ChargedChainsawOverrun,
                SamusProjectileFrameBehavior.ChargedChainsawOverrun)
            : Runs(SamusBeamFrameBehaviorChoice.SpaceTimeProgressionOverwrite,
                SamusProjectileFrameBehavior.SpaceTimeProgressionOverwrite),
        SamusBeamCombination.SpazerPlasmaIceWave => charged
            ? Runs(SamusBeamFrameBehaviorChoice.MurderBeamMisalignedExecution,
                SamusProjectileFrameBehavior.MurderBeamMisalignedExecution)
            : OutsideScope(SamusBeamFrameBehaviorChoice.UnchargedCombinationFifteenOverrun),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    /// <summary>Pairs a beam's choice with the projectile frame behaviour it runs.</summary>
    private static SamusBeamFrameBehaviorDefinition Runs(
        SamusBeamFrameBehaviorChoice choice,
        SamusProjectileFrameBehavior frameBehavior) =>
        new(choice, frameBehavior);

    /// <summary>
    /// A choice past the authored list that runs whatever the game's live state or an unconnected region holds: it has no
    /// frame behaviour, so firing it is rejected under the emulation scope limit (#1500).
    /// </summary>
    private static SamusBeamFrameBehaviorDefinition OutsideScope(SamusBeamFrameBehaviorChoice choice) =>
        new(choice, null);
}
