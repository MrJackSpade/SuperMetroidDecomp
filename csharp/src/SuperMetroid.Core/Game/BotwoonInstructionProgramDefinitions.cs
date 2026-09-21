namespace SuperMetroid.Core.Game;

/// <summary>One compiled Botwoon head mechanics word at its bank-$B3 address.</summary>
internal readonly record struct BotwoonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Botwoon's selector-reachable head movement,
/// hiding, and spit programs. Interleaved spritemap operands remain cartridge data.
/// </summary>
internal static class BotwoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpLeft</c> at $B3:9341.</summary>
    internal const ushort MovingUpLeft = 0x9341;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingLeft</c> at $B3:9349.</summary>
    internal const ushort MovingLeft = 0x9349;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownLeft</c> at $B3:9351.</summary>
    internal const ushort MovingDownLeft = 0x9351;
    /// <summary>
    /// <c>InstList_Botwoon_MouthClosed_AimingDown_FacingRight</c> at $B3:9361.
    /// </summary>
    internal const ushort MovingDown = 0x9361;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownRight</c> at $B3:9369.</summary>
    internal const ushort MovingDownRight = 0x9369;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingRight</c> at $B3:9371.</summary>
    internal const ushort MovingRight = 0x9371;
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpRight</c> at $B3:9379.</summary>
    internal const ushort MovingUpRight = 0x9379;
    /// <summary>
    /// <c>InstList_Botwoon_MouthClosed_AimingUp_FacingRight</c> at $B3:9381.
    /// </summary>
    internal const ushort MovingUp = 0x9381;
    /// <summary><c>InstList_Botwoon_Hide</c> at $B3:9389.</summary>
    internal const ushort Hidden = 0x9389;

    /// <summary><c>InstList_Botwoon_Spit_AimingUpLeft</c> at $B3:939F.</summary>
    internal const ushort SpittingUpLeft = 0x939f;
    /// <summary><c>InstList_Botwoon_Spit_AimingLeft</c> at $B3:93AF.</summary>
    internal const ushort SpittingLeft = 0x93af;
    /// <summary><c>InstList_Botwoon_Spit_AimingDownLeft</c> at $B3:93BF.</summary>
    internal const ushort SpittingDownLeft = 0x93bf;
    /// <summary><c>InstList_Botwoon_Spit_AimingDown_FacingRight</c> at $B3:93DF.</summary>
    internal const ushort SpittingDown = 0x93df;
    /// <summary><c>InstList_Botwoon_Spit_AimingDownRight</c> at $B3:93EF.</summary>
    internal const ushort SpittingDownRight = 0x93ef;
    /// <summary><c>InstList_Botwoon_Spit_AimingRight</c> at $B3:93FF.</summary>
    internal const ushort SpittingRight = 0x93ff;
    /// <summary><c>InstList_Botwoon_Spit_AimingUpRight</c> at $B3:940F.</summary>
    internal const ushort SpittingUpRight = 0x940f;
    /// <summary><c>InstList_Botwoon_Spit_AimingUp_FacingRight</c> at $B3:941F.</summary>
    internal const ushort SpittingUp = 0x941f;

    /// <summary>
    /// <c>UNSUED_InstList_Botwoon_MouthClosed_AimDown_FaceLeft_B39359</c> at $B3:9359.
    /// </summary>
    internal const ushort UnusedMovingHorizontal = 0x9359;
    /// <summary>
    /// <c>UNUSED_InstList_Botwoon_Spit_AimingDown_FacingLeft_B393CF</c> at $B3:93CF.
    /// </summary>
    internal const ushort UnusedSpittingHorizontal = 0x93cf;
    /// <summary>
    /// <c>UNUSED_InstList_Botwoon_Hidden_AimingUp_FacingLeft_B3942F</c> at $B3:942F.
    /// </summary>
    internal const ushort FirstAdjacentProgram = 0x942f;

    private static readonly BotwoonInstructionMechanicsWord[] Words =
    [
        new(0x9341, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC),
        new(0x9343, 0x0001), new(0x9347, CommonEnemyInstructionCodes.Sleep),
        new(0x9349, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8),
        new(0x934b, 0x0001), new(0x934f, CommonEnemyInstructionCodes.Sleep),
        new(0x9351, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate),
        new(0x9353, 0x0001), new(0x9357, CommonEnemyInstructionCodes.Sleep),

        new(0x9361, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again),
        new(0x9363, 0x0001), new(0x9367, CommonEnemyInstructionCodes.Sleep),
        new(0x9369, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again),
        new(0x936b, 0x0001), new(0x936f, CommonEnemyInstructionCodes.Sleep),
        new(0x9371, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8_duplicate),
        new(0x9373, 0x0001), new(0x9377, CommonEnemyInstructionCodes.Sleep),
        new(0x9379, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again2),
        new(0x937b, 0x0001), new(0x937f, CommonEnemyInstructionCodes.Sleep),
        new(0x9381, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again2),
        new(0x9383, 0x0001), new(0x9387, CommonEnemyInstructionCodes.Sleep),
        new(Hidden, 0x0001), new(0x938d, CommonEnemyInstructionCodes.Sleep),

        new(SpittingUpLeft, 0x0020),
        new(0x93a3, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC),
        new(0x93a5, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x93a7, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x93a9, 0x0010), new(0x93ad, CommonEnemyInstructionCodes.Sleep),
        new(SpittingLeft, 0x0020),
        new(0x93b3, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8),
        new(0x93b5, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x93b7, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x93b9, 0x0019), new(0x93bd, CommonEnemyInstructionCodes.Sleep),
        new(SpittingDownLeft, 0x0020),
        new(0x93c3, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate),
        new(0x93c5, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x93c7, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x93c9, 0x0010), new(0x93cd, CommonEnemyInstructionCodes.Sleep),

        new(SpittingDown, 0x0020),
        new(0x93e3, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again),
        new(0x93e5, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x93e7, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x93e9, 0x0010), new(0x93ed, CommonEnemyInstructionCodes.Sleep),
        new(SpittingDownRight, 0x0020),
        new(0x93f3, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again),
        new(0x93f5, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x93f7, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x93f9, 0x0010), new(0x93fd, CommonEnemyInstructionCodes.Sleep),
        new(SpittingRight, 0x0020),
        new(0x9403, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8_duplicate),
        new(0x9405, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x9407, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x9409, 0x0010), new(0x940d, CommonEnemyInstructionCodes.Sleep),
        new(SpittingUpRight, 0x0020),
        new(0x9413, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again2),
        new(0x9415, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x9417, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x9419, 0x0010), new(0x941d, CommonEnemyInstructionCodes.Sleep),
        new(SpittingUp, 0x0020),
        new(0x9423, BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again2),
        new(0x9425, BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX),
        new(0x9427, BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag),
        new(0x9429, 0x0010), new(0x942d, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9345, 0x934d, 0x9355, 0x9365, 0x936d, 0x9375, 0x937d, 0x9385,
        0x938b,
        0x93a1, 0x93ab, 0x93b1, 0x93bb, 0x93c1, 0x93cb,
        0x93e1, 0x93eb, 0x93f1, 0x93fb, 0x9401, 0x940b, 0x9411, 0x941b,
        0x9421, 0x942b,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static BotwoonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BotwoonInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Botwoon instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
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
