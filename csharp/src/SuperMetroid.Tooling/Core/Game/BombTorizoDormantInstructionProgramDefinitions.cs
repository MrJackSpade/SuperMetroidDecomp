namespace SuperMetroid.Core.Game;

/// <summary>
/// The cartridge's $AA:B879 dormant Bomb Torizo entry, through its first sleep.
/// This is a bounded program segment, not a replacement for the later awakening
/// and combat lists. The selected extended frame is presentation, not mechanics.
/// </summary>
internal abstract class BombTorizoDormantInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Torizo_BombTorizo_Initial_0</c> at $AA:B879.</summary>
    internal const ushort Initial = 0xb879;

    /// <summary>The first extended-spritemap operand at $AA:B87F.</summary>
    internal const ushort DormantFrameOperand = 0xb87f;

    /// <summary>The bank-$AA common sleep instruction at $AA:B885.</summary>
    internal const ushort Sleep = 0xb885;

    /// <summary><c>WakeBT_WhenChozoIsCrumbled</c> at $AA:C6C6.</summary>
    internal const ushort WakeWhenHandCrumbles = 0xc6c6;

    /// <summary>Number of control and duration words in the bounded dormant entry through its first sleep.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>Resolves one mechanics operand while skipping the interleaved dormant-frame presentation word.</summary>
    /// <param name="index">Zero-based ordinal among the six compiled mechanics words.</param>
    /// <returns>The native address and value of the selected control or duration word.</returns>
    /// <exception cref="IndexOutOfRangeException">The ordinal is outside the dormant program segment.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Initial + 2 * index + (index >= 3 ? 2 : 0));
        _ = TryReadMechanicsWord(address, out ushort value);
        return new(address, value);
    }
    /// <summary>Number of presentation operands in the dormant program segment.</summary>
    public static int PresentationWordCount => 1;

    /// <summary>Returns the address of the dormant extended-spritemap operand.</summary>
    /// <param name="index">Presentation-word ordinal; only zero is defined.</param>
    /// <returns>Bank-$AA address of the dormant-frame presentation word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The ordinal is not zero.</exception>
    public static ushort PresentationWordAddress(int index) => index == 0
        ? DormantFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Looks up the compiled setup, wake-function, and sleep words in the dormant entry segment.</summary>
    /// <param name="address">Bank-$AA address of the candidate instruction word.</param>
    /// <param name="value">Receives the compiled operand when the address belongs to the mechanics sequence.</param>
    /// <returns><see langword="true"/> for a control or duration word; the interleaved spritemap word is excluded.</returns>
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

    /// <summary>Tests whether a full address selects a byte in this program's compiled mechanics words.</summary>
    /// <param name="address">Full cartridge address to classify.</param>
    /// <returns><see langword="true"/> for bank-$AA mechanics bytes, excluding the dormant-frame presentation word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        int offset = unchecked((ushort)address) - Initial;
        return (uint)offset < 14 && (offset < 6 || offset >= 8);
    }
}
