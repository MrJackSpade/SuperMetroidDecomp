using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which callback each beam projectile runs.</summary>
internal static class SamusBeamCallbacks
{
    /// <summary>Resolves every combination for one beam producer.</summary>
    public static SamusBeamCallbackDefinition Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaIce =>
            Translated(SamusBeamCallback.NoWave, SamusProjectileFrameBehavior.NoWaveBeam),
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? Translated(SamusBeamCallback.WaveFourFrameTrail, SamusProjectileFrameBehavior.WaveBeamFourFrameTrail)
            : Translated(SamusBeamCallback.WaveThreeFrameTrail, SamusProjectileFrameBehavior.WaveBeamThreeFrameTrail),
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            Translated(SamusBeamCallback.WaveFourFrameTrail, SamusProjectileFrameBehavior.WaveBeamFourFrameTrail),
        // The four glitched combinations read past the twelve authored callbacks.
        SamusBeamCombination.SpazerPlasma => charged
            ? Untranslated(SamusBeamCallback.ChargedCombinationTwelveOverrun)
            : Untranslated(SamusBeamCallback.UnchargedCombinationTwelveOverrun),
        SamusBeamCombination.SpazerPlasmaWave => charged
            ? Translated(SamusBeamCallback.ChargedChainsawOverrun,
                SamusProjectileFrameBehavior.ChargedChainsawOverrun)
            : Translated(SamusBeamCallback.ChainsawPowerBomb,
                SamusProjectileFrameBehavior.ChainsawPowerBomb),
        SamusBeamCombination.SpazerPlasmaIce => charged
            ? Translated(SamusBeamCallback.ChargedChainsawOverrun,
                SamusProjectileFrameBehavior.ChargedChainsawOverrun)
            : Translated(SamusBeamCallback.SpacetimePaletteCopyTail,
                SamusProjectileFrameBehavior.SpacetimePaletteCopyTail),
        SamusBeamCombination.SpazerPlasmaIceWave => charged
            ? Translated(SamusBeamCallback.MurderBeamMisalignedExecution,
                SamusProjectileFrameBehavior.MurderBeamMisalignedExecution)
            : Untranslated(SamusBeamCallback.UnchargedCombinationFifteenOverrun),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    /// <summary>Pairs a callback with an implemented projectile frame behaviour.</summary>
    private static SamusBeamCallbackDefinition Translated(
        SamusBeamCallback callback,
        SamusProjectileFrameBehavior translated) =>
        new(callback, translated);

    /// <summary>Keeps an adjacent-code callback visible without assigning unsupported behavior.</summary>
    private static SamusBeamCallbackDefinition Untranslated(SamusBeamCallback callback) =>
        new(callback, null);
}
