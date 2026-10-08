using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The two callable left-facing Golden Torizo Chozo-orb lists at
/// $AA:CAFF-CB82. Their 20 interleaved extended-frame selectors are editable
/// presentation; callbacks, timers, loop operands, and movement functions are
/// fixed cartridge mechanics.
/// </summary>
internal abstract class GoldenTorizoLeftOrbInstructionProgramDefinitions
{
    internal const ushort Start = 0xcaff;
    internal const ushort LeftFootForward = 0xcb41;

    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FaceLeft_RightFootFwd_1</c> at $AA:CB1F.</summary>
    private const ushort TorizoSpewChozoOrbsFaceLeftRightFootFwd1 = 0xcb1f;
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrbs_FacingLeft_LeftFootFwd_1</c> at $AA:CB61.</summary>
    private const ushort TorizoSpewChozoOrbsFacingLeftLeftFootFwd1 = 0xcb61;
    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort TorizoMovementAttackingFunction = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort TorizoMovementWalkingFunction = 0xd5f1;

    /// <summary>Native program bank $AA.</summary>
    internal const byte Bank = 0xaa;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xcaff),
        Entry(Start),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementAttackingFunction),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0006),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0006),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TorizoSpewChozoOrbsFaceLeftRightFootFwd1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return),
        Entry(LeftFootForward),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementAttackingFunction),
        Frame(6),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(6),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0006),
        Op(TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        Op(TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        Op(CommonEnemyInstructionCodes.WaitFrames, 0x0006),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, TorizoSpewChozoOrbsFacingLeftLeftFootFwd1),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op(TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, TorizoMovementWalkingFunction),
        Op(TorizoInstructionCodes.Instruction_Torizo_Return));
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);
}
