using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Bosses.Torizo;

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


    /// <summary><c>WakeBT_WhenChozoIsCrumbled</c> at $AA:C6C6.</summary>
    internal const ushort WakeWhenHandCrumbles = 0xc6c6;

    /// <summary>Counts six control words before Bomb Torizo's first sleep.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Returns an indexed dormant control word while skipping the interleaved frame operand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Initial + 2 * index + (index >= 3 ? 2 : 0));
        _ = TryReadMechanicsWord(address, out ushort value);
        return new(address, value);
    }
    /// <summary>Counts the single extended frame selected by the dormant entry.</summary>
    public static int PresentationWordCount => 1;
    /// <summary>Locates the dormant extended-spritemap operand and rejects other indices.</summary>
    public static ushort PresentationWordAddress(int index) => index == 0
        ? DormantFrameOperand
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Reads compiled foot-state, animation-lock, wake-function, and sleep words in the bounded entry.</summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        // Establish foot state and animation lock, show the dormant frame, then install
        // the hand-crumble wake function and sleep. The interleaved frame is not control.
        // Word positions are byte offsets from Initial: the frame operand sits at +6
        // (DormantFrameOperand) and the sleep at +12 (Sleep).
        value = (address - Initial) switch
        {
            0 => (ushort)TorizoInstruction.Instruction_Torizo_SetSteppedLeftWithRightFootState,
            2 => (ushort)TorizoInstruction.Instruction_Torizo_SetAnimationLock,
            4 => 1,
            8 => (ushort)TorizoInstruction.Instruction_Torizo_FunctionInY,
            10 => WakeWhenHandCrumbles,
            12 => (ushort)CommonEnemyInstruction.Sleep,
            _ => 0,
        };
        return value != 0;
    }

    /// <summary>Accepts the dormant control bytes in bank $AA while excluding the frame operand.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000) return false;
        int offset = unchecked((ushort)address) - Initial;
        return (uint)offset < 14 && (offset < 6 || offset >= 8);
    }
}
