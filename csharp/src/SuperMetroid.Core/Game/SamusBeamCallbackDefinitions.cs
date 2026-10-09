namespace SuperMetroid.Core.Game;

/// <summary>
/// $90:B96E/BA3E: Wave selects terrain-passing motion; uncharged Wave/Ice-Wave
/// reload their trail after three frames, while charged or wider beams use four.
/// Includes the four bounded adjacent-code observations for each producer.
/// </summary>
internal static class SamusBeamCallbackDefinitions
{
    /// <summary>Number of addressable low-nibble combinations in each native table.</summary>
    public const int CombinationCount = 16;

    /// <summary>Resolves the complete native low-nibble domain for one beam producer.</summary>
    public static SamusBeamCallbackDefinition Resolve(bool charged, int combination)
    {
        if ((uint)combination >= CombinationCount)
            throw new ArgumentOutOfRangeException(nameof(combination));
        if (combination >= 12)
            return (charged, combination) switch
            {
                (false, 12) => Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationTwelveAdjacentWord),
                (false, 13) => Translated(SamusBeamPreInstructionCodes.ChainsawWindowStoreThenPowerBomb,
                    SamusProjectilePreInstruction.ChainsawWindowStoreThenPowerBomb),
                (false, 14) => Translated(SamusBeamPreInstructionCodes.SpacetimePaletteCopyTail,
                    SamusProjectilePreInstruction.SpacetimePaletteCopyTail),
                (false, 15) => Untranslated(SamusBeamPreInstructionCodes.UnchargedCombinationFifteenAdjacentWord),
                (true, 12) => Untranslated(SamusBeamPreInstructionCodes.ChargedCombinationTwelveAdjacentWord),
                (true, 13 or 14) => Translated(SamusBeamPreInstructionCodes.ChargedChainsawLowWramExecution,
                    SamusProjectilePreInstruction.ChargedChainsawLowWramExecution),
                _ => Translated(SamusBeamPreInstructionCodes.MurderBeamMisalignedExecution,
                    SamusProjectilePreInstruction.MurderBeamMisalignedExecution),
            };

        SamusBeamFlags beam = (SamusBeamFlags)combination;
        if ((beam & SamusBeamFlags.Wave) == 0)
            return Translated(SamusBeamPreInstructionCodes.NoWave, SamusProjectilePreInstruction.NoWaveBeam);
        if (!charged && (beam & (SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)) == 0)
            return Translated(SamusBeamPreInstructionCodes.WaveThreeFrameTrail,
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail);
        return Translated(SamusBeamPreInstructionCodes.WaveFourFrameTrail,
            SamusProjectilePreInstruction.WaveBeamFourFrameTrail);
    }

    /// <summary>Creates a callback result with both its native address and translated projectile behavior.</summary>
    /// <param name="nativePointer">The callback word's native address.</param>
    /// <param name="translated">The semantic projectile pre-instruction represented by that callback.</param>
    private static SamusBeamCallbackDefinition Translated(
        ushort nativePointer,
        SamusProjectilePreInstruction translated) =>
        new(nativePointer, translated);

    /// <summary>Creates a callback result when the native callback has no supported translated identity.</summary>
    /// <param name="nativePointer">The callback word's native address, retained for exact table matching.</param>
    private static SamusBeamCallbackDefinition Untranslated(ushort nativePointer) =>
        new(nativePointer, null);
}

/// <summary>One native callback word and its translated semantic identity, when supported.</summary>
/// <param name="NativePointer">The native callback-word address used by cartridge tables.</param>
/// <param name="Translated">The equivalent projectile pre-instruction, or <see langword="null"/> when the native word is not represented by a supported translated behavior.</param>
internal readonly record struct SamusBeamCallbackDefinition(
    ushort NativePointer,
    SamusProjectilePreInstruction? Translated);
