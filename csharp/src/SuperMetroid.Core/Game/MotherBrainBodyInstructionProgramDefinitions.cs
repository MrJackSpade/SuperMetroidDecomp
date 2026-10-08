using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled mechanics words from Mother Brain's bank-$A9 body animation programs.
/// </summary>
/// <remarks>
/// The native streams interleave mechanics with presentation. Command words and frame
/// durations control body movement, pose, footsteps, and AI-visible timing, so they belong
/// to the translated mechanics model. The word following each duration is an extended
/// spritemap pointer and belongs to presentation. The initial dummy's fixed visual
/// selector is compiled separately for installed play. Editable display bindings
/// select installed OAM/BG2 art without changing these physical frame identities.
/// </remarks>
internal static class MotherBrainBodyInstructionProgramDefinitions
{
    /// <summary>$A9:9C13, <c>InstList_MotherBrainHead_InitialDummy</c>.</summary>
    internal const ushort InitialDummy = 0x9c13;
    /// <summary>
    /// The dummy list's presentation operand at $A9:9C15 selects extended
    /// frame $A9:A320. It is an authored visual identity, not a timer or opcode.
    /// </summary>
    internal const ushort InitialDummyVisualOperand = 0x9c15;
    private const ushort InitialDummyVisualFrame = 0xa320;

    /// <summary>Resolves the fixed initial dummy selector without reading the ROM.</summary>
    internal static ushort ReadInitialDummyVisualSelector(ushort address) =>
        address == InitialDummyVisualOperand ? InitialDummyVisualFrame :
        throw new InvalidDataException(
            $"Mother Brain initial dummy visual operand $A9:{address:X4} is not compiled.");

    /// <summary>
    /// Selects the compiled extended spritemap following one timed body frame in
    /// $A9:9730-$9A43 or the initial dummy at $A9:9C15. Repeated walk speeds share the
    /// same named drawing sequence; only their frame durations differ.
    /// </summary>
    internal static ushort ReadVisualSelector(ushort address) =>
        Layout.IsPresentationWord(address) && Layout.TryReadWord(address, out ushort frame)
            ? frame : throw UnknownVisual(address);

    private static InvalidDataException UnknownVisual(ushort address) => new(
        $"Mother Brain body visual operand $A9:{address:X4} is not compiled.");

    /// <summary>$A9:9730-$A9:9851, five forward-walk programs.</summary>
    private const ushort ForwardWalkReallyFast = 0x9730;

    /// <summary>$A9:9852-$A9:9973, five backward-walk programs.</summary>
    private const ushort BackwardWalkSlow = 0x9852;

    /// <summary>$A9:9974, InstList_MotherBrainBody_CrouchAndThenStandUp.</summary>
    private const ushort CrouchAndThenStandUp = 0x9974;

    /// <summary>$A9:99AA, InstList_MotherBrainBody_StandingUpAfterCrouching_Slow.</summary>
    private const ushort StandAfterCrouchSlow = 0x99aa;

    /// <summary>$A9:99C6, InstList_MotherBrainBody_StandingUpAfterCrouching_Fast.</summary>
    private const ushort StandAfterCrouchFast = 0x99c6;

    /// <summary>$A9:99E2, InstList_MotherBrainBody_StandingUpAfterLeaningDown.</summary>
    private const ushort StandAfterLeaning = 0x99e2;

    /// <summary>$A9:99F2, InstList_MotherBrainBody_LeaningDown.</summary>
    private const ushort LeanDown = 0x99f2;

    /// <summary>$A9:9A02, InstList_MotherBrainBody_Crouched.</summary>
    private const ushort Crouched = 0x9a02;

    /// <summary>$A9:9A0A, InstList_MotherBrainBody_Crouch_Slow.</summary>
    private const ushort CrouchSlow = 0x9a0a;

    /// <summary>$A9:9A26, InstList_MotherBrainBody_Crouch_Fast.</summary>
    private const ushort CrouchFast = 0x9a26;

