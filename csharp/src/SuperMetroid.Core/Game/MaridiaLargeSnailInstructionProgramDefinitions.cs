namespace SuperMetroid.Core.Game;

internal readonly record struct MaridiaLargeSnailInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Oum's idle, rolling, and attacking programs.
/// Their sixty extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MaridiaLargeSnailInstructionProgramDefinitions
{
    /// <summary><c>InstList_Oum_FacingLeft_Idle</c> at $A2:CA4B.</summary>
    internal const ushort FacingLeftIdle = 0xca4b;

    /// <summary><c>InstList_Oum_FacingLeft_Attacking</c> at $A2:CA51.</summary>
    internal const ushort FacingLeftAttacking = 0xca51;

    /// <summary><c>InstList_Oum_FacingLeft_RollingForwards</c> at $A2:CA8B.</summary>
    internal const ushort FacingLeftRollingForwards = 0xca8b;

    /// <summary><c>InstList_Oum_FacingLeft_RollingBackwards</c> at $A2:CAB3.</summary>
    internal const ushort FacingLeftRollingBackwards = 0xcab3;

    /// <summary><c>InstList_Oum_FacingRight_Idle</c> at $A2:CADB.</summary>
    internal const ushort FacingRightIdle = 0xcadb;

    /// <summary><c>InstList_Oum_FacingRight_Attacking</c> at $A2:CAE1.</summary>
    internal const ushort FacingRightAttacking = 0xcae1;

    /// <summary><c>InstList_Oum_FacingRight_RollingForwards</c> at $A2:CB1B.</summary>
    internal const ushort FacingRightRollingForwards = 0xcb1b;

    /// <summary><c>InstList_Oum_FacingRight_RollingBackwards</c> at $A2:CB43.</summary>
    internal const ushort FacingRightRollingBackwards = 0xcb43;

    /// <summary>The selector table immediately following Oum's programs, at $A2:CB77.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xcb77;

    private static readonly MaridiaLargeSnailInstructionMechanicsWord[] Words =
    [
        new(FacingLeftIdle, 1),
        new(0xca4f, CommonEnemyInstructionCodes.Sleep),

        new(FacingLeftAttacking, 0x10), new(0xca55, 0x10),
        new(0xca59, MaridiaLargeSnailInstructionCodes.PlaySplashedOutOfWaterSound),
        new(0xca5b, 0x10), new(0xca5f, 2), new(0xca63, 3), new(0xca67, 4),
        new(0xca6b, 2), new(0xca6f, 3), new(0xca73, 1), new(0xca77, 3),
        new(0xca7b, 2), new(0xca7f, 1), new(0xca83, 0x12),
        new(0xca87, MaridiaLargeSnailInstructionCodes.SetAnimationFinished),
        new(0xca89, CommonEnemyInstructionCodes.Sleep),

        new(FacingLeftRollingForwards, 7),
        new(0xca8f, MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        new(0xca91, 7),
        new(0xca95, MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        new(0xca97, 7), new(0xca9b, 7), new(0xca9f, 7), new(0xcaa3, 7),
        new(0xcaa7, 7), new(0xcaab, 7),
        new(0xcaaf, CommonEnemyInstructionCodes.Goto),
        new(0xcab1, FacingLeftRollingForwards),

        new(FacingLeftRollingBackwards, 7), new(0xcab7, 7), new(0xcabb, 7),
        new(0xcabf, 7), new(0xcac3, 7), new(0xcac7, 7), new(0xcacb, 7),
        new(0xcacf, MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        new(0xcad1, 7),
        new(0xcad5, MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        new(0xcad7, CommonEnemyInstructionCodes.Goto),
        new(0xcad9, FacingLeftRollingBackwards),

        new(FacingRightIdle, 1),
        new(0xcadf, CommonEnemyInstructionCodes.Sleep),

        new(FacingRightAttacking, 0x10), new(0xcae5, 0x10), new(0xcae9, 0x10),
        new(0xcaed, MaridiaLargeSnailInstructionCodes.PlaySplashedOutOfWaterSound),
        new(0xcaef, 2), new(0xcaf3, 3), new(0xcaf7, 4), new(0xcafb, 2),
        new(0xcaff, 3), new(0xcb03, 1), new(0xcb07, 3), new(0xcb0b, 2),
        new(0xcb0f, 1), new(0xcb13, 0x12),
        new(0xcb17, MaridiaLargeSnailInstructionCodes.SetAnimationFinished),
        new(0xcb19, CommonEnemyInstructionCodes.Sleep),

        new(FacingRightRollingForwards, 7),
        new(0xcb1f, MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        new(0xcb21, 7),
        new(0xcb25, MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        new(0xcb27, 7), new(0xcb2b, 7), new(0xcb2f, 7), new(0xcb33, 7),
        new(0xcb37, 7), new(0xcb3b, 7),
        new(0xcb3f, CommonEnemyInstructionCodes.Goto),
        new(0xcb41, FacingRightRollingForwards),

        new(FacingRightRollingBackwards, 7), new(0xcb47, 7), new(0xcb4b, 7),
        new(0xcb4f, 7), new(0xcb53, 7), new(0xcb57, 7), new(0xcb5b, 7),
        new(0xcb5f, MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        new(0xcb61, 7),
        new(0xcb65, MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        new(0xcb67, CommonEnemyInstructionCodes.Goto),
        new(0xcb69, FacingRightRollingBackwards),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xca4d,
        0xca53, 0xca57, 0xca5d, 0xca61, 0xca65, 0xca69, 0xca6d,
        0xca71, 0xca75, 0xca79, 0xca7d, 0xca81, 0xca85,
        0xca8d, 0xca93, 0xca99, 0xca9d, 0xcaa1, 0xcaa5, 0xcaa9, 0xcaad,
        0xcab5, 0xcab9, 0xcabd, 0xcac1, 0xcac5, 0xcac9, 0xcacd, 0xcad3,
        0xcadd,
        0xcae3, 0xcae7, 0xcaeb, 0xcaf1, 0xcaf5, 0xcaf9, 0xcafd,
        0xcb01, 0xcb05, 0xcb09, 0xcb0d, 0xcb11, 0xcb15,
        0xcb1d, 0xcb23, 0xcb29, 0xcb2d, 0xcb31, 0xcb35, 0xcb39, 0xcb3d,
        0xcb45, 0xcb49, 0xcb4d, 0xcb51, 0xcb55, 0xcb59, 0xcb5d, 0xcb63,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MaridiaLargeSnailInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Maridia Large Snail instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
