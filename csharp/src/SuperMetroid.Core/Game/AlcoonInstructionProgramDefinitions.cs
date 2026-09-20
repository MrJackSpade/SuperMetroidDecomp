namespace SuperMetroid.Core.Game;

internal readonly record struct AlcoonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Alcoon's walking, fire-volley, and airborne
/// programs. Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class AlcoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Alcoon_FacingLeft_Walking_0</c> at $A8:DBE7.</summary>
    internal const ushort WalkingLeft = 0xdbe7;
    /// <summary>The first timed frame in the left-walking loop at $A8:DBE9.</summary>
    internal const ushort WalkingLeftFirstFrame = 0xdbe9;
    /// <summary><c>InstList_Alcoon_FacingLeft_SpawnFireballs</c> at $A8:DC03.</summary>
    internal const ushort FireLeft = 0xdc03;
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingUp</c> at $A8:DC4B.</summary>
    internal const ushort AirborneLeftLookingUp = 0xdc4b;
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingForward</c> at $A8:DC51.</summary>
    internal const ushort AirborneLeftLookingForward = 0xdc51;
    /// <summary><c>InstList_Alcoon_FacingRight_Walking_0</c> at $A8:DC57.</summary>
    internal const ushort WalkingRight = 0xdc57;
    /// <summary>The first timed frame in the right-walking loop at $A8:DC59.</summary>
    internal const ushort WalkingRightFirstFrame = 0xdc59;
    /// <summary><c>InstList_Alcoon_FacingRight_SpawnFireballs</c> at $A8:DC73.</summary>
    internal const ushort FireRight = 0xdc73;
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingUp</c> at $A8:DCBB.</summary>
    internal const ushort AirborneRightLookingUp = 0xdcbb;
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingForward</c> at $A8:DCC1.</summary>
    internal const ushort AirborneRightLookingForward = 0xdcc1;

    private static readonly AlcoonInstructionMechanicsWord[] Words =
    [
        new(0xdbe7, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdbe9, 10),
        new(0xdbed, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdbef, 10),
        new(0xdbf3, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdbf5, 10),
        new(0xdbf9, EnemyInstructionCodePointers.Instruction_Alcoon_DecrementStepCounter_MoveHorizontally),
        new(0xdbfb, 10),
        new(0xdbff, CommonEnemyInstructionCodes.Goto), new(0xdc01, WalkingLeft),

        new(0xdc03, 20), new(0xdc07, 9), new(0xdc0b, 16), new(0xdc0f, 3),
        new(0xdc13, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballHorizontally),
        new(0xdc15, 10), new(0xdc19, 10), new(0xdc1d, 9), new(0xdc21, 16), new(0xdc25, 3),
        new(0xdc29, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballUpward),
        new(0xdc2b, 10), new(0xdc2f, 10), new(0xdc33, 9), new(0xdc37, 16), new(0xdc3b, 3),
        new(0xdc3f, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballDownward),
        new(0xdc41, 40),
        new(0xdc45, EnemyInstructionCodePointers.Instruction_Alcoon_StartWalking),
        new(0xdc47, 1),

        new(0xdc4b, 0x7fff), new(0xdc4f, CommonEnemyInstructionCodes.Sleep),
        new(0xdc51, 0x7fff), new(0xdc55, CommonEnemyInstructionCodes.Sleep),

        new(0xdc57, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdc59, 10),
        new(0xdc5d, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdc5f, 10),
        new(0xdc63, EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision),
        new(0xdc65, 10),
        new(0xdc69, EnemyInstructionCodePointers.Instruction_Alcoon_DecrementStepCounter_MoveHorizontally),
        new(0xdc6b, 10),
        new(0xdc6f, CommonEnemyInstructionCodes.Goto), new(0xdc71, WalkingRight),

        new(0xdc73, 20), new(0xdc77, 9), new(0xdc7b, 16), new(0xdc7f, 3),
        new(0xdc83, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballHorizontally),
        new(0xdc85, 10), new(0xdc89, 10), new(0xdc8d, 9), new(0xdc91, 16), new(0xdc95, 3),
        new(0xdc99, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballUpward),
        new(0xdc9b, 10), new(0xdc9f, 10), new(0xdca3, 9), new(0xdca7, 16), new(0xdcab, 3),
        new(0xdcaf, EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballDownward),
        new(0xdcb1, 40),
        new(0xdcb5, EnemyInstructionCodePointers.Instruction_Alcoon_StartWalking),
        new(0xdcb7, 1),

        new(0xdcbb, 0x7fff), new(0xdcbf, CommonEnemyInstructionCodes.Sleep),
        new(0xdcc1, 0x7fff), new(0xdcc5, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xdbeb, 0xdbf1, 0xdbf7, 0xdbfd,
        0xdc05, 0xdc09, 0xdc0d, 0xdc11, 0xdc17, 0xdc1b, 0xdc1f, 0xdc23,
        0xdc27, 0xdc2d, 0xdc31, 0xdc35, 0xdc39, 0xdc3d, 0xdc43, 0xdc49,
        0xdc4d, 0xdc53,
        0xdc5b, 0xdc61, 0xdc67, 0xdc6d,
        0xdc75, 0xdc79, 0xdc7d, 0xdc81, 0xdc87, 0xdc8b, 0xdc8f, 0xdc93,
        0xdc97, 0xdc9d, 0xdca1, 0xdca5, 0xdca9, 0xdcad, 0xdcb3, 0xdcb9,
        0xdcbd, 0xdcc3,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static AlcoonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            AlcoonInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Alcoon instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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
