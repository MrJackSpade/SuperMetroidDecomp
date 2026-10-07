namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Tourian Shitroid's finish-draining,
/// normal, latched, and remorse programs. Their thirty spritemap selections resolve
/// compiled identities to installed artwork.
/// </summary>
internal abstract class ShitroidInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_BabyMetroid_FinishDraining</c> at $A9:F906.</summary>
    internal const ushort FinishDraining = 0xf906;

    /// <summary><c>InstList_BabyMetroid_Normal</c> at $A9:F90E.</summary>
    internal const ushort Normal = 0xf90e;

    /// <summary><c>InstList_BabyMetroid_LatchedOn</c> at $A9:F924.</summary>
    internal const ushort LatchedOn = 0xf924;

    /// <summary><c>InstList_BabyMetroid_Remorse</c> at $A9:F93A.</summary>
    internal const ushort Remorse = 0xf93a;

    /// <summary>The random remorse branch callback word at $A9:F95A.</summary>
    internal const ushort RemorseRandomBranchOpcode = 0xf95a;

    /// <summary>The final unconditional remorse-loop callback word at $A9:F98E.</summary>
    internal const ushort RemorseLoopOpcode = 0xf98e;

    /// <summary>The first callback implementation after the programs, at $A9:F990.</summary>
    internal const ushort FirstAdjacentCallbackCode = 0xf990;

    /// <summary>$A9:F906 holds pose2 for128 ticks before returning through pose1 to the normal loop.
    /// Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort FinishDrainHold = 128;
    /// <summary>$A9:F924/F928/F92C/F930 latched pose0/1/2/1 durations
    /// The native uses the same poses as the normal loop with its own pulse cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private static readonly ushort[] LatchedDurations = [8, 8, 5, 2];
    /// <summary>$A9:F90A-F91A: normal pose cadence, including the finish-drain return pose. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort NormalFrameDuration = 16;
    /// <summary>$A9:F93A-F956: two normal-shaped pulses at the remorse idle cadence before its RNG branch. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort RemorseFrameDuration = 10;
    /// <summary>$A9:F95E: pose0 starts the speed-up/slow-down pulse after the remorse SFX callback.</summary>
    private const ushort RemorseSoundPulse = RemorseRandomBranchOpcode + 4;

    public static int MechanicsWordCount => 35;
    public static int PresentationWordCount => 30;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 2) return new((ushort)(FinishDraining + index * 4), index == 0 ? FinishDrainHold : NormalFrameDuration);
        index -= 2;
        if (index < 5) return index < 4 ? new((ushort)(Normal + index * 4), NormalFrameDuration)
            : new((ushort)(Normal + 16), EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoNormal);
        index -= 5;
        if (index < 5) return index < 4 ? new((ushort)(LatchedOn + index * 4), LatchedDurations[index])
            : new((ushort)(LatchedOn + 16), EnemyInstructionCodePointers.Instruction_GotoLatchedOn);
        index -= 5;
        if (index < 8) return new((ushort)(Remorse + index * 4), RemorseFrameDuration);
        if (index == 8) return new(RemorseRandomBranchOpcode, EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoY_OrPlayRemorseSFX);
        if (index == 9) return new((ushort)(RemorseRandomBranchOpcode + 2), Remorse);
        index -= 10;
        return index < 12 ? new((ushort)(RemorseSoundPulse + index * 4), RemorsePulseDuration(index))
            : new(RemorseLoopOpcode, EnemyInstructionCodePointers.Instruction_BabyMetroid_GotoRemorse);
    }

    /// <summary>$A9:F95E-F98A remorse sound pulse shortens6..2 then lengthens3..9 ticks,
    /// turning at the next pose0 after one four-pose contraction cycle.</summary>
    private static ushort RemorsePulseDuration(int frame) => (ushort)(2 + Math.Abs(frame - 4));

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 2) return (ushort)(FinishDraining + index * 4 + 2);
        index -= 2;
        if (index < 4) return (ushort)(Normal + index * 4 + 2);
        index -= 4;
        if (index < 4) return (ushort)(LatchedOn + index * 4 + 2);
        index -= 4;
        return index < 8 ? (ushort)(Remorse + index * 4 + 2) : (ushort)(RemorseSoundPulse + (index - 8) * 4 + 2);
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Shitroid instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
