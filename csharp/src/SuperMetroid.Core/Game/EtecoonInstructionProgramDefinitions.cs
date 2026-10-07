using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the friendly Etecoon's overlapping animation programs.
/// Their forty-five spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class EtecoonInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
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

    /// <summary>Native program bank $A7.</summary>
    internal const byte Bank = 0xa7;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xe81e),
        Entry(LookRightAtSamusAndRunLeft),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BeginRunningLeft),
        Frame(1),
        Entry(RunningLeft),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, RunningLeft),
        Entry(WallJumpLeft),
        Frame(8),
        Entry(WallJumpLeftLoop),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op(CommonEnemyInstructionCodes.Goto, WallJumpLeftLoop),
        Entry(HoppingFacingLeft),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(ContinueHoppingFacingLeft),
        Frame(12),
        Frame(12),
        Entry(HitCeiling),
        Frame(6),
        Frame(12),
        Frame(12),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(WallJumpLeftEligible),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LookLeftAtSamusAndRunRight),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BeginRunningRight),
        Frame(1),
        Entry(RunningRight),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(CommonEnemyInstructionCodes.Goto, RunningRight),
        Entry(WallJumpRight),
        Frame(8),
        Entry(JumpingRight),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op(CommonEnemyInstructionCodes.Goto, JumpingRight),
        Entry(HoppingFacingRight),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(ContinueHoppingFacingRight),
        Frame(12),
        Frame(12),
        Frame(6),
        Frame(12),
        Frame(12),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(WallJumpRightEligible),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(Initial),
        Frame(8),
        Op(CommonEnemyInstructionCodes.Goto, Initial),
        Entry(Flexing),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0004),
        Entry(FlexingLoop),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Frame(8),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, FlexingLoop),
        Frame(32),
        Frame(32),
        Op(CommonEnemyInstructionCodes.Sleep));

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
                $"Etecoon instruction mechanics pointer $A7:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
