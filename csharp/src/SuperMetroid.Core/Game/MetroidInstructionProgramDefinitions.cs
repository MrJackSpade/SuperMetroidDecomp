namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled animation timing, sound callbacks, and loop control for ordinary Metroids.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class MetroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_Metroid_ChasingSamus</c> at $A3:E9CF.</summary>
    internal const ushort ChasingSamus = 0xe9cf;
    /// <summary><c>InstList_Metroid_DrainingSamus</c> at $A3:EA25.</summary>
    internal const ushort DrainingSamus = 0xea25;

    /// <summary>$A3:E9CF..E9DF/EA25..EA35: one pulse comprises four16-tick beats; exact chosen beat is retained as reviewed drawn-pulse/synchronized-cry choreography.</summary>
    private const ushort PulseBeatTicks = 16;
    /// <summary>$A3:E9D7/EA2D: six ticks brighten the pulse; the peak uses the remainder of that beat. Exact chosen split is retained under the same bounded pulse-exposure disposition; damage and other sounds/artwork are excluded.</summary>
    private const ushort PulseBrighteningTicks = 6;
    private enum PulseStage
    {
        /// <summary>$A3:E9CF/EA25 selects Insides_0 atF10D.</summary>
        ContractedRest,
        /// <summary>$A3:E9D3/EA29 selects Insides_1 atF137.</summary>
        ExpandedRest,
        /// <summary>$A3:E9D7/EA2D selects Insides_2 atF157.</summary>
        Brightening,
        /// <summary>$A3:E9DB/EA31 selects Insides_3 atF181.</summary>
        Peak,
        /// <summary>$A3:E9DF/EA35 returns to Insides_1 atF137.</summary>
        ReturnToExpanded,
    }

    private static ushort FrameDuration(int stage) => (PulseStage)stage switch
    {
        PulseStage.Brightening => PulseBrighteningTicks,
        PulseStage.Peak => PulseBeatTicks - PulseBrighteningTicks,
        PulseStage.ContractedRest or PulseStage.ExpandedRest or PulseStage.ReturnToExpanded => PulseBeatTicks,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - ChasingSamus;
        if ((uint)offset < 80) return offset % 4 == 2;
        offset = address - DrainingSamus;
        return (uint)offset < 20 && offset % 4 == 2;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        bool chasing = address < DrainingSamus;
        int start = chasing ? ChasingSamus : DrainingSamus;
        int offset = address - start;
        int frames = chasing ? 20 : 5;
        if ((uint)offset < frames * 4 && offset % 4 == 0) return FrameDuration(offset / 4 % 5);
        if (offset == frames * 4) return chasing ? EnemyInstructionCodePointers.Instruction_Metroid_PlayRandomMetroidSFX :
            EnemyInstructionCodePointers.Instruction_Metroid_PlayDrainingSamusSFX;
        if (offset == frames * 4 + 2) return CommonEnemyInstructionCodes.Goto;
        if (offset == frames * 4 + 4) return (ushort)start;
        throw new InvalidDataException($"Metroid instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
