namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoStunnedMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// The callable bank-$AA Golden Torizo stunned program at $AA:D193-D1E6.
/// All 28 words are control data; four existing $814B descriptors stream
/// separately installed Torizo tile art, and the list selects no sprite pose.
/// </summary>
internal static class GoldenTorizoStunnedInstructionProgramDefinitions
{
    /// <summary><c>InstList_Torizo_Stunned_0</c> at $AA:D193.</summary>
    internal const ushort Start = 0xd193;
    /// <summary><c>InstList_Torizo_Stunned_1</c> at $AA:D1A1.</summary>
    internal const ushort TileLoop = 0xd1a1;
    /// <summary>First byte after the stunned instruction list, $AA:D1E7.</summary>
    internal const ushort End = 0xd1e7;

    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort AttackingMovement = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort WalkingMovement = 0xd5f1;

    private static readonly GoldenTorizoStunnedMechanicsWord[] Words =
    [
        new(0xd193, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd195, AttackingMovement),
        new(0xd197, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xd199, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd19b, 0x0018),
        new(0xd19d, CommonEnemyInstructionCodes.SetTimer),
        new(0xd19f, 0x0002),
        new(0xd1a1, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd1a3, 0x0003),
        new(0xd1a5, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd1ae, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd1b0, 0x0003),
        new(0xd1b2, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd1bb, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd1bd, 0x0003),
        new(0xd1bf, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd1c8, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd1ca, 0x0003),
        new(0xd1cc, CommonEnemyInstructionCodes.CopyToVram),
        new(0xd1d5, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xd1d7, TileLoop),
        new(0xd1d9, CommonEnemyInstructionCodes.WaitFrames),
        new(0xd1db, 0x0010),
        new(0xd1dd, TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock),
        new(0xd1df, TorizoInstructionCodes.Instruction_GoldenTorizo_UnmarkStunned),
        new(0xd1e1, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xd1e3, WalkingMovement),
        new(0xd1e5, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoStunnedMechanicsWord MechanicsWord(int index) =>
        Words[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoStunnedMechanicsWord word in Words)
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
        foreach (GoldenTorizoStunnedMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
