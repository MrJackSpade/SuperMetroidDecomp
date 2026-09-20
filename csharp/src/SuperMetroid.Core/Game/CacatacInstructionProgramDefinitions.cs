namespace SuperMetroid.Core.Game;

internal readonly record struct CacatacInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Cacatac's upright and inverted idle/attack programs.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CacatacInstructionProgramDefinitions
{
    /// <summary><c>InstList_Cacatac_UpsideUp_Idling</c> at $A2:9E8A.</summary>
    internal const ushort UpsideUpIdle = 0x9e8a;
    /// <summary><c>InstList_Cacatac_UpsideUp_Attacking</c> at $A2:9EB0.</summary>
    internal const ushort UpsideUpAttack = 0x9eb0;
    /// <summary><c>InstList_Cacatac_UpsideDown_Idling_0</c> at $A2:9EDA.</summary>
    internal const ushort UpsideDownIdle = 0x9eda;
    /// <summary><c>InstList_Cacatac_UpsideDown_Idling_1</c> at $A2:9EDC.</summary>
    internal const ushort UpsideDownIdleLoop = 0x9edc;
    /// <summary><c>InstList_Cacatac_UpsideDown_Attacking</c> at $A2:9F00.</summary>
    internal const ushort UpsideDownAttack = 0x9f00;

    private static readonly CacatacInstructionMechanicsWord[] Words =
    [
        new(0x9e8a, EnemyInstructionCodePointers.Instruction_Cacatac_SetFunction_MovingLeftRight),
        new(0x9e8c, 8), new(0x9e90, 8), new(0x9e94, 8), new(0x9e98, 8),
        new(0x9e9c, 8), new(0x9ea0, 8), new(0x9ea4, 8), new(0x9ea8, 8),
        new(0x9eac, CommonEnemyInstructionCodes.Goto),
        new(0x9eae, UpsideUpIdle),

        new(0x9eb0, 0x0015), new(0x9eb4, 5),
        new(0x9eb8, 0x0015), new(0x9ebc, 5),
        new(0x9ec0, EnemyInstructionCodePointers.Instruction_Cacatac_PlaySpikesSFX),
        new(0x9ec2, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9ec4, (ushort)CacatacSpikeDirection.LeftFacingUp),
        new(0x9ec6, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9ec8, (ushort)CacatacSpikeDirection.UpLeft),
        new(0x9eca, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9ecc, (ushort)CacatacSpikeDirection.Up),
        new(0x9ece, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9ed0, (ushort)CacatacSpikeDirection.UpRight),
        new(0x9ed2, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9ed4, (ushort)CacatacSpikeDirection.RightFacingUp),
        new(0x9ed6, CommonEnemyInstructionCodes.Goto),
        new(0x9ed8, UpsideUpIdle),

        new(0x9eda, EnemyInstructionCodePointers.Instruction_Cacatac_SetFunction_MovingLeftRight),
        new(0x9edc, 8), new(0x9ee0, 8), new(0x9ee4, 8), new(0x9ee8, 8),
        new(0x9eec, 8), new(0x9ef0, 8), new(0x9ef4, 8), new(0x9ef8, 8),
        new(0x9efc, CommonEnemyInstructionCodes.Goto),
        new(0x9efe, UpsideDownIdleLoop),

        new(0x9f00, 0x0015), new(0x9f04, 5),
        new(0x9f08, 0x0015), new(0x9f0c, 5),
        new(0x9f10, EnemyInstructionCodePointers.Instruction_Cacatac_PlaySpikesSFX),
        new(0x9f12, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9f14, (ushort)CacatacSpikeDirection.LeftFacingDown),
        new(0x9f16, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9f18, (ushort)CacatacSpikeDirection.DownLeft),
        new(0x9f1a, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9f1c, (ushort)CacatacSpikeDirection.Down),
        new(0x9f1e, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9f20, (ushort)CacatacSpikeDirection.DownRight),
        new(0x9f22, EnemyInstructionCodePointers.Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY),
        new(0x9f24, (ushort)CacatacSpikeDirection.RightFacingDown),
        new(0x9f26, CommonEnemyInstructionCodes.Goto),
        new(0x9f28, UpsideDownIdle),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x9e8e, 0x9e92, 0x9e96, 0x9e9a, 0x9e9e, 0x9ea2, 0x9ea6, 0x9eaa,
        0x9eb2, 0x9eb6, 0x9eba, 0x9ebe,
        0x9ede, 0x9ee2, 0x9ee6, 0x9eea, 0x9eee, 0x9ef2, 0x9ef6, 0x9efa,
        0x9f02, 0x9f06, 0x9f0a, 0x9f0e,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CacatacInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CacatacInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Cacatac instruction mechanics pointer $A2:{address:X4} is not compiled.");
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
