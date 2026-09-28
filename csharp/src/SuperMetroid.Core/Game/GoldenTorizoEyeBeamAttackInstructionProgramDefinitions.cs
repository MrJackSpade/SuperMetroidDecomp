namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoEyeBeamAttackMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The callable Golden Torizo eye-beam attack at $AA:D10D-D17A. This list has
/// no spritemap operands: it times and spawns projectiles while four existing
/// $814B descriptors stream editable Torizo character art into VRAM.
/// </summary>
internal static class GoldenTorizoEyeBeamAttackInstructionProgramDefinitions
{
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_0</c> at $AA:D10D.</summary>
    internal const ushort Start = 0xd10d;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_1</c> at $AA:D11F.</summary>
    internal const ushort SpawnLoop = 0xd11f;
    /// <summary><c>InstList_GoldenTorizo_EyeBeamAttack_2</c> at $AA:D133.</summary>
    internal const ushort TileLoop = 0xd133;
    /// <summary>The first byte after the callable list, $AA:D17B.</summary>
    internal const ushort End = 0xd17b;

    private static readonly GoldenTorizoEyeBeamAttackMechanicsWord[] Words =
    [
        new(0xd10d, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd10f, 0xd5df),
        new(0xd111, TorizoInstructionCodes.Instruction_GoldenTorizo_DisableEyeBeamExplosions),
        new(0xd113, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xd115, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd117, 0x0008),
        new(0xd119, CommonEnemyInstructionCodes.SetTimer),
        new(0xd11b, 0x0004),
        new(0xd11d, TorizoInstructionCodes.Instruction_GoldenTorizo_QueueLaserSFX),
        new(0xd11f, TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnEyeBeam),
        new(0xd121, 0x0000),
        new(0xd123, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd125, 0x0004),
        new(0xd127, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xd129, SpawnLoop),
        new(0xd12b, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd12d, 0x0008),
        new(0xd12f, CommonEnemyInstructionCodes.SetTimer),
        new(0xd131, 0x0002),
        new(0xd133, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd135, 0x0003),
        new(0xd137, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd140, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd142, 0x0003),
        new(0xd144, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd14d, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd14f, 0x0003),
        new(0xd151, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd15a, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd15c, 0x0003),
        new(0xd15e, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd167, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xd169, TileLoop),
        new(0xd16b, TorizoInstructionCodes.Instruction_GoldenTorizo_EnableEyeBeamExplosions),
        new(0xd16d, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd16f, 0x0008),
        new(0xd171, TorizoInstructionCodes.Instruction_GoldenTorizo_DisableEyeBeamExplosions),
        new(0xd173, TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        new(0xd175, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xd177, 0xd5e6),
        new(0xd179, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoEyeBeamAttackMechanicsWord MechanicsWord(int index) =>
        Words[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoEyeBeamAttackMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoEyeBeamAttackMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
