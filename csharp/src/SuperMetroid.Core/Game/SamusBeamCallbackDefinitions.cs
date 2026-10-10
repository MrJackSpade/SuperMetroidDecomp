namespace SuperMetroid.Core.Game;

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
            Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        SamusBeamCombination.Wave or SamusBeamCombination.IceWave => charged
            ? Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail)
            : Translated(SamusBeamPreInstructionCodes.WaveThreeFrameTrail, SamusProjectilePreInstruction.WaveBeamThreeFrameTrail),
        SamusBeamCombination.SpazerWave or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIceWave =>
            Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        // The four glitched combinations read past the twelve authored callbacks.
        SamusBeamCombination.SpazerPlasma => charged
            ? Untranslated(SamusBeamPreInstructionCodes.ChargedCombinationTwelveAdjacentWord)
            : Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationTwelveAdjacentWord),
        SamusBeamCombination.SpazerPlasmaWave => charged
            ? Translated(SamusBeamPreInstructionCodes.ChargedChainsawLowWramExecution,
                SamusProjectilePreInstruction.ChargedChainsawLowWramExecution)
            : Translated(SamusBeamPreInstructionCodes.ChainsawWindowStoreThenPowerBomb,
                SamusProjectilePreInstruction.ChainsawWindowStoreThenPowerBomb),
        SamusBeamCombination.SpazerPlasmaIce => charged
            ? Translated(SamusBeamPreInstructionCodes.ChargedChainsawLowWramExecution,
                SamusProjectilePreInstruction.ChargedChainsawLowWramExecution)
            : Translated(SamusBeamPreInstructionCodes.SpacetimePaletteCopyTail,
                SamusProjectilePreInstruction.SpacetimePaletteCopyTail),
        SamusBeamCombination.SpazerPlasmaIceWave => charged
            ? Translated(SamusBeamPreInstructionCodes.MurderBeamMisalignedExecution,
                SamusProjectilePreInstruction.MurderBeamMisalignedExecution)
            : Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationFifteenAdjacentWord),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    private static SamusBeamCallbackDefinition Translated(
        ushort nativePointer,
        SamusProjectilePreInstruction translated) =>
        new(nativePointer, translated);

    private static SamusBeamCallbackDefinition Untranslated(ushort nativePointer) =>
        new(nativePointer, null);
}

/// <summary>One native callback word and its translated semantic identity, when supported.</summary>
internal readonly record struct SamusBeamCallbackDefinition(
    ushort NativePointer,
    SamusProjectilePreInstruction? Translated);
