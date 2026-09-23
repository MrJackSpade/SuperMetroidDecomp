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
    /// <summary>
    /// <c>InstList_Cacatac_UpsideUp_Idling</c> at $A2:9E8A-$A2:9EAF.
    /// Its $A095 moving-left/right instruction precedes eight animation durations:
    /// the word at $9E8C + 4*i is exactly $0008 for i = 0..7. Each duration is
    /// followed by a live spritemap operand. The $80ED goto at $9EAC targets
    /// $9E8A, so every idle cycle re-executes the moving-left/right instruction.
    /// </summary>
    internal const ushort UpsideUpIdle = 0x9e8a;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideUp_Attacking</c> at $A2:9EB0-$A2:9ED9.
    /// Four opening duration words at base + 4*i are $0015, $0005, $0015,
    /// $0005 for i = 0..3, each followed by a live spritemap operand.
    /// At base + $10 the $9F2A sound command precedes five $A0A7 spike
    /// commands at base + $12 + 4*i for i = 0..4. Their authored direction
    /// selectors are left-facing-up, up-left, up, up-right, right-facing-up.
    /// The $80ED goto at base + $26 returns to <see cref="UpsideUpIdle"/>.
    /// </summary>
    internal const ushort UpsideUpAttack = 0x9eb0;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Idling_0</c> at $A2:9EDA-$A2:9EFF.
    /// Its initial $A095 moving-left/right instruction is executed once before
    /// the eight-pose loop. The duration word at $9EDC + 4*i is exactly $0008
    /// for i = 0..7, with a live spritemap operand after each duration.
    /// The $80ED goto at $9EFC targets <see cref="UpsideDownIdleLoop"/>.
    /// </summary>
    internal const ushort UpsideDownIdle = 0x9eda;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Idling_1</c> at $A2:9EDC starts the
    /// inverted idle loop after its one-time moving-left/right instruction.
    /// This differs from the upright loop target $9E8A.
    /// </summary>
    internal const ushort UpsideDownIdleLoop = 0x9edc;
    /// <summary>
    /// <c>InstList_Cacatac_UpsideDown_Attacking</c> at $A2:9F00-$A2:9F29.
    /// It has the same four-duration, sound, and five-spike command layout as
    /// the upright list. Its authored direction selectors are left-facing-down,
    /// down-left, down, down-right, right-facing-down. The $80ED goto at
    /// base + $26 returns to <see cref="UpsideDownIdle"/>.
    /// </summary>
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

    /// <summary>
    /// Live spritemap operand addresses in the four $A2 Cacatac lists.
    /// In the two idle lists, pose i = 0..7 uses address $9E8E + 4*i
    /// upright or $9EDE + 4*i inverted. The pinned cartridge stores
    /// spritemap pointer $A0BB + $20*i or $A223 + $20*i respectively;
    /// corresponding inverted pointers are $0168 above upright pointers.
    /// These operands remain live presentation data, not compiled mechanics.
    /// </summary>
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
