namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct HopperInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Sidehopper and Dessgeega floor/ceiling animation
/// programs. Their forty interleaved spritemap operands remain live cartridge presentation
/// data so replacing the visual asset pipeline does not change enemy mechanics.
/// </summary>
internal static class HopperInstructionProgramDefinitions
{
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideUp</c> at $A3:AA76.</summary>
    internal const ushort SidehopperJumpingFloor = 0xaa76;
    /// <summary><c>InstList_Sidehopper_Landed_UpsideUp</c> at $A3:AA82.</summary>
    internal const ushort SidehopperLandedFloor = 0xaa82;
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideDown</c> at $A3:AA9C.</summary>
    internal const ushort SidehopperJumpingCeiling = 0xaa9c;
    /// <summary><c>InstList_Sidehopper_Landed_UpsideDown</c> at $A3:AAA8.</summary>
    internal const ushort SidehopperLandedCeiling = 0xaaa8;

    /// <summary><c>InstList_Dessgeega_Hopping_UpsideUp</c> at $A3:AFA5.</summary>
    internal const ushort DessgeegaJumpingFloor = 0xafa5;
    /// <summary><c>InstList_Dessgeega_Landed_UpsideUp</c> at $A3:AFAD.</summary>
    internal const ushort DessgeegaLandedFloor = 0xafad;
    /// <summary><c>InstList_Dessgeega_Hopping_UpsideDown</c> at $A3:AFC3.</summary>
    internal const ushort DessgeegaJumpingCeiling = 0xafc3;
    /// <summary><c>InstList_Dessgeega_Landed_UpsideDown</c> at $A3:AFCB.</summary>
    internal const ushort DessgeegaLandedCeiling = 0xafcb;

    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideUp</c> at $A3:B0C5.</summary>
    internal const ushort LargeSidehopperJumpingFloor = 0xb0c5;
    /// <summary><c>InstList_SidehopperLarge_Landed_UpsideUp</c> at $A3:B0D1.</summary>
    internal const ushort LargeSidehopperLandedFloor = 0xb0d1;
    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideDown</c> at $A3:B0EB.</summary>
    internal const ushort LargeSidehopperJumpingCeiling = 0xb0eb;
    /// <summary><c>InstList_SidehopperLarge_Landed_UpsideDown</c> at $A3:B0F7.</summary>
    internal const ushort LargeSidehopperLandedCeiling = 0xb0f7;

    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideUp</c> at $A3:B237.</summary>
    internal const ushort LargeDessgeegaJumpingFloor = 0xb237;
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideUp</c> at $A3:B23F.</summary>
    internal const ushort LargeDessgeegaLandedFloor = 0xb23f;
    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideDown</c> at $A3:B255.</summary>
    internal const ushort LargeDessgeegaJumpingCeiling = 0xb255;
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideDown</c> at $A3:B25D.</summary>
    internal const ushort LargeDessgeegaLandedCeiling = 0xb25d;

    /// <summary>The final hopper physics-table word immediately before the first program.</summary>
    internal const ushort LastAdjacentPhysicsWord = 0xaa74;

    private static readonly HopperInstructionMechanicsWord[] Words =
    [
        new(SidehopperJumpingFloor, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xaa78, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xaa7a, 0x005d), new(0xaa7c, 1), new(0xaa80, CommonEnemyInstructionCodes.Sleep),

        new(SidehopperLandedFloor, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xaa84, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xaa86, 0x005e), new(0xaa88, 2), new(0xaa8c, 5), new(0xaa90, 2),
        new(0xaa94, 3), new(0xaa98, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xaa9a, CommonEnemyInstructionCodes.Sleep),

        new(SidehopperJumpingCeiling, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xaa9e, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xaaa0, 0x005d), new(0xaaa2, 1), new(0xaaa6, CommonEnemyInstructionCodes.Sleep),

        new(SidehopperLandedCeiling, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xaaaa, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xaaac, 0x005e), new(0xaaae, 2), new(0xaab2, 5), new(0xaab6, 2),
        new(0xaaba, 3), new(0xaabe, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xaac0, CommonEnemyInstructionCodes.Sleep),

        new(DessgeegaJumpingFloor, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xafa7, 1), new(0xafab, CommonEnemyInstructionCodes.Sleep),
        new(DessgeegaLandedFloor, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xafaf, 2), new(0xafb3, 5), new(0xafb7, 2), new(0xafbb, 3),
        new(0xafbf, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xafc1, CommonEnemyInstructionCodes.Sleep),
        new(DessgeegaJumpingCeiling, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xafc5, 1), new(0xafc9, CommonEnemyInstructionCodes.Sleep),
        new(DessgeegaLandedCeiling, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xafcd, 2), new(0xafd1, 5), new(0xafd5, 2), new(0xafd9, 3),
        new(0xafdd, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xafdf, CommonEnemyInstructionCodes.Sleep),

        new(LargeSidehopperJumpingFloor, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb0c7, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xb0c9, 0x005d), new(0xb0cb, 1), new(0xb0cf, CommonEnemyInstructionCodes.Sleep),
        new(LargeSidehopperLandedFloor, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb0d3, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xb0d5, 0x005e), new(0xb0d7, 2), new(0xb0db, 5), new(0xb0df, 2),
        new(0xb0e3, 3), new(0xb0e7, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xb0e9, CommonEnemyInstructionCodes.Sleep),
        new(LargeSidehopperJumpingCeiling, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb0ed, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xb0ef, 0x005d), new(0xb0f1, 1), new(0xb0f5, CommonEnemyInstructionCodes.Sleep),
        new(LargeSidehopperLandedCeiling, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb0f9, EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3),
        new(0xb0fb, 0x005e), new(0xb0fd, 2), new(0xb101, 5), new(0xb105, 2),
        new(0xb109, 3), new(0xb10d, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xb10f, CommonEnemyInstructionCodes.Sleep),

        new(LargeDessgeegaJumpingFloor, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb239, 1), new(0xb23d, CommonEnemyInstructionCodes.Sleep),
        new(LargeDessgeegaLandedFloor, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb241, 2), new(0xb245, 5), new(0xb249, 2), new(0xb24d, 3),
        new(0xb251, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xb253, CommonEnemyInstructionCodes.Sleep),
        new(LargeDessgeegaJumpingCeiling, CommonEnemyInstructionCodes.EnableOffScreenProcessing),
        new(0xb257, 1), new(0xb25b, CommonEnemyInstructionCodes.Sleep),
        new(LargeDessgeegaLandedCeiling, CommonEnemyInstructionCodes.DisableOffScreenProcessing),
        new(0xb25f, 2), new(0xb263, 5), new(0xb267, 2), new(0xb26b, 3),
        new(0xb26f, EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop),
        new(0xb271, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xaa7e, 0xaa8a, 0xaa8e, 0xaa92, 0xaa96,
        0xaaa4, 0xaab0, 0xaab4, 0xaab8, 0xaabc,
        0xafa9, 0xafb1, 0xafb5, 0xafb9, 0xafbd,
        0xafc7, 0xafcf, 0xafd3, 0xafd7, 0xafdb,
        0xb0cd, 0xb0d9, 0xb0dd, 0xb0e1, 0xb0e5,
        0xb0f3, 0xb0ff, 0xb103, 0xb107, 0xb10b,
        0xb23b, 0xb243, 0xb247, 0xb24b, 0xb24f,
        0xb259, 0xb261, 0xb265, 0xb269, 0xb26d,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static HopperInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            HopperInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Hopper instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
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
