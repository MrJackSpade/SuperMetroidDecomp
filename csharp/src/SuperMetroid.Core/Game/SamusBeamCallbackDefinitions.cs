namespace SuperMetroid.Core.Game;

/// <summary>Fixed low-nibble beam callback selections used by both firing routines.</summary>
internal static class SamusBeamCallbackDefinitions
{
    /// <summary>Number of addressable low-nibble combinations in each native table.</summary>
    public const int CombinationCount = 16;

    private static readonly SamusBeamCallbackDefinition[] UnchargedDefinitions =
    [
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveThreeFrameTrail, SamusProjectilePreInstruction.WaveBeamThreeFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveThreeFrameTrail, SamusProjectilePreInstruction.WaveBeamThreeFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationTwelveAdjacentWord),
        Translated(SamusBeamPreInstructionCodes.ChainsawWindowStoreThenPowerBomb,
            SamusProjectilePreInstruction.ChainsawWindowStoreThenPowerBomb),
        Translated(SamusBeamPreInstructionCodes.SpacetimePaletteCopyTail,
            SamusProjectilePreInstruction.SpacetimePaletteCopyTail),
        Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationFifteenAdjacentWord),
    ];

    private static readonly SamusBeamCallbackDefinition[] ChargedDefinitions =
    [
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam),
        Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail, SamusProjectilePreInstruction.WaveBeamFourFrameTrail),
        Untranslated(SamusBeamPreInstructionCodes.ChargedCombinationTwelveAdjacentWord),
        Translated(SamusBeamPreInstructionCodes.ChargedChainsawLowWramExecution,
            SamusProjectilePreInstruction.ChargedChainsawLowWramExecution),
        Translated(SamusBeamPreInstructionCodes.ChargedChainsawLowWramExecution,
            SamusProjectilePreInstruction.ChargedChainsawLowWramExecution),
        Translated(SamusBeamPreInstructionCodes.MurderBeamMisalignedExecution,
            SamusProjectilePreInstruction.MurderBeamMisalignedExecution),
    ];

    /// <summary><c>$90:B96E-$B98D</c>, including four adjacent-code observations.</summary>
    public static ReadOnlySpan<SamusBeamCallbackDefinition> Uncharged => UnchargedDefinitions;

    /// <summary><c>$90:BA3E-$BA5D</c>, including four adjacent-code observations.</summary>
    public static ReadOnlySpan<SamusBeamCallbackDefinition> Charged => ChargedDefinitions;

    /// <summary>Resolves the complete native low-nibble domain for one beam producer.</summary>
    public static SamusBeamCallbackDefinition Resolve(bool charged, int combination)
    {
        if ((uint)combination >= CombinationCount)
            throw new ArgumentOutOfRangeException(nameof(combination));
        return (charged ? ChargedDefinitions : UnchargedDefinitions)[combination];
    }

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
