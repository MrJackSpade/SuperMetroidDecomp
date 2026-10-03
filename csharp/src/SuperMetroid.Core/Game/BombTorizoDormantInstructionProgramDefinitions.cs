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

    internal static int MechanicsWordCount => 6;
    internal static BombTorizoDormantMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Initial + 2 * index + (index >= 3 ? 2 : 0));
        _ = TryReadMechanicsWord(address, out ushort value);
        return new(address, value);
    }
    internal static int PresentationWordCount => 1;
    internal static ushort PresentationWordAddress(int index) => index == 0
        ? DormantFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        // Establish foot state and animation lock, show the dormant frame, then install
        // the hand-crumble wake function and sleep. The interleaved frame is not control.
        value = address switch
        {
            Initial => TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState,
            Initial + 2 => TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock,
            Initial + 4 => 1,
            DormantFrameOperand + 2 => TorizoInstructionCodes.Instruction_Torizo_FunctionInY,
            DormantFrameOperand + 4 => WakeWhenHandCrumbles,
            Sleep => CommonEnemyInstructionCodes.Sleep,
            _ => 0,
        };
        return value != 0;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        int offset = unchecked((ushort)address) - Initial;
        return (uint)offset < 14 && (offset < 6 || offset >= 8);
    }
}