    /// <summary>Native program bank $A9.</summary>
    internal const byte Bank = 0xa9;

    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Fast</c> at $A9:976A.</summary>
    private const ushort InstList_MotherBrainBody_WalkingForwards_Fast = 0x976a;
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Medium</c> at $A9:97A4.</summary>
    private const ushort InstList_MotherBrainBody_WalkingForwards_Medium = 0x97a4;
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Slow</c> at $A9:97DE.</summary>
    private const ushort InstList_MotherBrainBody_WalkingForwards_Slow = 0x97de;
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallySlow</c> at $A9:9818.</summary>
    private const ushort InstList_MotherBrainBody_WalkingForwards_ReallySlow = 0x9818;
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallyFast</c> at $A9:988C.</summary>
    private const ushort InstList_MotherBrainBody_WalkingBackwards_ReallyFast = 0x988c;
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Fast</c> at $A9:98C6.</summary>
    private const ushort InstList_MotherBrainBody_WalkingBackwards_Fast = 0x98c6;
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Medium</c> at $A9:9900.</summary>
    private const ushort InstList_MotherBrainBody_WalkingBackwards_Medium = 0x9900;
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallySlow</c> at $A9:993A.</summary>
    private const ushort InstList_MotherBrainBody_WalkingBackwards_ReallySlow = 0x993a;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Standing</c> at $A9:9FA0.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Standing = 0x9fa0;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_0</c> at $A9:9FEA.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_0 = 0x9fea;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_1</c> at $A9:A03C.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_1 = 0xa03c;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_2</c> at $A9:A08E.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_2 = 0xa08e;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_3</c> at $A9:A0E0.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_3 = 0xa0e0;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_4</c> at $A9:A12A.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_4 = 0xa12a;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_5</c> at $A9:A174.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_5 = 0xa174;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_6</c> at $A9:A1BE.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_6 = 0xa1be;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Walking_7</c> at $A9:A208.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Walking_7 = 0xa208;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Crouched</c> at $A9:A252.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Crouched = 0xa252;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_Uncrouching</c> at $A9:A28C.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_Uncrouching = 0xa28c;
    /// <summary><c>ExtendedSpritemap_MotherBrainBody_LeaningDown</c> at $A9:A2D6.</summary>
    private const ushort ExtendedSpritemap_MotherBrainBody_LeaningDown = 0xa2d6;

    /// <summary>
    /// Body programs in native order: named pose and movement instructions, frames with their
    /// compiled extended-spritemap identity, and sleep terminators. Durations are authored walk,
    /// crouch and stand cadence (reviewed under #1165); speed variants differ only in them.
    /// </summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0x9730),
        Entry(ForwardWalkReallyFast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingForwards_Fast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingForwards_Medium),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingForwards_Slow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingForwards_ReallySlow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(BackwardWalkSlow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingBackwards_ReallyFast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingBackwards_Fast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(4, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingBackwards_Medium),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(6, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InstList_MotherBrainBody_WalkingBackwards_ReallySlow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_7),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_6),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_5),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_3),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_1),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Walking_0),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Frame(10, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(CrouchAndThenStandUp),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(StandAfterCrouchSlow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(16, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4),
        Frame(16, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4),
        Frame(16, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2),
        Frame(16, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(StandAfterCrouchFast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(StandAfterLeaning),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LeanDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToLeaningDown),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(Crouched),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(CrouchSlow),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4),
        Frame(8, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(CrouchFast),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Standing),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4),
        Frame(2, ExtendedSpritemap_MotherBrainBody_LeaningDown),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2),
        Frame(2, ExtendedSpritemap_MotherBrainBody_Uncrouching),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2),
        Op(MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching),
        Frame(8, ExtendedSpritemap_MotherBrainBody_Crouched),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0x9c13),
        Entry(InitialDummy),
        Frame(0, InitialDummyVisualFrame),
        Op(CommonEnemyInstructionCodes.Sleep));

    /// <summary>
    /// Looks up a mechanics word while returning false for interleaved spritemap words and
    /// for instruction streams outside the translated body-program family.
    /// </summary>
    internal static bool TryGetWord(ushort address, out ushort word) =>
        Layout.TryReadMechanicsWord(address, out word);

    /// <summary>
    /// Whether an address lies in the dummy list at $A9:9C13-$9C18 that both the body and
    /// the head enemy install; the head keeps it for life, its $A320 frame being its hitbox.
    /// </summary>
    internal static bool IsInitialDummyWord(ushort address) =>
        address is >= InitialDummy and < InitialDummy + 6;

    /// <summary>
    /// Reads one compiled mechanics word and rejects presentation or foreign addresses.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address) =>
        TryGetWord(address, out ushort word) ? word :
            throw new InvalidDataException(
                $"Mother Brain body instruction mechanics pointer $A9:{address:X4} is not compiled.");
}
