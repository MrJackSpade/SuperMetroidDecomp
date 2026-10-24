using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// $90:B96E/BA3E: Wave selects terrain-passing motion; uncharged Wave/Ice-Wave
/// reload their trail after three frames, while charged or wider beams use four.
/// Includes the four bounded adjacent-code observations for each producer.
/// </summary>
internal static class SamusBeamCallbackDefinitions
{
    /// <summary>Resolves the complete native low-nibble domain for one beam producer.</summary>
    public static SamusBeamCallbackDefinition Resolve(bool charged, SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaIce =>
            Translated(SamusBeamCallback.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? Translated(SamusBeamCallback.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail)
            : Translated(SamusBeamCallback.WaveThreeFrameTrail, SamusProjectilePreInstruction.WaveBeamThreeFrameTrail),
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            Translated(SamusBeamCallback.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        // The four glitched combinations read past the twelve authored callbacks.
        SamusBeamCombination.SpazerPlasma => charged
            ? Untranslated(SamusBeamCallback.ChargedCombinationTwelveAdjacentWord)
            : Untranslated(SamusBeamCallback.UnchargedCombinationTwelveAdjacentWord),
        SamusBeamCombination.SpazerPlasmaWave => charged
            ? Translated(SamusBeamCallback.ChargedChainsawLowWramExecution,
                SamusProjectilePreInstruction.ChargedChainsawLowWramExecution)
            : Translated(SamusBeamCallback.ChainsawPowerBomb,
                SamusProjectilePreInstruction.ChainsawPowerBomb),
        SamusBeamCombination.SpazerPlasmaIce => charged
            ? Translated(SamusBeamCallback.ChargedChainsawLowWramExecution,
                SamusProjectilePreInstruction.ChargedChainsawLowWramExecution)
            : Translated(SamusBeamCallback.SpacetimePaletteCopyTail,
                SamusProjectilePreInstruction.SpacetimePaletteCopyTail),
        SamusBeamCombination.SpazerPlasmaIceWave => charged
            ? Translated(SamusBeamCallback.MurderBeamMisalignedExecution,
                SamusProjectilePreInstruction.MurderBeamMisalignedExecution)
            : Untranslated(SamusBeamCallback.UnchargedCombinationFifteenAdjacentWord),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    /// <summary>Pairs a callback with an implemented projectile pre-instruction.</summary>
    private static SamusBeamCallbackDefinition Translated(
        SamusBeamCallback callback,
        SamusProjectilePreInstruction translated) =>
        new(callback, translated);

    /// <summary>Keeps an adjacent-code callback visible without assigning unsupported behavior.</summary>
    private static SamusBeamCallbackDefinition Untranslated(SamusBeamCallback callback) =>
        new(callback, null);
}
