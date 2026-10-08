using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Magdollite's head, pillar, and hand programs.
/// Interleaved spritemap selectors are compiled separately from editable OAM frames.
/// </summary>
internal abstract class MagdolliteInstructionProgramDefinitions
{
    /// <summary><c>InstList_Magdollite_Idling_FacingLeft</c> at $A8:AC9C.</summary>
    internal const ushort LeftIdle = 0xac9c;
    /// <summary><c>InstList_Magdollite_Slave2_ThrowFireballs_FacingLeft</c> at $A8:ACB0.</summary>
    internal const ushort LeftThrow = 0xacb0;
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Left_0</c> at $A8:ACDE.</summary>
    internal const ushort LeftSubmerge = 0xacde;
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Left_1</c> at $A8:ACFE.</summary>
    internal const ushort LeftRaisePillar = 0xacfe;
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Left_0</c> at $A8:AD0C.</summary>
    internal const ushort LeftEmerge = 0xad0c;
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Left_1</c> at $A8:AD14.</summary>
    internal const ushort LeftLowerPillar = 0xad14;
    /// <summary><c>InstList_Magdollite_Idling_FacingRight</c> at $A8:AD3C.</summary>
    internal const ushort RightIdle = 0xad3c;
    /// <summary><c>InstList_Magdollite_ThrowFireballs_FacingRight</c> at $A8:AD50.</summary>
    internal const ushort RightThrow = 0xad50;
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Right_0</c> at $A8:AD7E.</summary>
    internal const ushort RightSubmerge = 0xad7e;
    /// <summary><c>InstList_Magdollite_SplashIntoLavaAndFormBasePillar_Right_1</c> at $A8:AD9E.</summary>
    internal const ushort RightRaisePillar = 0xad9e;
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Right_0</c> at $A8:ADAC.</summary>
    internal const ushort RightEmerge = 0xadac;
    /// <summary><c>InstList_Magdollite_UnformBasePillar_SplashBackToIdle_Right_1</c> at $A8:ADB4.</summary>
    internal const ushort RightLowerPillar = 0xadb4;
    /// <summary><c>InstList_Magdollite_Slave1_NarrowPillar_FacingLeft</c> at $A8:ADDC.</summary>
    internal const ushort PillarPhase0 = 0xaddc;
    /// <summary><c>InstList_Magdollite_Slave1_NarrowPillar_FacingRight</c> at $A8:ADE2.</summary>
    internal const ushort PillarPhase1 = 0xade2;
    /// <summary><c>InstList_Magdollite_Slave1_3xPillarStack</c> at $A8:ADE8.</summary>
    internal const ushort PillarPhase2 = 0xade8;
    /// <summary><c>InstList_Magdollite_Slave1_4xPillarStack</c> at $A8:ADEE.</summary>
    internal const ushort PillarPhase3 = 0xadee;
    /// <summary><c>InstList_Magdollite_Slave1_5xPillarStack</c> at $A8:ADF4.</summary>
    internal const ushort PillarPhase4 = 0xadf4;
    /// <summary><c>InstList_Magdollite_Slave1_6xPillarStack</c> at $A8:ADFA.</summary>
    internal const ushort PillarPhase5 = 0xadfa;
    /// <summary><c>InstList_Magdollite_Slave1_7xPillarStack</c> at $A8:AE00.</summary>
    internal const ushort PillarPhase6 = 0xae00;
    /// <summary><c>InstList_Magdollite_Slave1_8xPillarStack</c> at $A8:AE06.</summary>
    internal const ushort PillarPhase7 = 0xae06;
    /// <summary><c>InstList_Magdollite_Slave2_PillarCap</c> at $A8:AE0C.</summary>
    internal const ushort PillarCap = 0xae0c;

    /// <summary>Native program bank $A8.</summary>
    internal const byte Bank = 0xa8;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xac9c),
        Entry(LeftIdle),
        Frame(13),
        Frame(13),
        Frame(13),
        Frame(13),
        Op(CommonEnemyInstructionCodes.Goto, LeftIdle),
        Entry(LeftThrow),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen, 0x0061),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetCooldownTimerTo100),
        Frame(26),
        Frame(8),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LeftSubmerge),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0018),
        Entry(LeftRaisePillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel),
        Frame(1),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, LeftRaisePillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(LeftEmerge),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_RestoreInitialYPositions),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0018),
        Entry(LeftLowerPillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel),
        Frame(1),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, LeftLowerPillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(RightIdle),
        Frame(13),
        Frame(13),
        Frame(13),
        Frame(13),
        Op(CommonEnemyInstructionCodes.Goto, RightIdle),
        Entry(RightThrow),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen, 0x0061),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetCooldownTimerTo100),
        Frame(26),
        Frame(8),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(RightSubmerge),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0018),
        Entry(RightRaisePillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel),
        Frame(1),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, RightRaisePillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(RightEmerge),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_RestoreInitialYPositions),
        Op(CommonEnemyInstructionCodes.SetTimer, 0x0018),
        Entry(RightLowerPillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel),
        Frame(1),
        Op(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, RightLowerPillar),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        Frame(5),
        Frame(5),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        Frame(5),
        Op(MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase0),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase1),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase2),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase3),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase4),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase5),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase6),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarPhase7),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(PillarCap),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep));

    /// <summary>Whether an address is one of the 53 authored visual operands.</summary>
    internal static bool IsPresentationWord(ushort address) => Layout.IsPresentationWord(address);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Magdollite instruction mechanics pointer $A8:{address:X4} is not compiled.");
}
