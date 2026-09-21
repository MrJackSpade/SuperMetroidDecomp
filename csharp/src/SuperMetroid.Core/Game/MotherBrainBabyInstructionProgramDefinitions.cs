namespace SuperMetroid.Core.Game;

/// <summary>One compiled cutscene-Baby mechanics word at its bank-$A9 address.</summary>
internal readonly record struct MotherBrainBabyInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Baby Metroid used by Mother Brain's final
/// cutscene. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MotherBrainBabyInstructionProgramDefinitions
{
    /// <summary><c>InstList_BabyMetroid_Initial</c> at $A9:CFA2.</summary>
    internal const ushort Initial = 0xcfa2;

    /// <summary><c>InstList_BabyMetroid_DrainingMotherBrain</c> at $A9:CFB8.</summary>
    internal const ushort DrainingMotherBrain = 0xcfb8;

    /// <summary><c>InstList_BabyMetroid_TakingFatalBlow</c> at $A9:CFCE.</summary>
    internal const ushort TakingFatalBlow = 0xcfce;

    /// <summary>The first Baby movement routine after the fatal-blow program, at $A9:CFD4.</summary>
    internal const ushort FirstAdjacentMovementCode = 0xcfd4;

    private static readonly MotherBrainBabyInstructionMechanicsWord[] Words =
    [
        new(Initial, 0x0010), new(0xcfa6, 0x0010),
        new(0xcfaa, 0x0010), new(0xcfae, 0x0010),
        new(0xcfb2, MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoInitial),

        new(DrainingMotherBrain, 0x0008), new(0xcfbc, 0x0008),
        new(0xcfc0, 0x0005), new(0xcfc4, 0x0002),
        new(0xcfc8,
            MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoDrainingMotherBrain),

        new(TakingFatalBlow, 0x0080),
        new(0xcfd2, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcfa4, 0xcfa8, 0xcfac, 0xcfb0,
        0xcfba, 0xcfbe, 0xcfc2, 0xcfc6,
        0xcfd0,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MotherBrainBabyInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed cutscene-Baby control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainBabyInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain Baby instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }
}
