using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for KiHunter body and wing instruction programs.
/// Interleaved spritemap operands are compiled selectors for installed presentation art.
/// </summary>
internal abstract class KiHunterInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kihunter_Idling_FacingLeft</c> at $A8:E9FA.</summary>
    internal const ushort FlyingLeft = 0xe9fa;
    /// <summary><c>InstList_Kihunter_Swiping_FacingLeft</c> at $A8:EA08.</summary>
    internal const ushort SwoopLeft = 0xea08;
    /// <summary><c>InstList_Kihunter_Idling_FacingRight</c> at $A8:EA24.</summary>
    internal const ushort FlyingRight = 0xea24;
    /// <summary><c>InstList_Kihunter_Swiping_FacingRight</c> at $A8:EA32.</summary>
    internal const ushort SwoopRight = 0xea32;
    /// <summary><c>InstList_KihunterWings_FacingLeft</c> at $A8:EA4E.</summary>
    internal const ushort WingsLeft = 0xea4e;
    /// <summary><c>InstList_KihunterWings_FacingRight</c> at $A8:EA5E.</summary>
    internal const ushort WingsRight = 0xea5e;
    /// <summary><c>InstList_KihunterWings_Falling</c> at $A8:EA7E.</summary>
    internal const ushort DetachedWings = 0xea7e;
    /// <summary><c>InstList_Kihunter_Hop_FacingLeft</c> at $A8:EA8A.</summary>
    internal const ushort JumpLeft = 0xea8a;
    /// <summary><c>InstList_Kihunter_Hop_FacingRight</c> at $A8:EAA6.</summary>
    internal const ushort JumpRight = 0xeaa6;
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingLeft</c> at $A8:EAC2.</summary>
    internal const ushort LandLeft = 0xeac2;
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingRight</c> at $A8:EADA.</summary>
    internal const ushort LandRight = 0xeada;
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingLeft</c> at $A8:EAF2.</summary>
    internal const ushort SpitLeft = 0xeaf2;
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingRight</c> at $A8:EB10.</summary>
    internal const ushort SpitRight = 0xeb10;

    /// <summary><c>Instruction_Kihunter_SetIdlingInstListsFacingForwards</c> at $A8:F526.</summary>
    private const ushort KihunterSetIdlingInstListsFacingForwards = 0xf526;
    /// <summary><c>Instruction_Kihunter_SetFunctionToHop</c> at $A8:F5E4.</summary>
    private const ushort KihunterSetFunctionToHop = 0xf5e4;
    /// <summary><c>Instruction_Kihunter_SetFunctionTo_Wingless_Thinking</c> at $A8:F67F.</summary>
    private const ushort KihunterSetFunctionToWinglessThinking = 0xf67f;
    /// <summary><c>Instruction_Kihunter_FireAcidSpitLeft</c> at $A8:F6D2.</summary>
    private const ushort KihunterFireAcidSpitLeft = 0xf6d2;
    /// <summary><c>Instruction_Kihunter_FireAcidSpitRight</c> at $A8:F6D8.</summary>
    private const ushort KihunterFireAcidSpitRight = 0xf6d8;

    /// <summary>Native program bank $A8.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Compiled Ki Hunter body and wing programs, with their frame words identified for installed visual data.</summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xe9fa),
        Entry(FlyingLeft),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(KihunterSetIdlingInstListsFacingForwards),
        Entry(SwoopLeft),
        Frame(2),
        Frame(6),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(32),
        Op(CommonEnemyInstructionCodes.Goto, FlyingLeft),
        Entry(FlyingRight),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(KihunterSetIdlingInstListsFacingForwards),
        Entry(SwoopRight),
        Frame(2),
        Frame(6),
        Frame(2),
        Frame(2),
        Frame(2),
        Frame(32),
        Op(CommonEnemyInstructionCodes.Goto, FlyingRight),
        Entry(WingsLeft),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Goto, WingsLeft),
        Entry(WingsRight),
        Frame(2),
        Frame(2),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Goto, WingsRight),
        Origin(0xea7e),
        Entry(DetachedWings),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Skip(6),
        Entry(JumpLeft),
        Frame(8),
        Frame(8),
        Frame(11),
        Frame(2),
        Frame(2),
        Op(KihunterSetFunctionToHop),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(JumpRight),
        Frame(8),
        Frame(8),
        Frame(11),
        Frame(2),
        Frame(2),
        Op(KihunterSetFunctionToHop),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LandLeft),
        Frame(8),
        Frame(8),
        Frame(11),
        Frame(8),
        Op(KihunterSetFunctionToWinglessThinking),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LandRight),
        Frame(8),
        Frame(8),
        Frame(11),
        Frame(8),
        Op(KihunterSetFunctionToWinglessThinking),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(SpitLeft),
        Frame(32),
        Frame(6),
        Frame(16),
        Frame(2),
        Op(KihunterFireAcidSpitLeft),
        Frame(24),
        Op(KihunterSetFunctionToWinglessThinking),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(SpitRight),
        Frame(32),
        Frame(6),
        Frame(16),
        Frame(2),
        Op(KihunterFireAcidSpitRight),
        Frame(24),
        Op(KihunterSetFunctionToWinglessThinking),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep));
    /// <summary>Number of frame words in the compiled Ki Hunter programs that select presentation artwork.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Gets the bank-$A8 address for a frame word identified by presentation-slot index.</summary>
    /// <param name="index">Zero-based slot from zero through <see cref="PresentationWordCount"/> minus one.</param>
    /// <returns>The instruction-list address of that frame word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the presentation slots.</exception>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Reads a compiled mechanics word from one of the Ki Hunter instruction programs.</summary>
    /// <param name="address">Bank-$A8 address of the instruction word.</param>
    /// <returns>The control or timing word stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled Ki Hunter mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"KiHunter instruction mechanics pointer $A8:{address:X4} is not compiled.");
}
