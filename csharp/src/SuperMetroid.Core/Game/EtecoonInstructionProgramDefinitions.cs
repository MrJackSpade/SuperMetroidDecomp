namespace SuperMetroid.Core.Game;

internal readonly record struct EtecoonInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// Compiled engine-control words for the friendly Etecoon's overlapping animation programs.
/// Their forty-five spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class EtecoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Etecoon_LookRightAtSamusAndRunLeft</c> at $A7:E81E.</summary>
    internal const ushort LookRightAtSamusAndRunLeft = 0xe81e;
    /// <summary>Post-sleep run-left continuation at $A7:E824.</summary>
    internal const ushort BeginRunningLeft = 0xe824;
    /// <summary><c>InstList_Etecoon_RunningLeft</c> at $A7:E828.</summary>
    internal const ushort RunningLeft = 0xe828;
    /// <summary><c>InstList_Etecoon_WallJump_0</c> at $A7:E83C.</summary>
    internal const ushort WallJumpLeft = 0xe83c;
    /// <summary><c>InstList_Etecoon_WallJump_1</c> at $A7:E840.</summary>
    internal const ushort WallJumpLeftLoop = 0xe840;
    /// <summary><c>InstList_Etecoon_Hopping_FacingLeft</c> at $A7:E854.</summary>
    internal const ushort HoppingFacingLeft = 0xe854;
    /// <summary>Post-sleep left-hop continuation at $A7:E85A.</summary>
    internal const ushort ContinueHoppingFacingLeft = 0xe85a;
    /// <summary><c>InstList_Etecoon_HitCeiling</c> at $A7:E862.</summary>
    internal const ushort HitCeiling = 0xe862;
    /// <summary><c>InstList_Etecoon_WallJumpLeftEligible</c> at $A7:E870.</summary>
    internal const ushort WallJumpLeftEligible = 0xe870;
    /// <summary><c>InstList_Etecoon_LookLeftAtSamusAndRunRight</c> at $A7:E876.</summary>
    internal const ushort LookLeftAtSamusAndRunRight = 0xe876;
    /// <summary>Post-sleep run-right continuation at $A7:E87C.</summary>
    internal const ushort BeginRunningRight = 0xe87c;
    /// <summary><c>InstList_Etecoon_RunningRight</c> at $A7:E880.</summary>
    internal const ushort RunningRight = 0xe880;
    /// <summary><c>InstList_Etecoon_WallJumpRight</c> at $A7:E894.</summary>
    internal const ushort WallJumpRight = 0xe894;
    /// <summary><c>InstList_Etecoon_JumpingRight</c> at $A7:E898.</summary>
    internal const ushort JumpingRight = 0xe898;
    /// <summary><c>InstList_Etecoon_Hopping_FacingRight</c> at $A7:E8AC.</summary>
    internal const ushort HoppingFacingRight = 0xe8ac;
    /// <summary>Post-sleep right-hop continuation at $A7:E8B2.</summary>
    internal const ushort ContinueHoppingFacingRight = 0xe8b2;
    /// <summary><c>InstList_Etecoon_WallJumpRightEligible</c> at $A7:E8C8.</summary>
    internal const ushort WallJumpRightEligible = 0xe8c8;
    /// <summary><c>InstList_Etecoon_Initial</c> at $A7:E8CE.</summary>
    internal const ushort Initial = 0xe8ce;
    /// <summary><c>InstList_Etecoon_Flexing_0</c> at $A7:E8D6.</summary>
    internal const ushort Flexing = 0xe8d6;
    /// <summary><c>InstList_Etecoon_Flexing_1</c> at $A7:E8DA.</summary>
    internal const ushort FlexingLoop = 0xe8da;
    /// <summary>The first Etecoon movement constant after the programs, at $A7:E900.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0xe900;

    private static readonly EtecoonInstructionMechanicsWord[] Words =
    [
        new(LookRightAtSamusAndRunLeft, 5),
        new(0xe822, CommonEnemyInstructionCodes.Sleep),
        new(BeginRunningLeft, 1),
        new(RunningLeft, 5), new(0xe82c, 5), new(0xe830, 5), new(0xe834, 5),
        new(0xe838, CommonEnemyInstructionCodes.Goto), new(0xe83a, RunningLeft),
        new(WallJumpLeft, 8), new(WallJumpLeftLoop, 3), new(0xe844, 3),
        new(0xe848, 3), new(0xe84c, 3),
        new(0xe850, CommonEnemyInstructionCodes.Goto), new(0xe852, WallJumpLeftLoop),
        new(HoppingFacingLeft, 1), new(0xe858, CommonEnemyInstructionCodes.Sleep),
        new(ContinueHoppingFacingLeft, 0x0c), new(0xe85e, 0x0c),
        new(HitCeiling, 6), new(0xe866, 0x0c), new(0xe86a, 0x0c),
        new(0xe86e, CommonEnemyInstructionCodes.Sleep),
        new(WallJumpLeftEligible, 1), new(0xe874, CommonEnemyInstructionCodes.Sleep),
        new(LookLeftAtSamusAndRunRight, 5),
        new(0xe87a, CommonEnemyInstructionCodes.Sleep),
        new(BeginRunningRight, 1),
        new(RunningRight, 5), new(0xe884, 5), new(0xe888, 5), new(0xe88c, 5),
        new(0xe890, CommonEnemyInstructionCodes.Goto), new(0xe892, RunningRight),
        new(WallJumpRight, 8),
        new(JumpingRight, 3), new(0xe89c, 3), new(0xe8a0, 3), new(0xe8a4, 3),
        new(0xe8a8, CommonEnemyInstructionCodes.Goto), new(0xe8aa, JumpingRight),
        new(HoppingFacingRight, 1), new(0xe8b0, CommonEnemyInstructionCodes.Sleep),
        new(ContinueHoppingFacingRight, 0x0c), new(0xe8b6, 0x0c),
        new(0xe8ba, 6), new(0xe8be, 0x0c), new(0xe8c2, 0x0c),
        new(0xe8c6, CommonEnemyInstructionCodes.Sleep),
        new(WallJumpRightEligible, 1), new(0xe8cc, CommonEnemyInstructionCodes.Sleep),
        new(Initial, 8), new(0xe8d2, CommonEnemyInstructionCodes.Goto),
        new(0xe8d4, Initial),
        new(Flexing, CommonEnemyInstructionCodes.SetTimer), new(0xe8d8, 4),
        new(FlexingLoop, 8), new(0xe8de, 8), new(0xe8e2, 8), new(0xe8e6, 8),
        new(0xe8ea, 8), new(0xe8ee, 8),
        new(0xe8f2, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xe8f4, FlexingLoop), new(0xe8f6, 0x20), new(0xe8fa, 0x20),
        new(0xe8fe, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe820, 0xe826,
        0xe82a, 0xe82e, 0xe832, 0xe836,
        0xe83e, 0xe842, 0xe846, 0xe84a, 0xe84e,
        0xe856, 0xe85c, 0xe860,
        0xe864, 0xe868, 0xe86c,
        0xe872,
        0xe878, 0xe87e,
        0xe882, 0xe886, 0xe88a, 0xe88e,
        0xe896,
        0xe89a, 0xe89e, 0xe8a2, 0xe8a6,
        0xe8ae, 0xe8b4, 0xe8b8, 0xe8bc, 0xe8c0, 0xe8c4,
        0xe8ca,
        0xe8d0,
        0xe8dc, 0xe8e0, 0xe8e4, 0xe8e8, 0xe8ec, 0xe8f0,
        0xe8f8, 0xe8fc,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EtecoonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }
        throw new InvalidDataException(
            $"Etecoon instruction mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
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
