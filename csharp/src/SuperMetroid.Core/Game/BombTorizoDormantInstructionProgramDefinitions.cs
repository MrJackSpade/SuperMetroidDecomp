namespace SuperMetroid.Core.Game;

internal readonly record struct BombTorizoDormantMechanicsWord(ushort Address, ushort Value);

/// <summary>
/// The cartridge's $AA:B879 dormant Bomb Torizo entry, through its first sleep.
/// This is a bounded program segment, not a replacement for the later awakening
/// and combat lists. The selected extended frame is presentation, not mechanics.
/// </summary>
internal static class BombTorizoDormantInstructionProgramDefinitions
{
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_0</c> at $AA:B879.</summary>
    internal const ushort Initial = 0xb879;

    /// <summary>The first extended-spritemap operand at $AA:B87F.</summary>
    internal const ushort DormantFrameOperand = 0xb87f;

    /// <summary>The bank-$AA common sleep instruction at $AA:B885.</summary>
    internal const ushort Sleep = 0xb885;

    /// <summary><c>WakeBT_WhenChozoIsCrumbled</c> at $AA:C6C6.</summary>
    internal const ushort WakeWhenHandCrumbles = 0xc6c6;

    private static readonly BombTorizoDormantMechanicsWord[] Words =
    [
        new(Initial, TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState),
        new(0xb87b, TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock),
        new(0xb87d, 1),
        new(0xb881, TorizoInstructionCodes.Instruction_Torizo_FunctionInY),
        new(0xb883, WakeWhenHandCrumbles),
        new(Sleep, CommonEnemyInstructionCodes.Sleep),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static BombTorizoDormantMechanicsWord MechanicsWord(int index) => Words[index];
    internal static int PresentationWordCount => 1;
    internal static ushort PresentationWordAddress(int index) => index == 0
        ? DormantFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (BombTorizoDormantMechanicsWord word in Words)
        {
            if (word.Address == address)
            {
                value = word.Value;
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (BombTorizoDormantMechanicsWord word in Words)
        {
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
