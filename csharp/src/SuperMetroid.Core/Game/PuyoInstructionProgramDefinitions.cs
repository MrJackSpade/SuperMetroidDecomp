namespace SuperMetroid.Core.Game;

internal readonly record struct PuyoInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for Puyo's three grounded loops and five airborne pose
/// programs. Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PuyoInstructionProgramDefinitions
{
    /// <summary><c>InstList_Puyo_GroundedDropping_Fast</c> at $A2:99AD.</summary>
    internal const ushort GroundedFast = 0x99ad;

    /// <summary><c>InstList_Puyo_GroundedDropping_Medium</c> at $A2:99C1.</summary>
    internal const ushort GroundedMedium = 0x99c1;

    /// <summary><c>InstList_Puyo_GroundedDropping_Slow</c> at $A2:99D5.</summary>
    internal const ushort GroundedSlow = 0x99d5;

    /// <summary><c>InstList_Puyo_HoppingRight_0_HoppingLeft_4</c> at $A2:99E9.</summary>
    internal const ushort RightFrame0LeftFrame4 = 0x99e9;

    /// <summary><c>InstList_Puyo_HoppingRight_1_HoppingLeft_3</c> at $A2:99EF.</summary>
    internal const ushort RightFrame1LeftFrame3 = 0x99ef;

    /// <summary><c>InstList_Puyo_Hopping_2</c> at $A2:99F5.</summary>
    internal const ushort Frame2 = 0x99f5;

    /// <summary><c>InstList_Puyo_HoppingRight_3_HoppingLeft_1</c> at $A2:99FB.</summary>
    internal const ushort RightFrame3LeftFrame1 = 0x99fb;

    /// <summary><c>InstList_Puyo_HoppingRight_4_HoppingLeft_0</c> at $A2:9A01.</summary>
    internal const ushort RightFrame4LeftFrame0 = 0x9a01;

    /// <summary>The final airborne-pose sleep opcode at $A2:9A05.</summary>
    internal const ushort LastSleepOpcode = 0x9a05;

    /// <summary>The first word of <c>PuyoHopTable</c>, immediately after the programs at $A2:9A07.</summary>
    internal const ushort FirstAdjacentDefinition = 0x9a07;

    private static readonly PuyoInstructionMechanicsWord[] Words =
    [
        new(0x99ad, 5), new(0x99b1, 5), new(0x99b5, 5), new(0x99b9, 5),
        new(0x99bd, CommonEnemyInstructionCodes.Goto), new(0x99bf, GroundedFast),
        new(0x99c1, 8), new(0x99c5, 8), new(0x99c9, 8), new(0x99cd, 8),
        new(0x99d1, CommonEnemyInstructionCodes.Goto), new(0x99d3, GroundedMedium),
        new(0x99d5, 10), new(0x99d9, 10), new(0x99dd, 10), new(0x99e1, 10),
        new(0x99e5, CommonEnemyInstructionCodes.Goto), new(0x99e7, GroundedSlow),
        new(0x99e9, 1), new(0x99ed, CommonEnemyInstructionCodes.Sleep),
        new(0x99ef, 1), new(0x99f3, CommonEnemyInstructionCodes.Sleep),
        new(0x99f5, 1), new(0x99f9, CommonEnemyInstructionCodes.Sleep),
        new(0x99fb, 1), new(0x99ff, CommonEnemyInstructionCodes.Sleep),
        new(0x9a01, 1), new(LastSleepOpcode, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x99af, 0x99b3, 0x99b7, 0x99bb,
        0x99c3, 0x99c7, 0x99cb, 0x99cf,
        0x99d7, 0x99db, 0x99df, 0x99e3,
        0x99eb, 0x99f1, 0x99f7, 0x99fd, 0x9a03,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PuyoInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PuyoInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Puyo instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
