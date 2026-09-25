namespace SuperMetroid.Core.Game;

internal readonly record struct MagdolliteInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Magdollite's head, pillar, and hand programs.
/// Interleaved spritemap selectors are compiled separately from editable OAM frames.
/// </summary>
internal static class MagdolliteInstructionProgramDefinitions
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

    private static readonly MagdolliteInstructionMechanicsWord[] Words =
    [
        new(0xac9c, 13), new(0xaca0, 13), new(0xaca4, 13), new(0xaca8, 13),
        new(0xacac, CommonEnemyInstructionCodes.Goto), new(0xacae, LeftIdle),

        new(0xacb0, MagdolliteInstructionCodes.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen),
        new(0xacb2, 0x0061),
        new(0xacb4, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xacb6, MagdolliteInstructionCodes.Instruction_Magdollite_SetCooldownTimerTo100),
        new(0xacb8, 0x001a), new(0xacbc, 8),
        new(0xacc0, MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft),
        new(0xacc2, 5),
        new(0xacc6, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xacc8, 5),
        new(0xaccc, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xacce, 5),
        new(0xacd2, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xacd4, MagdolliteInstructionCodes.Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup),
        new(0xacd6, 5),
        new(0xacda, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xacdc, CommonEnemyInstructionCodes.Sleep),

        new(0xacde, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xace0, 5),
        new(0xace4, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        new(0xace6, 5), new(0xacea, 5), new(0xacee, 5),
        new(0xacf2, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        new(0xacf4, 5),
        new(0xacf8, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible),
        new(0xacfa, CommonEnemyInstructionCodes.SetTimer), new(0xacfc, 0x0018),
        new(0xacfe, MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel),
        new(0xad00, 1),
        new(0xad04, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xad06, LeftRaisePillar),
        new(0xad08, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xad0a, CommonEnemyInstructionCodes.Sleep),

        new(0xad0c, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xad0e, MagdolliteInstructionCodes.Instruction_Magdollite_RestoreInitialYPositions),
        new(0xad10, CommonEnemyInstructionCodes.SetTimer), new(0xad12, 0x0018),
        new(0xad14, MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel),
        new(0xad16, 1),
        new(0xad1a, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xad1c, LeftLowerPillar),
        new(0xad1e, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible),
        new(0xad20, 5),
        new(0xad24, MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        new(0xad26, 5), new(0xad2a, 5), new(0xad2e, 5),
        new(0xad32, MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        new(0xad34, 5),
        new(0xad38, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xad3a, CommonEnemyInstructionCodes.Sleep),

        new(0xad3c, 13), new(0xad40, 13), new(0xad44, 13), new(0xad48, 13),
        new(0xad4c, CommonEnemyInstructionCodes.Goto), new(0xad4e, RightIdle),

        new(0xad50, MagdolliteInstructionCodes.Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen),
        new(0xad52, 0x0061),
        new(0xad54, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xad56, MagdolliteInstructionCodes.Instruction_Magdollite_SetCooldownTimerTo100),
        new(0xad58, 0x001a), new(0xad5c, 8),
        new(0xad60, MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight),
        new(0xad62, 5),
        new(0xad66, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xad68, 5),
        new(0xad6c, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xad6e, 5),
        new(0xad72, MagdolliteInstructionCodes.Instruction_Magdollite_SpawnLavaProjectile),
        new(0xad74, MagdolliteInstructionCodes.Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup),
        new(0xad76, 5),
        new(0xad7a, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xad7c, CommonEnemyInstructionCodes.Sleep),

        new(0xad7e, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xad80, 5),
        new(0xad84, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        new(0xad86, 5), new(0xad8a, 5), new(0xad8e, 5),
        new(0xad92, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown2Pixels),
        new(0xad94, 5),
        new(0xad98, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible),
        new(0xad9a, CommonEnemyInstructionCodes.SetTimer), new(0xad9c, 0x0018),
        new(0xad9e, MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarUp1Pixel),
        new(0xada0, 1),
        new(0xada4, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xada6, RightRaisePillar),
        new(0xada8, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xadaa, CommonEnemyInstructionCodes.Sleep),

        new(0xadac, MagdolliteInstructionCodes.Instruction_Magdollite_SetWaitingFlag),
        new(0xadae, MagdolliteInstructionCodes.Instruction_Magdollite_RestoreInitialYPositions),
        new(0xadb0, CommonEnemyInstructionCodes.SetTimer), new(0xadb2, 0x0018),
        new(0xadb4, MagdolliteInstructionCodes.Instruction_Magdollite_MoveBaseAndPillarDown1Pixel),
        new(0xadb6, 1),
        new(0xadba, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xadbc, RightLowerPillar),
        new(0xadbe, MagdolliteInstructionCodes.Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible),
        new(0xadc0, 5),
        new(0xadc4, MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        new(0xadc6, 5), new(0xadca, 5), new(0xadce, 5),
        new(0xadd2, MagdolliteInstructionCodes.Instruction_Magdollite_MoveUp2Pixels),
        new(0xadd4, 5),
        new(0xadd8, MagdolliteInstructionCodes.Instruction_Magdollite_ResetWaitingFlag),
        new(0xadda, CommonEnemyInstructionCodes.Sleep),

        new(0xaddc, 1), new(0xade0, CommonEnemyInstructionCodes.Sleep),
        new(0xade2, 1), new(0xade6, CommonEnemyInstructionCodes.Sleep),
        new(0xade8, 1), new(0xadec, CommonEnemyInstructionCodes.Sleep),
        new(0xadee, 1), new(0xadf2, CommonEnemyInstructionCodes.Sleep),
        new(0xadf4, 1), new(0xadf8, CommonEnemyInstructionCodes.Sleep),
        new(0xadfa, 1), new(0xadfe, CommonEnemyInstructionCodes.Sleep),
        new(0xae00, 1), new(0xae04, CommonEnemyInstructionCodes.Sleep),
        new(0xae06, 1), new(0xae0a, CommonEnemyInstructionCodes.Sleep),
        new(0xae0c, 1), new(0xae10, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xac9e, 0xaca2, 0xaca6, 0xacaa,
        0xacba, 0xacbe, 0xacc4, 0xacca, 0xacd0, 0xacd8,
        0xace2, 0xace8, 0xacec, 0xacf0, 0xacf6, 0xad02,
        0xad18, 0xad22, 0xad28, 0xad2c, 0xad30, 0xad36,
        0xad3e, 0xad42, 0xad46, 0xad4a,
        0xad5a, 0xad5e, 0xad64, 0xad6a, 0xad70, 0xad78,
        0xad82, 0xad88, 0xad8c, 0xad90, 0xad96, 0xada2,
        0xadb8, 0xadc2, 0xadc8, 0xadcc, 0xadd0, 0xadd6,
        0xadde, 0xade4, 0xadea, 0xadf0, 0xadf6, 0xadfc, 0xae02, 0xae08, 0xae0e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MagdolliteInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Whether an address is one of the 53 authored visual operands.</summary>
    internal static bool IsPresentationWord(ushort address) =>
        Array.BinarySearch(PresentationWords, address) >= 0;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MagdolliteInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Magdollite instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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
