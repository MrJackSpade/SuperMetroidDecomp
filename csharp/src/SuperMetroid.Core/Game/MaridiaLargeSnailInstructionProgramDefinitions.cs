using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Oum's idle, rolling, and attacking programs.
/// Installed gameplay resolves the sixty extended-spritemap operands through the
/// compiled selector catalog; uninstalled diagnostic streams may remain mutable.
/// </summary>
internal abstract class MaridiaLargeSnailInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
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

    /// <summary>Native program bank $A2.</summary>
    internal const byte Bank = 0xa2;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xca4b),
        Entry(FacingLeftIdle),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(FacingLeftAttacking),
        Frame(16),
        Frame(16),
        Op(MaridiaLargeSnailInstructionCodes.PlaySplashedOutOfWaterSound),
        Frame(16),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(2),
        Frame(3),
        Frame(1),
        Frame(3),
        Frame(2),
        Frame(1),
        Frame(18),
        Op(MaridiaLargeSnailInstructionCodes.SetAnimationFinished),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(FacingLeftRollingForwards),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Op(CommonEnemyInstructionCodes.Goto, FacingLeftRollingForwards),
        Entry(FacingLeftRollingBackwards),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        Op(CommonEnemyInstructionCodes.Goto, FacingLeftRollingBackwards),
        Entry(FacingRightIdle),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(FacingRightAttacking),
        Frame(16),
        Frame(16),
        Frame(16),
        Op(MaridiaLargeSnailInstructionCodes.PlaySplashedOutOfWaterSound),
        Frame(2),
        Frame(3),
        Frame(4),
        Frame(2),
        Frame(3),
        Frame(1),
        Frame(3),
        Frame(2),
        Frame(1),
        Frame(18),
        Op(MaridiaLargeSnailInstructionCodes.SetAnimationFinished),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(FacingRightRollingForwards),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightRollingForwards),
        Entry(FacingRightRollingBackwards),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.AllowAttackRotation),
        Frame(7),
        Op(MaridiaLargeSnailInstructionCodes.DisallowAttackRotation),
        Op(CommonEnemyInstructionCodes.Goto, FacingRightRollingBackwards));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Maridia Large Snail instruction mechanics pointer $A2:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
